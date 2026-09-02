using System.Text;
using Dapper;
using Orion.Framework.Data;
using Orion.Framework.Metadata;
using Orion.Framework.Sql;
using Orion.Framework.Tenancy;

namespace Orion.Framework.Crud;

/// <summary>
/// Evaluates duplicate metadata for single-field, composite,
/// and tenant-aware duplicate checks.
/// </summary>
public interface IDuplicateEngine
{
    /// <summary>
    /// Returns true when a duplicate entity exists.
    /// </summary>
    Task<bool> ExistsDuplicateAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken)
        where TEntity : class;
}

/// <summary>
/// Default implementation of the duplicate engine.
/// </summary>
public sealed class DuplicateEngine(
    IReflectionMetadataCache metadataCache,
    ISqlExecutor executor,
    ITenantContextAccessor tenantAccessor)
    : IDuplicateEngine
{
    /// <inheritdoc />
    public Task<bool> ExistsDuplicateAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var metadata = metadataCache.GetOrAdd<TEntity>();

        if (metadata.Duplicate.Columns.Count == 0)
        {
            return Task.FromResult(false);
        }

        var parameters = new DynamicParameters();

        var sql = new StringBuilder();

        sql.Append("SELECT EXISTS (SELECT 1 FROM ");
        sql.Append(
            SqlName.Identifier(metadata.TableName));
        sql.Append(" WHERE ");

        var duplicateConditions = metadata.Duplicate.Columns
            .Select(column =>
            {
                var property = entity
                    .GetType()
                    .GetProperty(column.PropertyName);

                var value = property?.GetValue(entity);

                parameters.Add(
                    column.PropertyName,
                    value);

                return
                    $"{SqlName.Identifier(column.ColumnName)} " +
                    $"= @{column.PropertyName}";
            });

        sql.Append(
            string.Join(
                " AND ",
                duplicateConditions));

        if (!tenantAccessor.TenantContext.IsGlobalScope &&
            metadata.Tenant.TenantId is not null)
        {
            parameters.Add(
                "TenantId",
                tenantAccessor.TenantContext.TenantId);

            sql.Append(" AND ");

            sql.Append(
                SqlName.Identifier(
                    metadata.Tenant.TenantId.ColumnName));

            sql.Append(" = @TenantId");
        }

        sql.Append(')');

        return executor.ScalarAsync<bool>(
            sql.ToString(),
            parameters,
            cancellationToken);
    }
}
