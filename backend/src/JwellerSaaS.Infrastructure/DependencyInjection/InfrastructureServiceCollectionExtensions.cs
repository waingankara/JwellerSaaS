using Microsoft.Extensions.DependencyInjection;
using JwellerSaaS.Application.Identity;
using JwellerSaaS.Infrastructure.Identity;
using Orion.Framework.DependencyInjection;
using Orion.Framework.Identity;

namespace JwellerSaaS.Infrastructure.DependencyInjection;

/// <summary>
/// Registers infrastructure dependencies.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services
            .AddOrionFramework()
            .AddScoped<IRefreshTokenStore, DapperRefreshTokenStore>()
            .AddScoped<IIdentityRepository, DapperIdentityRepository>();

        return services;
    }
}
