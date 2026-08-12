using System.Reflection;

namespace Orion.Framework.Metadata;

/// <summary>Registers and resolves framework master metadata.</summary>
public interface IMasterRegistry
{
    /// <summary>Registers a master entity type.</summary>
    MasterDefinition Register<T>() where T : class;

    /// <summary>Registers a master entity type.</summary>
    MasterDefinition Register(Type entityType);

    /// <summary>Discovers and registers master entity types from the supplied assemblies.</summary>
    IReadOnlyCollection<MasterDefinition> DiscoverFromAssemblies(IEnumerable<Assembly> assemblies);

    /// <summary>Gets metadata for a previously registered master entity type.</summary>
    MasterDefinition Get<T>() where T : class;

    /// <summary>Attempts to get metadata by entity name case-insensitively.</summary>
    bool TryGetByEntityName(string entityName, out MasterDefinition? definition);

    /// <summary>Gets all registered master metadata definitions.</summary>
    IReadOnlyCollection<MasterDefinition> GetAll();
}
