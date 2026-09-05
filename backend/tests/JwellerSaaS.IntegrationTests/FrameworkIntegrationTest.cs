using Orion.Framework.Metadata.Attributes;

namespace JwellerSaaS.IntegrationTests;

[Transactional("framework_integration_test")]
public sealed class FrameworkIntegrationTest
{
    [PrimaryKey]
    [DbColumn("framework_integration_test_id")]
    public long FrameworkIntegrationTestId { get; set; }

    [DbColumn("value")]
    public string Value { get; set; } = string.Empty;
}
