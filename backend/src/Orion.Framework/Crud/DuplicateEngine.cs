using System.Text;
using Dapper;
using Orion.Framework.Data;
using Orion.Framework.Metadata;
using Orion.Framework.Sql;
using Orion.Framework.Tenancy;

namespace Orion.Framework.Crud;

/// <summary>Evaluates duplicate metadata for single-field, composite, and tenant-aware checks.</summary>
public interface IDuplicateEngine
{
    /// <summary>Returns true when a duplicate exists.</summary>
    Task<bool> ExistsDuplicateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class;
}

/// <summary>Default duplicate engine.</summary>
public sealed class DuplicateEngine(IReflectionMetadataCache metadataCache, ISqlExecutor executor, ITenantContextAccessor tenantAccessor) : IDuplicateEngine
{
    /// <inheritdoc />
    public Task<bool> ExistsDuplicateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class
    {
        var metadata = metadataCache.GetOrAdd<TEntity>();
        if (metadata.Duplicate.Columns.Count == 0) return Task.FromResult(false);
        var parameters = new DynamicParameters(); var sql = new StringBuilder("SELECT EXISTS (SELECT 1 FROM ").Append(SqlName.Identifier(metadata.TableName)).Append(" WHERE ");
        sql.Append(string.Join(" AND ", metadata.Duplicate.Columns.Select(c => { parameters.Add(c.PropertyName, entity.GetType().GetProperty(c.PropertyName)?.GetValue(entity)); return $"{SqlName.Identifier(c.ColumnName)} = @{c.PropertyName}"; })));
        if (!tenantAccessor.TenantContext.IsGlobalScope && metadata.Tenant.TenantId is not null) { parameters.Add("TenantId", tenantAccessor.TenantContext.TenantId); sql.Append(" AND ").Append(SqlName.Identifier(metadata.Tenant.TenantId.ColumnName)).Append(" = @TenantId"); }
        sql.Append(')');
        return executor.ScalarAsync<bool>(sql.ToString(), parameters, cancellationToken)!;
    }
}
