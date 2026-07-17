using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Data;

namespace Orion.Framework.DependencyInjection;

/// <summary>Registers Orion Framework services.</summary>
public static class OrionFrameworkServiceCollectionExtensions
{
    /// <summary>Adds reusable Orion Framework services.</summary>
    public static IServiceCollection AddOrionFramework(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory, NpgsqlConnectionFactory>();
        return services;
    }
}
