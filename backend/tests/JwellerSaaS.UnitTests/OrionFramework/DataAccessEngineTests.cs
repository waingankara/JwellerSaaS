using Xunit;
using Orion.Framework.Crud;
using Orion.Framework.Metadata;
using Orion.Framework.Metadata.Attributes;
using Orion.Framework.Query;
using Orion.Framework.Search;

namespace JwellerSaaS.UnitTests.OrionFramework;

public sealed class DataAccessEngineTests
{
    [Fact]
    public void Query_builder_supports_search_sort_and_pagination()
    {
        var metadata = new ReflectionMetadataCache().GetOrAdd<TestEntity>();
        var builder = new CrudSqlBuilder();
        var statement = builder.Select(metadata, new QueryDefinition(
            new[] { new FilterDefinition(nameof(TestEntity.Name), SearchOperator.Contains, "gold") },
            new[] { new SortDefinition(nameof(TestEntity.Name), SortDirection.Descending) },
            new QueryOptions(Offset: 20, PageSize: 10),
            new SearchOptions("sku", new[] { nameof(TestEntity.Code) })));

        Assert.Contains("LIKE @p0", statement.CommandText);
        Assert.Contains("ORDER BY \"Name\" DESC", statement.CommandText);
        Assert.Contains("LIMIT @PageSize OFFSET @Offset", statement.CommandText);
    }

    [Fact]
    public void Query_builder_supports_projection_distinct_and_top()
    {
        var metadata = new ReflectionMetadataCache().GetOrAdd<TestEntity>();
        var statement = new CrudSqlBuilder().Select(metadata, new QueryDefinition(Options: new QueryOptions(Distinct: true, Top: 1, Columns: new[] { nameof(TestEntity.Name), nameof(TestEntity.Code) })));

        Assert.StartsWith("SELECT DISTINCT \"Name\" FROM", statement.CommandText);
    }

    [Fact]
    public void Metadata_cache_detects_duplicate_audit_and_tenant_columns()
    {
        var metadata = new ReflectionMetadataCache().GetOrAdd<TestEntity>();

        Assert.Equal(2, metadata.Duplicate.Columns.Count);
        Assert.NotNull(metadata.Tenant.TenantId);
        Assert.NotNull(metadata.Audit.CreatedDate);
        Assert.NotNull(metadata.Audit.IsDeleted);
    }

    [Master("test_entities")]
    private sealed class TestEntity
    {
        [PrimaryKey]
        public long Id { get; set; }

        [Searchable]
        [DuplicateCheck]
        public string Name { get; set; } = string.Empty;

        [Searchable]
        [DuplicateCheck]
        public string Code { get; set; } = string.Empty;

        public long TenantId { get; set; }

        public long BranchId { get; set; }

        public long? CreatedBy { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public long? ModifiedBy { get; set; }

        public DateTimeOffset? ModifiedDate { get; set; }

        public long? DeletedBy { get; set; }

        public DateTimeOffset? DeletedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
