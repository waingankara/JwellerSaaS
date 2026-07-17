using Dapper;
using Microsoft.Extensions.Logging;
using Orion.Framework.Data;
using Orion.Framework.DomainEvents;
using Orion.Framework.Interceptors;
using Orion.Framework.Metadata;
using Orion.Framework.Query;
using Orion.Framework.Security;
using Orion.Framework.Sql;
using Orion.Framework.Tenancy;
using Orion.Framework.Validation;

namespace Orion.Framework.Crud;

/// <summary>Generic CRUD service used by future ERP modules.</summary>
public interface ICrudService<TEntity> where TEntity : class
{
    /// <summary>Creates an entity.</summary>
Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);
    /// <summary>Updates an entity.</summary>
Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    /// <summary>Deletes an entity permanently.</summary>
Task<int> DeleteAsync(object key, CancellationToken cancellationToken);
    /// <summary>Soft deletes an entity.</summary>
Task<int> SoftDeleteAsync(object key, CancellationToken cancellationToken);
    /// <summary>Restores an entity.</summary>
Task<int> RestoreAsync(object key, CancellationToken cancellationToken);
    /// <summary>Gets one entity by id.</summary>
Task<TEntity?> GetByIdAsync(object key, CancellationToken cancellationToken);
    /// <summary>Gets entities matching a query.</summary>
Task<IReadOnlyList<TEntity>> GetManyAsync(QueryDefinition query, CancellationToken cancellationToken);
    /// <summary>Checks if an entity exists.</summary>
Task<bool> ExistsAsync(object key, CancellationToken cancellationToken);
    /// <summary>Counts entities matching a query.</summary>
Task<long> CountAsync(QueryDefinition query, CancellationToken cancellationToken);
    /// <summary>Bulk inserts entities.</summary>
Task<int> BulkInsertAsync(IReadOnlyList<TEntity> entities, int batchSize, CancellationToken cancellationToken);
    /// <summary>Bulk updates entities.</summary>
Task<int> BulkUpdateAsync(IReadOnlyList<TEntity> entities, int batchSize, CancellationToken cancellationToken);
    /// <summary>Bulk deletes keys.</summary>
Task<int> BulkDeleteAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken);
    /// <summary>Bulk soft deletes keys.</summary>
Task<int> BulkSoftDeleteAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken);
    /// <summary>Bulk restores keys.</summary>
Task<int> BulkRestoreAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken);
}

/// <summary>Replaceable CRUD pipeline abstraction.</summary>
public interface ICrudPipeline {
    /// <summary>Executes a pipeline context.</summary>
Task ExecuteAsync(CrudContext context, CancellationToken cancellationToken); }

