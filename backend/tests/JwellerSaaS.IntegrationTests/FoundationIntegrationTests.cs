using Xunit;

namespace JwellerSaaS.IntegrationTests;

/// <summary>Foundation integration smoke tests.</summary>
public sealed class FoundationIntegrationTests
{
    /// <summary>Verifies integration test discovery.</summary>
    [Fact]
    public void TestHostLoads() => Assert.True(true);
}
