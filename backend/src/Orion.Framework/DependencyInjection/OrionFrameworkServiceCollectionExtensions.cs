using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Audit;
using Orion.Framework.Data;
using Orion.Framework.Metadata;
using Orion.Framework.Sql;
using Orion.Framework.Tenancy;

namespace Orion.Framework.DependencyInjection;

/// <summary>Registers Orion Framework services.</summary>
public static class OrionFrameworkServiceCollectionExtensions
{
    /// <summary>Adds reusable Orion Framework services.</summary>
    public static IServiceCollection AddOrionFramework(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory, NpgsqlConnectionFactory>();
        services.AddSingleton<IReflectionMetadataCache, ReflectionMetadataCache>();
        services.AddSingleton<IMasterRegistry, MasterRegistry>();
        services.AddScoped<ISqlExecutor, SqlExecutor>();
        services.AddSingleton<InsertBuilder>();
        services.AddSingleton<UpdateBuilder>();
        services.AddSingleton<DeleteBuilder>();
        services.AddSingleton<SelectBuilder>();
        services.AddSingleton<SearchBuilder>();
        services.AddSingleton<PaginationBuilder>();
        services.AddSingleton<ExistsBuilder>();
        services.AddSingleton<CountBuilder>();
        services.AddSingleton<DuplicateBuilder>();
        services.AddScoped<IAuditEngine, AuditEngine>();
        services.AddScoped<ITenantEngine, TenantEngine>();
        return services;
    }
}
