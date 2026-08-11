using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Orion.Framework.Metadata;

/// <summary>Discovers Orion master metadata before the application accepts requests.</summary>
public sealed class OrionMetadataDiscoveryHostedService(IMasterRegistry masterRegistry, IHostEnvironment environment, ILogger<OrionMetadataDiscoveryHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation("Orion Framework startup");
        logger.LogInformation("Orion metadata discovery start");
        var assemblies = LoadCandidateAssemblies().OrderBy(a => a.GetName().Name, StringComparer.Ordinal).ToArray();
        logger.LogInformation("Orion metadata scanned assemblies: {Assemblies}", assemblies.Select(a => a.GetName().Name).ToArray());
        var discovered = masterRegistry.DiscoverFromAssemblies(assemblies);
        logger.LogInformation("Orion metadata discovered entity count: {Count}", discovered.Count);
        logger.LogInformation("Orion metadata discovered entity names: {EntityNames}", discovered.Select(d => d.EntityName).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        logger.LogInformation("Orion metadata validation completed");
        logger.LogInformation("Orion Framework startup completed in {ElapsedMilliseconds} ms for {Environment}", stopwatch.ElapsedMilliseconds, environment.EnvironmentName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static IReadOnlyCollection<Assembly> LoadCandidateAssemblies()
    {
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (name.StartsWith("JwellerSaaS", StringComparison.Ordinal) || name.StartsWith("Orion", StringComparison.Ordinal))
            {
                try { Assembly.Load(new AssemblyName(name)); }
                catch (FileLoadException) { }
                catch (BadImageFormatException) { }
            }
        }

        return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && IsApplicationAssembly(a)).Distinct().ToArray();
    }

    private static bool IsApplicationAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name ?? string.Empty;
        return name.StartsWith("JwellerSaaS", StringComparison.Ordinal) || name.StartsWith("Orion", StringComparison.Ordinal);
    }
}
