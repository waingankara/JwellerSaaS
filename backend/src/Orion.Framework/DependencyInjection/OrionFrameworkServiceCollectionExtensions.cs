using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Audit;
using Orion.Framework.Data;
using Orion.Framework.Metadata;
using Orion.Framework.Sql;
using Orion.Framework.Tenancy;
using Orion.Framework.Authentication;
using Orion.Framework.Authorization;
using Orion.Framework.Identity;
using Orion.Framework.Crud;
using Orion.Framework.DomainEvents;
using Orion.Framework.Validation;

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
        services.AddSingleton<CrudSqlBuilder>();
        services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));
        services.AddScoped<IGenericMasterCrudService, GenericMasterCrudService>();
        services.AddScoped<ICrudPipeline, CrudPipeline>();
        services.AddScoped<IDuplicateEngine, DuplicateEngine>();
        services.AddSingleton<IDomainEventPublisher, NullDomainEventPublisher>();
        services.AddSingleton<IValidationPipeline, NullValidationPipeline>();
        services.AddScoped<IAuditEngine, AuditEngine>();
        services.AddScoped<ITenantEngine, TenantEngine>();
        services.AddSingleton<IClaimsBuilder, ClaimsBuilder>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ITokenValidator, JwtTokenValidator>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IOrionAuthorizationService, OrionAuthorizationService>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IPasswordHasher, MicrosoftPasswordHasher>();
        services.AddSingleton<IPasswordHistoryValidator, PasswordHistoryValidator>();
        services.AddScoped<HeaderTenantResolver>();
        services.AddScoped<ITenantResolver>(provider => new CompositeTenantResolver(new ITenantResolver[] { provider.GetRequiredService<HeaderTenantResolver>() }));
        return services;
    }
}
