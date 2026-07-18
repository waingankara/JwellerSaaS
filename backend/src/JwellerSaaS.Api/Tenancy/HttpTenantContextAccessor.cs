using Orion.Framework.Authentication;
using Orion.Framework.Tenancy;

namespace JwellerSaaS.Api.Tenancy;

public sealed class HttpTenantContextAccessor(IHttpContextAccessor httpContextAccessor) : ITenantContextAccessor
{
    public TenantContext TenantContext
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            var tenantId = context?.Items["TenantId"] as long?;
            var branchId = context?.User.FindFirst(OrionClaimTypes.BranchId)?.Value;
            var parsedBranchId = long.TryParse(branchId, out var value) ? value : (long?)null;
            return tenantId.HasValue ? new TenantContext(tenantId, false, parsedBranchId) : TenantContext.Global;
        }
    }
}
