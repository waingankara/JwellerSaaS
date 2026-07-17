using Orion.Framework.Tenancy;

namespace JwellerSaaS.Api.Middleware;

public sealed class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        var tenantId = await tenantResolver.ResolveAsync(context, context.RequestAborted).ConfigureAwait(false);
        if (tenantId.HasValue) context.Items["TenantId"] = tenantId.Value;
        await next(context).ConfigureAwait(false);
    }
}
