using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Orion.Framework.Options;

namespace Orion.Framework.Tenancy;

public interface ITenantResolver { Task<long?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken); }

public sealed class HeaderTenantResolver(IOptions<TenantOptions> options) : ITenantResolver
{
    public Task<long?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var header = options.Value.HeaderName;
        if (httpContext.Request.Headers.TryGetValue(header, out var values) && long.TryParse(values.FirstOrDefault(), out var tenantId)) return Task.FromResult<long?>(tenantId);
        return Task.FromResult<long?>(null);
    }
}

public sealed class CompositeTenantResolver(IEnumerable<ITenantResolver> resolvers) : ITenantResolver
{
    public async Task<long?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        foreach (var resolver in resolvers)
        {
            if (resolver is CompositeTenantResolver) continue;
            var tenantId = await resolver.ResolveAsync(httpContext, cancellationToken).ConfigureAwait(false);
            if (tenantId.HasValue) return tenantId;
        }
        return null;
    }
}
