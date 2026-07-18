using JwellerSaaS.Domain.Masters;
using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Metadata;

namespace JwellerSaaS.Infrastructure.DependencyInjection;

/// <summary>Registers business masters with the Orion metadata registry.</summary>
public static class MetadataRegistration
{
    /// <summary>Registers metadata-driven masters without controllers or repositories.</summary>
    public static IServiceProvider RegisterBusinessMasters(this IServiceProvider services)
    {
        services.GetRequiredService<IMasterRegistry>().Register<Category>();
        return services;
    }
}
