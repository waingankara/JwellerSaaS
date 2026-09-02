using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Orion.Framework.Options;

namespace Orion.Framework.Tenancy;

/// <summary>
/// Resolves the tenant associated with the current HTTP request.
/// </summary>
public interface ITenantResolver
{
    Task<long?> ResolveAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken);
}

/// <summary>
/// Resolves the tenant ID from the configured HTTP header.
/// </summary>
public sealed class HeaderTenantResolver(
    IOptions<TenantOptions> options)
    : ITenantResolver
{
    /// <inheritdoc />
    public Task<long?> ResolveAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var headerName =
            options.Value.HeaderName;

        if (httpContext.Request.Headers.TryGetValue(
                headerName,
                out var values) &&
            long.TryParse(
                values.FirstOrDefault(),
                out var tenantId))
        {
            return Task.FromResult<long?>(
                tenantId);
        }

        return Task.FromResult<long?>(
            null);
    }
}

/// <summary>
/// Resolves a tenant by trying multiple tenant resolvers
/// in registration order.
/// </summary>
public sealed class CompositeTenantResolver(
    IEnumerable<ITenantResolver> resolvers)
    : ITenantResolver
{
    /// <inheritdoc />
    public async Task<long?> ResolveAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        foreach (var resolver in resolvers)
        {
            if (resolver is CompositeTenantResolver)
            {
                continue;
            }

            var tenantId =
                await resolver
                    .ResolveAsync(
                        httpContext,
                        cancellationToken)
                    .ConfigureAwait(false);

            if (tenantId.HasValue)
            {
                return tenantId;
            }
        }

        return null;
    }
}