/// <summary>Default CRUD pipeline: auth, validation, metadata, tenant, audit, SQL, Dapper, events and diagnostics.</summary>
public sealed class CrudPipeline(IReflectionMetadataCache metadataCache, ISqlExecutor executor, CrudSqlBuilder sqlBuilder, IValidationPipeline? validation, ITenantContextAccessor tenantAccessor, ICurrentUserAccessor userAccessor, IDomainEventPublisher events, IEnumerable<ICrudInterceptor> interceptors, ILogger<CrudPipeline> logger) : ICrudPipeline
{
    /// <inheritdoc />
    public async Task ExecuteAsync(CrudContext context, CancellationToken cancellationToken)
    {
        var started = TimeProvider.System.GetTimestamp();
        try
        {
            context.Metadata = metadataCache.GetOrAdd(context.EntityType);
            InjectTenant(context); InjectAudit(context);
            if (context.Entity is not null && validation is not null) { var result = await validation.ValidateAsync(context.Entity, cancellationToken).ConfigureAwait(false); if (!result.IsValid) throw new InvalidOperationException(string.Join("; ", result.Failures.Select(f => f.Message))); }
            await RaiseBeforeAsync(context, cancellationToken).ConfigureAwait(false);
            BuildSql(context);
            foreach (var interceptor in interceptors) await interceptor.BeforeExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            await ExecuteSqlAsync(context, cancellationToken).ConfigureAwait(false);
            foreach (var interceptor in interceptors) await interceptor.AfterExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            await RaiseAfterAsync(context, cancellationToken).ConfigureAwait(false);
            foreach (var interceptor in interceptors) await interceptor.OnSuccessAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            foreach (var interceptor in interceptors) await interceptor.OnFailureAsync(context, ex, cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally { logger.LogInformation("CRUD {Operation} {Entity} completed in {ElapsedMs}ms. CorrelationId={CorrelationId}; Rows={Rows}; SQL={Sql}", context.Operation, context.EntityType.Name, TimeProvider.System.GetElapsedTime(started).TotalMilliseconds, context.CorrelationId, context.AffectedRows, context.Sql); }
    }
    private void BuildSql(CrudContext c) { var m = c.Metadata!; var key = m.Columns.FirstOrDefault(x => x.IsPrimaryKey); switch (c.Operation) { case CrudOperation.Create: c.Sql = new InsertBuilder().Build(m, c.Entity!).Sql; c.Parameters = c.Entity; break; case CrudOperation.Update: c.Sql = new UpdateBuilder().Build(m, c.Entity!).Sql; c.Parameters = c.Entity; break; case CrudOperation.Delete: c.Sql = new DeleteBuilder().Build(m, KeyParams(key!, c.Key)).Sql; c.Parameters = KeyParams(key!, c.Key); break; case CrudOperation.GetMany: var s = sqlBuilder.Select(m, c.Query); c.Sql = s.Sql; c.Parameters = s.Parameters; break; case CrudOperation.Count: var count = sqlBuilder.Count(m, c.Query); c.Sql = count.Sql; c.Parameters = count.Parameters; break; case CrudOperation.GetById: c.Sql = $"SELECT {string.Join(", ", m.Columns.Select(x => SqlName.Identifier(x.ColumnName)))} FROM {SqlName.Identifier(m.TableName)} WHERE {SqlName.Identifier(key!.ColumnName)} = @{key.PropertyName}"; c.Parameters = KeyParams(key, c.Key); break; case CrudOperation.Exists: c.Sql = $"SELECT EXISTS (SELECT 1 FROM {SqlName.Identifier(m.TableName)} WHERE {SqlName.Identifier(key!.ColumnName)} = @{key.PropertyName})"; c.Parameters = KeyParams(key, c.Key); break; case CrudOperation.SoftDelete: case CrudOperation.Restore: c.Sql = $"UPDATE {SqlName.Identifier(m.TableName)} SET {SqlName.Identifier(m.Audit.IsDeleted!.ColumnName)} = @IsDeleted WHERE {SqlName.Identifier(key!.ColumnName)} = @{key.PropertyName}"; c.Parameters = new DynamicParameters(KeyParams(key, c.Key)) { { "IsDeleted", c.Operation == CrudOperation.SoftDelete } }; break; } }
    private async Task ExecuteSqlAsync(CrudContext c, CancellationToken ct) { if (c.Sql is null) return; c.AffectedRows = c.Operation is CrudOperation.GetMany or CrudOperation.GetById or CrudOperation.Exists or CrudOperation.Count ? 0 : await executor.ExecuteAsync(c.Sql, c.Parameters, ct).ConfigureAwait(false); }
    private void InjectTenant(CrudContext c) { if (c.Entity is null) return; var t = metadataCache.GetOrAdd(c.EntityType).Tenant; Set(c.Entity, t.TenantId?.PropertyName, tenantAccessor.TenantContext.TenantId); Set(c.Entity, t.BranchId?.PropertyName, tenantAccessor.TenantContext.BranchId); }
    private void InjectAudit(CrudContext c) { if (c.Entity is null) return; var a = metadataCache.GetOrAdd(c.EntityType).Audit; var now = DateTimeOffset.UtcNow; var user = userAccessor.CurrentUser.UserId; if (c.Operation == CrudOperation.Create) { Set(c.Entity, a.CreatedDate?.PropertyName, now); Set(c.Entity, a.CreatedBy?.PropertyName, user); } if (c.Operation is CrudOperation.Update or CrudOperation.Create) { Set(c.Entity, a.ModifiedDate?.PropertyName, now); Set(c.Entity, a.ModifiedBy?.PropertyName, user); } }
    private static object KeyParams(ColumnDefinition key, object? value) { var p = new DynamicParameters(); p.Add(key.PropertyName, value); return p; }
    private static void Set(object entity, string? property, object? value) { if (property is null || value is null) return; var p = entity.GetType().GetProperty(property); if (p?.CanWrite == true) p.SetValue(entity, Convert.ChangeType(value, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)); }
    private Task RaiseBeforeAsync(CrudContext c, CancellationToken ct) => c.Operation switch { CrudOperation.Create => events.PublishAsync(new EntityLifecycleEvent(c.EntityType, DomainEventNames.EntityCreating, c.Entity, DateTimeOffset.UtcNow), ct), CrudOperation.Update => events.PublishAsync(new EntityLifecycleEvent(c.EntityType, DomainEventNames.EntityUpdating, c.Entity, DateTimeOffset.UtcNow), ct), CrudOperation.Delete or CrudOperation.SoftDelete => events.PublishAsync(new EntityLifecycleEvent(c.EntityType, DomainEventNames.EntityDeleting, c.Entity, DateTimeOffset.UtcNow), ct), _ => Task.CompletedTask };
    private Task RaiseAfterAsync(CrudContext c, CancellationToken ct) => c.Operation switch { CrudOperation.Create => events.PublishAsync(new EntityLifecycleEvent(c.EntityType, DomainEventNames.EntityCreated, c.Entity, DateTimeOffset.UtcNow), ct), CrudOperation.Update => events.PublishAsync(new EntityLifecycleEvent(c.EntityType, DomainEventNames.EntityUpdated, c.Entity, DateTimeOffset.UtcNow), ct), CrudOperation.Delete or CrudOperation.SoftDelete => events.PublishAsync(new EntityLifecycleEvent(c.EntityType, DomainEventNames.EntityDeleted, c.Entity, DateTimeOffset.UtcNow), ct), _ => Task.CompletedTask };
}

/// <summary>Default generic CRUD service.</summary>
public sealed class CrudService<TEntity>(ICrudPipeline pipeline, ISqlExecutor executor, IReflectionMetadataCache metadataCache) : ICrudService<TEntity> where TEntity : class
{
    /// <inheritdoc />
public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken) { await pipeline.ExecuteAsync(new CrudContext(typeof(TEntity), CrudOperation.Create, entity, null, null, cancellationToken), cancellationToken).ConfigureAwait(false); return entity; }
    /// <inheritdoc />
public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken) { await pipeline.ExecuteAsync(new CrudContext(typeof(TEntity), CrudOperation.Update, entity, null, null, cancellationToken), cancellationToken).ConfigureAwait(false); return entity; }
    /// <inheritdoc />
public Task<int> DeleteAsync(object key, CancellationToken cancellationToken) => ExecuteRowsAsync(CrudOperation.Delete, key, cancellationToken);
    /// <inheritdoc />
public Task<int> SoftDeleteAsync(object key, CancellationToken cancellationToken) => ExecuteRowsAsync(CrudOperation.SoftDelete, key, cancellationToken);
    /// <inheritdoc />
public Task<int> RestoreAsync(object key, CancellationToken cancellationToken) => ExecuteRowsAsync(CrudOperation.Restore, key, cancellationToken);
    /// <inheritdoc />
public Task<TEntity?> GetByIdAsync(object key, CancellationToken cancellationToken) { var m = metadataCache.GetOrAdd<TEntity>(); var k = m.Columns.First(c => c.IsPrimaryKey); return executor.QueryFirstAsync<TEntity>($"SELECT {string.Join(", ", m.Columns.Select(c => SqlName.Identifier(c.ColumnName)))} FROM {SqlName.Identifier(m.TableName)} WHERE {SqlName.Identifier(k.ColumnName)} = @{k.PropertyName}", Key(k, key), cancellationToken); }
    /// <inheritdoc />
public async Task<IReadOnlyList<TEntity>> GetManyAsync(QueryDefinition query, CancellationToken cancellationToken) { var ctx = new CrudContext(typeof(TEntity), CrudOperation.GetMany, null, null, query, cancellationToken); await pipeline.ExecuteAsync(ctx, cancellationToken).ConfigureAwait(false); return await executor.QueryAsync<TEntity>(ctx.Sql!, ctx.Parameters, cancellationToken).ConfigureAwait(false); }
    /// <inheritdoc />
public async Task<bool> ExistsAsync(object key, CancellationToken cancellationToken) { var m = metadataCache.GetOrAdd<TEntity>(); var k = m.Columns.First(c => c.IsPrimaryKey); return await executor.ScalarAsync<bool>($"SELECT EXISTS (SELECT 1 FROM {SqlName.Identifier(m.TableName)} WHERE {SqlName.Identifier(k.ColumnName)} = @{k.PropertyName})", Key(k, key), cancellationToken).ConfigureAwait(false); }
    /// <inheritdoc />
public async Task<long> CountAsync(QueryDefinition query, CancellationToken cancellationToken) { var builder = new CrudSqlBuilder(); var st = builder.Count(metadataCache.GetOrAdd<TEntity>(), query); return await executor.ScalarAsync<long>(st.Sql, st.Parameters, cancellationToken).ConfigureAwait(false); }
    /// <inheritdoc />
public Task<int> BulkInsertAsync(IReadOnlyList<TEntity> entities, int batchSize, CancellationToken cancellationToken) => BulkAsync(entities, e => CreateAsync(e, cancellationToken), batchSize);
    /// <inheritdoc />
public Task<int> BulkUpdateAsync(IReadOnlyList<TEntity> entities, int batchSize, CancellationToken cancellationToken) => BulkAsync(entities, e => UpdateAsync(e, cancellationToken), batchSize);
    /// <inheritdoc />
public Task<int> BulkDeleteAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken) => BulkKeysAsync(keys, DeleteAsync, batchSize, cancellationToken);
    /// <inheritdoc />
