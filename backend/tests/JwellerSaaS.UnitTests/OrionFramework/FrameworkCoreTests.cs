using Orion.Framework.Metadata;
using Orion.Framework.Metadata.Attributes;
using Orion.Framework.Pagination;
using Orion.Framework.Search;
using Orion.Framework.Sql;
using Xunit;
using FilterDefinition = Orion.Framework.Query.FilterDefinition;


namespace JwellerSaaS.UnitTests.OrionFramework;

public sealed class FrameworkCoreTests
{
    [Fact]
    public void MetadataRegistryRegistersStronglyTypedMetadata()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        var definition = registry.Register<TestMaster>();
        Assert.Equal("test_masters", definition.TableName);
        Assert.Equal(typeof(long), definition.KeyType);
        Assert.NotNull(definition.Audit.CreatedDate);
        Assert.NotNull(definition.Tenant.TenantId);
        Assert.Single(definition.Search.Columns);
        Assert.Single(definition.Duplicate.Columns);
    }

    [Fact]
    public void ReflectionCacheReturnsSameDefinitionInstance()
    {
        var cache = new ReflectionMetadataCache();
        var first = cache.GetOrAdd<TestMaster>();
        var second = cache.GetOrAdd<TestMaster>();
        Assert.Same(first, second);
    }

    [Fact]
    public void SqlBuildersCreateParameterizedSql()
    {
        var definition = new ReflectionMetadataCache().GetOrAdd<TestMaster>();
        var insert = new InsertBuilder().Build(definition, new { Id = 1L, Name = "A" });
        var update = new UpdateBuilder().Build(definition, new { Id = 1L, Name = "A" });
        var duplicate = new DuplicateBuilder().Build(definition, new { Name = "A" });
        Assert.Contains("@Name", insert.Sql, StringComparison.Ordinal);
        Assert.Contains("WHERE id = @Id", update.Sql, StringComparison.Ordinal);
        Assert.Contains("name = @Name", duplicate.Sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SearchBuilderSupportsOperators()
    {
        var definition = new ReflectionMetadataCache().GetOrAdd<TestMaster>();
        var statement = new SearchBuilder().Build(definition, new[] { new FilterDefinition(nameof(TestMaster.Name), SearchOperator.Contains, "gold") });
        Assert.Equal("WHERE name LIKE @p0", statement.Sql);
        Assert.NotNull(statement.Parameters);
    }

    [Fact]
    public void PaginationCalculatesOffsetAndTotalPages()
    {
        var request = new PagedRequest(3, 25, Array.Empty<SortDefinition>(), Array.Empty<Orion.Framework.Pagination.FilterDefinition>());
        var result = new PagedResult<int>(new[] { 1 }, 101, request.PageNumber, request.PageSize);
        Assert.Equal(50, request.Offset);
        Assert.Equal(5, result.TotalPages);
    }

    [Master("test_masters")]
    private sealed class TestMaster
    {
        [PrimaryKey]
        [DbColumn("id")]
        public long Id { get; init; }
        [Searchable]
        [DuplicateCheck]
        [RequiredForInsert]
        [RequiredForUpdate]
        [DbColumn("name")]
        public string Name { get; init; } = string.Empty;
        public long TenantId { get; init; }
        public DateTime CreatedDate { get; init; }
        [IgnoreColumn]
        public string Ignored { get; init; } = string.Empty;
    }
}
