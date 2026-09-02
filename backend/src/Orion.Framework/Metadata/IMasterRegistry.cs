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

    /// <summary>Gets metadata for a previously registered master entity name or table name.</summary>
    MasterDefinition? Find(string name);

    /// <summary>Gets all registered master definitions.</summary>
    IReadOnlyCollection<MasterDefinition> All { get; }
}
