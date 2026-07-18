using Xunit;

namespace JwellerSaaS.UnitTests;

/// <summary>Foundation smoke tests.</summary>
public sealed class FoundationTests
{
    /// <summary>Verifies the test host is configured.</summary>
    [Fact]
    public void TestHostLoads() => Assert.True(true);
}
