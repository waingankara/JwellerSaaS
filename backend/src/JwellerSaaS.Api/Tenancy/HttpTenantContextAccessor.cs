using Orion.Framework.Tenancy;

namespace JwellerSaaS.Api.Tenancy;

/// <summary>Builds tenant context from request headers.</summary>
public sealed class HttpTenantContextAccessor(IHttpContextAccessor httpContextAccessor) : ITenantContextAccessor
{
    private const string TenantHeaderName = "X-Tenant-Id";

    /// <inheritdoc />
    public TenantContext TenantContext
    {
        get
        {
            var headers = httpContextAccessor.HttpContext?.Request.Headers;
            if (headers is null || !headers.TryGetValue(TenantHeaderName, out var tenantValue))
            {
                return TenantContext.Global;
            }

            return long.TryParse(tenantValue, out var tenantId) ? new TenantContext(tenantId, false) : TenantContext.Global;
        }
    }
}
