using DefaultHttpContext = Microsoft.AspNetCore.Http.DefaultHttpContext;
using Microsoft.Extensions.Options;
using Orion.Framework.Options;
using Orion.Framework.Tenancy;
using Xunit;

namespace JwellerSaaS.UnitTests;

public sealed class TenantResolutionTests
{
    [Fact]
    public async Task HeaderTenantResolverReadsTenantHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = "42";
        var resolver = new HeaderTenantResolver(Options.Create(new TenantOptions()));
        var tenantId = await resolver.ResolveAsync(context, CancellationToken.None);
        Assert.Equal(42, tenantId);
    }
}
