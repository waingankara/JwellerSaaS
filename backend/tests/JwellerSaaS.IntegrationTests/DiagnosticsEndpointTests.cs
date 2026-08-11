using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace JwellerSaaS.IntegrationTests;

public sealed class DiagnosticsEndpointTests
{
    [Fact]
    public async Task DiagnosticsEndpointIsAvailableInDevelopment()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("environment", "Development"));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Orion Framework", body, StringComparison.Ordinal);
        Assert.Contains("category", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DiagnosticsEndpointIsHiddenOutsideDevelopment()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("environment", "Production"));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpointIsAvailableOutsideDevelopment()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("environment", "Production"));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
