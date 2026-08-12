using Dapper;
using Microsoft.Extensions.Logging;
using Orion.Framework.Data;
using Orion.Framework.DomainEvents;
using Orion.Framework.Interceptors;
using Orion.Framework.Metadata;
using Orion.Framework.Query;
using Orion.Framework.Search;
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
public interface ICrudPipeline
{
    /// <summary>Executes a pipeline context.</summary>
    Task ExecuteAsync(CrudContext context, CancellationToken cancellationToken);
}

/// <summary>Default CRUD pipeline: auth, validation, metadata, tenant, audit, SQL, Dapper, events and diagnostics.</summary>
public sealed class CrudPipeline(
    IReflectionMetadataCache metadataCache,
    ISqlExecutor executor,
    CrudSqlBuilder sqlBuilder,
    IValidationPipeline? validation,
    ITenantContextAccessor tenantAccessor,
    ICurrentUserAccessor userAccessor,
    IDomainEventPublisher events,
    IEnumerable<ICrudInterceptor> interceptors,
    ILogger<CrudPipeline> logger) : ICrudPipeline
{
    /// <inheritdoc />
    public async Task ExecuteAsync(CrudContext context, CancellationToken cancellationToken)
    {
        var started = TimeProvider.System.GetTimestamp();

        try
        {
            context.Metadata = metadataCache.GetOrAdd(context.EntityType);
            InjectTenant(context);
            InjectAudit(context);
            await ValidateAsync(context, cancellationToken).ConfigureAwait(false);
            await RaiseBeforeAsync(context, cancellationToken).ConfigureAwait(false);
            BuildSql(context);

            foreach (var interceptor in interceptors)
            {
                await interceptor.BeforeExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }

            await ExecuteSqlAsync(context, cancellationToken).ConfigureAwait(false);

            foreach (var interceptor in interceptors)
            {
                await interceptor.AfterExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }

            await RaiseAfterAsync(context, cancellationToken).ConfigureAwait(false);

            foreach (var interceptor in interceptors)
            {
                await interceptor.OnSuccessAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            foreach (var interceptor in interceptors)
            {
                await interceptor.OnFailureAsync(context, ex, cancellationToken).ConfigureAwait(false);
            }

            throw;
        }
        finally
        {
            logger.LogInformation(
                "CRUD {Operation} {Entity} completed in {ElapsedMs}ms. CorrelationId={CorrelationId}; Rows={Rows}; SQL={Sql}",
                context.Operation,
                context.EntityType.Name,
                TimeProvider.System.GetElapsedTime(started).TotalMilliseconds,
                context.CorrelationId,
                context.AffectedRows,
                context.Sql);
        }
    }

    private async Task ValidateAsync(CrudContext context, CancellationToken cancellationToken)
    {
        if (context.Entity is null || validation is null)
        {
            return;
        }

        var result = await validation.ValidateAsync(context.Entity, cancellationToken).ConfigureAwait(false);
        if (!result.IsValid)
        {
            throw new InvalidOperationException(string.Join("; ", result.Failures.Select(f => f.Message)));
        }
    }

    private void BuildSql(CrudContext context)
    {
        var metadata = context.Metadata!;
        var key = GetPrimaryKey(metadata);

        switch (context.Operation)
        {
            case CrudOperation.Create:
                context.Sql = new InsertBuilder().Build(metadata, context.Entity!).CommandText;
                context.Parameters = context.Entity;
                break;
            case CrudOperation.Update:
                context.Sql = new UpdateBuilder().Build(metadata, context.Entity!).CommandText;
                context.Parameters = context.Entity;
                break;
            case CrudOperation.Delete:
                context.Sql = new DeleteBuilder().Build(metadata, KeyParamsWithTenant(metadata, key, context.Key, tenantAccessor.TenantContext.TenantId)).CommandText;
                context.Parameters = KeyParamsWithTenant(metadata, key, context.Key, tenantAccessor.TenantContext.TenantId);
                break;
            case CrudOperation.GetMany:
                var select = sqlBuilder.Select(metadata, AddTenantFilter(metadata, context.Query, tenantAccessor.TenantContext.TenantId));
                context.Sql = select.CommandText;
                context.Parameters = select.Parameters;
                break;
            case CrudOperation.Count:
                var count = sqlBuilder.Count(metadata, AddTenantFilter(metadata, context.Query, tenantAccessor.TenantContext.TenantId));
                context.Sql = count.CommandText;
                context.Parameters = count.Parameters;
                break;
            case CrudOperation.GetById:
                context.Sql = $"SELECT {ColumnList(metadata)} FROM {SqlName.Identifier(metadata.TableName)} WHERE {KeyAndTenantPredicate(metadata, key)}";
                context.Parameters = KeyParamsWithTenant(metadata, key, context.Key, tenantAccessor.TenantContext.TenantId);
                break;
            case CrudOperation.Exists:
                context.Sql = $"SELECT EXISTS (SELECT 1 FROM {SqlName.Identifier(metadata.TableName)} WHERE {KeyAndTenantPredicate(metadata, key)})";
                context.Parameters = KeyParamsWithTenant(metadata, key, context.Key, tenantAccessor.TenantContext.TenantId);
                break;
            case CrudOperation.SoftDelete:
            case CrudOperation.Restore:
                BuildSoftDeleteSql(context, metadata, key, userAccessor.CurrentUser.UserId, tenantAccessor.TenantContext.TenantId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), context.Operation, "Unsupported CRUD operation.");
        }
    }

    private static void BuildSoftDeleteSql(CrudContext context, MasterDefinition metadata, ColumnDefinition key, long? currentUserId, long? tenantId)
    {
        var deletedColumn = metadata.Audit.IsDeleted
            ?? throw new InvalidOperationException($"Entity {metadata.EntityType.Name} does not define an IsDeleted audit column.");
        var assignments = new List<string> { $"{SqlName.Identifier(deletedColumn.ColumnName)} = @IsDeleted" };
        var parameters = new DynamicParameters(KeyParams(key, context.Key));
        var isDeleted = context.Operation == CrudOperation.SoftDelete;
        parameters.Add("IsDeleted", isDeleted);

        if (metadata.Audit.DeletedDate is not null)
        {
            assignments.Add($"{SqlName.Identifier(metadata.Audit.DeletedDate.ColumnName)} = @DeletedDate");
            parameters.Add("DeletedDate", isDeleted ? DateTimeOffset.UtcNow : (DateTimeOffset?)null);
        }

        if (metadata.Audit.DeletedBy is not null)
        {
            assignments.Add($"{SqlName.Identifier(metadata.Audit.DeletedBy.ColumnName)} = @DeletedBy");
            parameters.Add("DeletedBy", isDeleted ? currentUserId : null);
        }

        var tenantPredicate = metadata.Tenant.TenantId is null ? string.Empty : $" AND {SqlName.Identifier(metadata.Tenant.TenantId.ColumnName)} = @TenantId";
        if (metadata.Tenant.TenantId is not null)
        {
            parameters.Add("TenantId", tenantId);
        }

        context.Sql = $"UPDATE {SqlName.Identifier(metadata.TableName)} SET {string.Join(", ", assignments)} WHERE {SqlName.Identifier(key.ColumnName)} = @{key.PropertyName}{tenantPredicate}";
        context.Parameters = parameters;
    }

    private async Task ExecuteSqlAsync(CrudContext context, CancellationToken cancellationToken)
    {
        if (context.Sql is null || context.Operation is CrudOperation.GetMany or CrudOperation.GetById or CrudOperation.Exists or CrudOperation.Count)
        {
            return;
        }

        context.AffectedRows = await executor.ExecuteAsync(context.Sql, context.Parameters, cancellationToken).ConfigureAwait(false);
    }

    private void InjectTenant(CrudContext context)
    {
        if (context.Entity is null)
        {
            return;
        }

        var tenant = metadataCache.GetOrAdd(context.EntityType).Tenant;
        Set(context.Entity, tenant.TenantId?.PropertyName, tenantAccessor.TenantContext.TenantId);
        Set(context.Entity, tenant.BranchId?.PropertyName, tenantAccessor.TenantContext.BranchId);
    }

    private void InjectAudit(CrudContext context)
    {
        if (context.Entity is null)
        {
            return;
        }

        var audit = metadataCache.GetOrAdd(context.EntityType).Audit;
        var now = DateTimeOffset.UtcNow;
        var user = userAccessor.CurrentUser.UserId;

        if (context.Operation == CrudOperation.Create)
        {
            Set(context.Entity, audit.CreatedDate?.PropertyName, now);
            Set(context.Entity, audit.CreatedBy?.PropertyName, user);
        }

        if (context.Operation is CrudOperation.Update or CrudOperation.Create)
        {
            Set(context.Entity, audit.ModifiedDate?.PropertyName, now);
            Set(context.Entity, audit.ModifiedBy?.PropertyName, user);
        }
    }

    private Task RaiseBeforeAsync(CrudContext context, CancellationToken cancellationToken) => context.Operation switch
    {
        CrudOperation.Create => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityCreating, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        CrudOperation.Update => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityUpdating, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        CrudOperation.Delete or CrudOperation.SoftDelete => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityDeleting, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        CrudOperation.Restore => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityRestoring, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        _ => Task.CompletedTask,
    };

    private Task RaiseAfterAsync(CrudContext context, CancellationToken cancellationToken) => context.Operation switch
    {
        CrudOperation.Create => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityCreated, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        CrudOperation.Update => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityUpdated, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        CrudOperation.Delete or CrudOperation.SoftDelete => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityDeleted, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        CrudOperation.Restore => events.PublishAsync(new EntityLifecycleEvent(context.EntityType, DomainEventNames.EntityRestored, context.Entity, DateTimeOffset.UtcNow), cancellationToken),
        _ => Task.CompletedTask,
    };



    private static QueryDefinition AddTenantFilter(MasterDefinition metadata, QueryDefinition? query, long? tenantId)
    {
        var actual = query ?? QueryDefinition.Empty;
        if (metadata.Tenant.TenantId is null || tenantId is null)
        {
            return actual;
        }

        var filters = (actual.Filters ?? Array.Empty<FilterDefinition>()).ToList();
        filters.Add(new FilterDefinition(metadata.Tenant.TenantId.PropertyName, SearchOperator.Equals, tenantId.Value));
        return actual with { Filters = filters };
    }

    private static string KeyAndTenantPredicate(MasterDefinition metadata, ColumnDefinition key)
    {
        var predicate = $"{SqlName.Identifier(key.ColumnName)} = @{key.PropertyName}";
        return metadata.Tenant.TenantId is null ? predicate : $"{predicate} AND {SqlName.Identifier(metadata.Tenant.TenantId.ColumnName)} = @TenantId";
    }

    private static object KeyParamsWithTenant(MasterDefinition metadata, ColumnDefinition key, object? value, long? tenantId)
    {
        var parameters = new DynamicParameters(KeyParams(key, value));
        if (metadata.Tenant.TenantId is not null)
        {
            parameters.Add("TenantId", tenantId);
        }

        return parameters;
    }

    private static string ColumnList(MasterDefinition metadata) => string.Join(", ", metadata.Columns.Select(column => SqlName.Identifier(column.ColumnName)));

    private static ColumnDefinition GetPrimaryKey(MasterDefinition metadata) => metadata.Columns.FirstOrDefault(column => column.IsPrimaryKey)
        ?? throw new InvalidOperationException($"Entity {metadata.EntityType.Name} does not define a primary key column.");

    private static object KeyParams(ColumnDefinition key, object? value)
    {
        var parameters = new DynamicParameters();
        parameters.Add(key.PropertyName, value);
        return parameters;
    }

    private static void Set(object entity, string? property, object? value)
    {
        if (property is null || value is null)
        {
            return;
        }

        var propertyInfo = entity.GetType().GetProperty(property);
        if (propertyInfo?.CanWrite == true)
        {
            propertyInfo.SetValue(entity, Convert.ChangeType(value, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType));
        }
    }
}