public Task<int> BulkSoftDeleteAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken) => BulkKeysAsync(keys, SoftDeleteAsync, batchSize, cancellationToken);
    /// <inheritdoc />
public Task<int> BulkRestoreAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken) => BulkKeysAsync(keys, RestoreAsync, batchSize, cancellationToken);
    private async Task<int> ExecuteRowsAsync(CrudOperation op, object key, CancellationToken ct) { var ctx = new CrudContext(typeof(TEntity), op, null, key, null, ct); await pipeline.ExecuteAsync(ctx, ct).ConfigureAwait(false); return ctx.AffectedRows; }
    private static async Task<int> BulkAsync(IReadOnlyList<TEntity> entities, Func<TEntity, Task<TEntity>> action, int batchSize) { var count = 0; foreach (var batch in entities.Chunk(Math.Max(1, batchSize))) foreach (var entity in batch) { await action(entity).ConfigureAwait(false); count++; } return count; }
    private static async Task<int> BulkKeysAsync(IReadOnlyList<object> keys, Func<object, CancellationToken, Task<int>> action, int batchSize, CancellationToken ct) { var count = 0; foreach (var batch in keys.Chunk(Math.Max(1, batchSize))) foreach (var key in batch) count += await action(key, ct).ConfigureAwait(false); return count; }
    private static object Key(ColumnDefinition key, object value) { var p = new DynamicParameters(); p.Add(key.PropertyName, value); return p; }
}
