using Dapper;
using Orion.Framework.Metadata;
using Orion.Framework.Sql;

namespace Orion.Framework.Tenancy;

/// <summary>Applies tenant and branch predicates to framework SQL.</summary>
public interface ITenantEngine { SqlStatement ApplyTenant(MasterDefinition definition, string sql, DynamicParameters parameters); }

/// <summary>Default tenant engine that injects TenantId and BranchId when available.</summary>
public sealed class TenantEngine(ITenantContextAccessor tenantContextAccessor) : ITenantEngine
{
    public SqlStatement ApplyTenant(MasterDefinition definition, string sql, DynamicParameters parameters)
    {
        var context = tenantContextAccessor.TenantContext;
        if (context.IsGlobalScope || context.TenantId is null || definition.Tenant.TenantId is null) return new SqlStatement(sql, parameters);
        var separator = sql.Contains(" WHERE ", StringComparison.OrdinalIgnoreCase) ? " AND " : " WHERE ";
        parameters.Add("TenantId", context.TenantId.Value);
        var predicate = SqlName.Identifier(definition.Tenant.TenantId.ColumnName) + " = @TenantId";
        if (context.BranchId is not null && definition.Tenant.BranchId is not null)
        {
            parameters.Add("BranchId", context.BranchId.Value);
            predicate += " AND " + SqlName.Identifier(definition.Tenant.BranchId.ColumnName) + " = @BranchId";
        }

        return new SqlStatement(sql + separator + predicate, parameters);
    }
}
