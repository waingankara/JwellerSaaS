using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orion.Framework.DependencyInjection;
using Orion.Framework.Diagnostics;
using Orion.Framework.Metadata;
using Orion.Framework.Metadata.Attributes;
using Orion.Framework.Pagination;
using Orion.Framework.Search;
using Orion.Framework.Security;
using Orion.Framework.Sql;
using Orion.Framework.Tenancy;
using Xunit;



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
    public void MetadataDiscoveryRegistersMastersFromAssembly()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        var definitions = registry.DiscoverFromAssemblies(
    new[] { typeof(JwellerSaaS.Domain.Masters.Category).Assembly });
        Assert.Contains(
    definitions,
    definition => definition.EntityName == "Category");

        Assert.Contains(
            registry.All,
            definition => definition.EntityName == "Category");
    }

    [Fact]
    public void MetadataRegistryLookupIsCaseInsensitive()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        registry.DiscoverFromAssemblies(new[] { typeof(JwellerSaaS.Domain.Masters.Category).Assembly });
        var definition = registry.Find("category");

        Assert.NotNull(definition);
        Assert.Equal("Category", definition?.EntityName);
    }

    [Fact]
    public void MetadataRegistryUnknownLookupReturnsFalse()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        var definition = registry.Find("missing");
        Assert.Null(definition);
        Assert.Null(definition);
    }

    [Fact]
    public void MetadataRegistryDetectsDuplicateEntityNames()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        registry.Register(typeof(DuplicateNameOne));
        var exception = Assert.Throws<InvalidOperationException>(() => registry.Register(typeof(DuplicateNameONE)));
        Assert.Contains("Duplicate Orion master entity name", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MetadataRegistryDetectsInvalidMetadata()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        var exception = Assert.Throws<InvalidOperationException>(() => registry.Register(typeof(InvalidMaster)));
        Assert.Contains("primary key", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MetadataRegistryDetectsDuplicateTableNames()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        registry.Register(typeof(DuplicateTableOne));
        var exception = Assert.Throws<InvalidOperationException>(() => registry.Register(typeof(DuplicateTableTwo)));
        Assert.Contains("Duplicate Orion master table name", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DiagnosticsSnapshotContainsRegisteredMasterMetadata()
    {
        var registry = new MasterRegistry(new ReflectionMetadataCache());
        registry.Register<TestMaster>();
        var service = new OrionDiagnosticsService(registry, new TestHostEnvironment());
        var snapshot = service.GetSnapshot();
        var master = Assert.Single(snapshot.Entities);
        Assert.Equal("Orion Framework", snapshot.Framework);
        Assert.Equal("Development", snapshot.Environment);
        Assert.Equal(1, snapshot.EntityCount);
        Assert.Equal("test_masters", master.TableName);
        Assert.Contains("name", master.SearchableColumns);
        Assert.Contains("name", master.DuplicateColumns);
        Assert.Equal("TenantId", master.TenantColumn);
    }

    [Fact]
    public void ServiceProviderBuildsWithScopeValidation()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder().Build());

        services.AddSingleton<IHostEnvironment>(
            new TestHostEnvironment());

        services.AddScoped<ITenantContextAccessor, TestTenantContextAccessor>();
        services.AddScoped<ICurrentUserAccessor, TestCurrentUserAccessor>();

        services.AddOrionFramework();

        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.NotNull(provider.GetRequiredService<IMasterRegistry>());
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
        Assert.Contains("WHERE \"id\" = @Id", update.Sql, StringComparison.Ordinal);
        Assert.Contains("\"name\" = @Name", duplicate.Sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SearchBuilderSupportsOperators()
    {
        var definition = new ReflectionMetadataCache().GetOrAdd<TestMaster>();
        var statement = new SearchBuilder().Build(definition, new[] { new Orion.Framework.Query.FilterDefinition(nameof(TestMaster.Name), SearchOperator.Contains, "gold") });
        Assert.Equal("WHERE \"name\" LIKE @p0", statement.Sql);
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
    public sealed class TestMaster
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

    private sealed class InvalidMaster { public string Name { get; init; } = string.Empty; }

    [Master("duplicate_name_one")]
    private sealed class DuplicateNameOne { [PrimaryKey] public long Id { get; init; } }

    [Master("duplicate_name_two")]
    private sealed class DuplicateNameONE { [PrimaryKey] public long Id { get; init; } }

    [Master("duplicate_table")]
    private sealed class DuplicateTableOne { [PrimaryKey] public long Id { get; init; } }

    [Master("DUPLICATE_TABLE")]
    private sealed class DuplicateTableTwo { [PrimaryKey] public long Id { get; init; } }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    private sealed class TestTenantContextAccessor : ITenantContextAccessor
    {
        public TenantContext TenantContext => TenantContext.Global;
    }

    private sealed class TestCurrentUserAccessor : ICurrentUserAccessor
    {
        public CurrentUser CurrentUser => CurrentUser.Anonymous;
    }
}
