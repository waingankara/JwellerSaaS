using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Metadata;
using Orion.Framework.Metadata.Attributes;

namespace JwellerSaaS.Infrastructure.DependencyInjection;

public static class MetadataRegistration
{
    public static IServiceProvider RegisterBusinessMasters(this IServiceProvider services)
    {
        var registry = services.GetRequiredService<IMasterRegistry>();

        var assembly = Assembly.Load("JwellerSaaS.Domain");

        var masterTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetCustomAttribute<MasterAttribute>() is not null);

        foreach (var type in masterTypes)
        {
            var method = typeof(IMasterRegistry)
                .GetMethod(nameof(IMasterRegistry.Register))!
                .MakeGenericMethod(type);

            method.Invoke(registry, null);
        }

        return services;
    }
}
