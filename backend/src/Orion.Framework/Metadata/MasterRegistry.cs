using System.Collections.Concurrent;

namespace Orion.Framework.Metadata;

/// <summary>Default metadata registry backed by the reflection cache.</summary>
public sealed class MasterRegistry(IReflectionMetadataCache metadataCache) : IMasterRegistry
{
    private readonly ConcurrentDictionary<Type, MasterDefinition> definitions = new();

    /// <inheritdoc />
    public IReadOnlyCollection<MasterDefinition> All => definitions.Values.ToArray();

    /// <inheritdoc />
    public MasterDefinition Register<T>() where T : class => definitions.GetOrAdd(typeof(T), metadataCache.GetOrAdd);

    /// <inheritdoc />
    public MasterDefinition Get<T>() where T : class => definitions.TryGetValue(typeof(T), out var definition) ? definition : Register<T>();

    /// <inheritdoc />
    public MasterDefinition? Find(string name) => definitions.Values.FirstOrDefault(definition =>
        string.Equals(definition.EntityName, name, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(definition.TableName, name, StringComparison.OrdinalIgnoreCase));
}
