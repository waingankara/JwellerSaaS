using System.Collections.Concurrent;
using System.Reflection;
using Orion.Framework.Metadata.Attributes;

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
    public MasterDefinition Register(Type entityType)
    {
        var definition = metadataCache.GetOrAdd(entityType);
        ValidateRequiredMetadata(definition);

        var existingEntity = definitions.Values.FirstOrDefault(existing => string.Equals(existing.EntityName, definition.EntityName, StringComparison.OrdinalIgnoreCase) && existing.EntityType != definition.EntityType);
        if (existingEntity is not null) throw new InvalidOperationException($"Duplicate Orion master entity name '{definition.EntityName}' found on '{existingEntity.EntityType.FullName}' and '{definition.EntityType.FullName}'.");

        var existingTable = definitions.Values.FirstOrDefault(existing => string.Equals(existing.TableName, definition.TableName, StringComparison.OrdinalIgnoreCase) && existing.EntityType != definition.EntityType);
        if (existingTable is not null) throw new InvalidOperationException($"Duplicate Orion master table name '{definition.TableName}' found on '{existingTable.EntityType.FullName}' and '{definition.EntityType.FullName}'.");

        return definitions.GetOrAdd(entityType, definition);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<MasterDefinition> DiscoverFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        var discovered = assemblies.Distinct().SelectMany(GetLoadableTypes).Where(IsValidMasterEntity).OrderBy(type => type.FullName, StringComparer.Ordinal).Select(Register).ToArray();
        return discovered;
    }
    private static bool IsValidMasterEntity(Type type)
    {
        return type.IsClass
            && !type.IsAbstract
            && type.GetCustomAttribute<MasterAttribute>() is not null;
    }
    /// <inheritdoc />
    public MasterDefinition Get<T>() where T : class => definitions.TryGetValue(typeof(T), out var definition) ? definition : Register<T>();

    /// <inheritdoc />
    public MasterDefinition? Find(string name) => definitions.Values.FirstOrDefault(definition =>
        string.Equals(definition.EntityName, name, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(definition.TableName, name, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    private static void ValidateRequiredMetadata(MasterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.KeyType is null)
        {
            throw new InvalidOperationException(
                $"Master entity '{definition.EntityType.FullName}' does not define a primary key.");
        }

        if (string.IsNullOrWhiteSpace(definition.EntityName))
        {
            throw new InvalidOperationException(
                $"Master entity '{definition.EntityType.FullName}' does not define an entity name.");
        }

        if (string.IsNullOrWhiteSpace(definition.TableName))
        {
            throw new InvalidOperationException(
                $"Master entity '{definition.EntityType.FullName}' does not define a table name.");
        }

        if (definition.Columns.Count == 0)
        {
            throw new InvalidOperationException(
                $"Master entity '{definition.EntityType.FullName}' does not define any columns.");
        }
    }
}