/// <summary>Default generic CRUD service.</summary>
public sealed class CrudService<TEntity>(ICrudPipeline pipeline, ISqlExecutor executor) : ICrudService<TEntity> where TEntity : class
{
    /// <inheritdoc />
    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await pipeline.ExecuteAsync(new CrudContext(typeof(TEntity), CrudOperation.Create, entity, null, null, cancellationToken), cancellationToken).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await pipeline.ExecuteAsync(new CrudContext(typeof(TEntity), CrudOperation.Update, entity, null, null, cancellationToken), cancellationToken).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public Task<int> DeleteAsync(object key, CancellationToken cancellationToken) => ExecuteRowsAsync(CrudOperation.Delete, key, cancellationToken);

    /// <inheritdoc />
    public Task<int> SoftDeleteAsync(object key, CancellationToken cancellationToken) => ExecuteRowsAsync(CrudOperation.SoftDelete, key, cancellationToken);

    /// <inheritdoc />
    public Task<int> RestoreAsync(object key, CancellationToken cancellationToken) => ExecuteRowsAsync(CrudOperation.Restore, key, cancellationToken);

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(object key, CancellationToken cancellationToken)
    {
        var context = await BuildReadContextAsync(CrudOperation.GetById, key, null, cancellationToken).ConfigureAwait(false);
        return await executor.QueryFirstAsync<TEntity>(context.Sql!, context.Parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetManyAsync(QueryDefinition query, CancellationToken cancellationToken)
    {
        var context = await BuildReadContextAsync(CrudOperation.GetMany, null, query, cancellationToken).ConfigureAwait(false);
        return await executor.QueryAsync<TEntity>(context.Sql!, context.Parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(object key, CancellationToken cancellationToken)
    {
        var context = await BuildReadContextAsync(CrudOperation.Exists, key, null, cancellationToken).ConfigureAwait(false);
        return await executor.ScalarAsync<bool>(context.Sql!, context.Parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<long> CountAsync(QueryDefinition query, CancellationToken cancellationToken)
    {
        var context = await BuildReadContextAsync(CrudOperation.Count, null, query, cancellationToken).ConfigureAwait(false);
        return await executor.ScalarAsync<long>(context.Sql!, context.Parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<int> BulkInsertAsync(IReadOnlyList<TEntity> entities, int batchSize, CancellationToken cancellationToken) => BulkAsync(entities, entity => CreateAsync(entity, cancellationToken), batchSize);

    /// <inheritdoc />
    public Task<int> BulkUpdateAsync(IReadOnlyList<TEntity> entities, int batchSize, CancellationToken cancellationToken) => BulkAsync(entities, entity => UpdateAsync(entity, cancellationToken), batchSize);

    /// <inheritdoc />
    public Task<int> BulkDeleteAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken) => BulkKeysAsync(keys, DeleteAsync, batchSize, cancellationToken);

    /// <inheritdoc />
    public Task<int> BulkSoftDeleteAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken) => BulkKeysAsync(keys, SoftDeleteAsync, batchSize, cancellationToken);

    /// <inheritdoc />
    public Task<int> BulkRestoreAsync(IReadOnlyList<object> keys, int batchSize, CancellationToken cancellationToken) => BulkKeysAsync(keys, RestoreAsync, batchSize, cancellationToken);

    private async Task<int> ExecuteRowsAsync(CrudOperation operation, object key, CancellationToken cancellationToken)
    {
        var context = new CrudContext(typeof(TEntity), operation, null, key, null, cancellationToken);
        await pipeline.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        return context.AffectedRows;
    }

    private async Task<CrudContext> BuildReadContextAsync(CrudOperation operation, object? key, QueryDefinition? query, CancellationToken cancellationToken)
    {
        var context = new CrudContext(typeof(TEntity), operation, null, key, query, cancellationToken);
        await pipeline.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        return context;
    }

    private static async Task<int> BulkAsync(IReadOnlyList<TEntity> entities, Func<TEntity, Task<TEntity>> action, int batchSize)
    {
        var count = 0;
        foreach (var batch in entities.Chunk(Math.Max(1, batchSize)))
        {
            foreach (var entity in batch)
            {
                await action(entity).ConfigureAwait(false);
                count++;
            }
        }

        return count;
    }

    private static async Task<int> BulkKeysAsync(IReadOnlyList<object> keys, Func<object, CancellationToken, Task<int>> action, int batchSize, CancellationToken cancellationToken)
    {
        var count = 0;
        foreach (var batch in keys.Chunk(Math.Max(1, batchSize)))
        {
            foreach (var key in batch)
            {
                count += await action(key, cancellationToken).ConfigureAwait(false);
            }
        }

        return count;
    }
}
