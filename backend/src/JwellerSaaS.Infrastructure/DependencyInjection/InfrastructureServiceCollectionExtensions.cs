using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.DependencyInjection;

namespace JwellerSaaS.Infrastructure.DependencyInjection;

/// <summary>Registers infrastructure dependencies.</summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>Adds infrastructure services.</summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services) => services.AddOrionFramework();
}
