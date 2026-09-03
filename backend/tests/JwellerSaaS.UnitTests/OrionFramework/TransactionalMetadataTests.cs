using JwellerSaaS.Domain.Transactions;
using Orion.Framework.Metadata;
using Xunit;

namespace JwellerSaaS.UnitTests.OrionFramework;

public sealed class TransactionalMetadataTests
{
    [Fact]
    public void TransactionalRegistryDiscoversTransactionalEntities()
    {
        var registry =
            new TransactionalRegistry(
                new ReflectionMetadataCache());

        var definitions =
            registry.DiscoverFromAssemblies(
                new[]
                {
                    typeof(ProductVariant).Assembly
                });

        Assert.Contains(
            definitions,
            definition =>
                definition.EntityType == typeof(ProductVariant));

        Assert.Contains(
            definitions,
            definition =>
                definition.EntityName == "ProductVariant");

        Assert.Contains(
            definitions,
            definition =>
                definition.TableName == "product_variant");
    }
}