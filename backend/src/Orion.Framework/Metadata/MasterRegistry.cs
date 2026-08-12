using System.Collections.Concurrent;
using System.Reflection;
using Orion.Framework.Metadata.Attributes;

namespace Orion.Framework.Metadata;

/// <summary>Default metadata registry backed by the reflection cache.</summary>
public sealed class MasterRegistry(IReflectionMetadataCache metadataCache) : IMasterRegistry
{
    private readonly ConcurrentDictionary<Type, MasterDefinition> definitions = new();

    /// <inheritdoc />
    public MasterDefinition Register<T>() where T : class => Register(typeof(T));

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

    /// <inheritdoc />
    public MasterDefinition Get<T>() where T : class => definitions.TryGetValue(typeof(T), out var definition) ? definition : Register<T>();

    /// <inheritdoc />
    public bool TryGetByEntityName(string entityName, out MasterDefinition? definition)
    {
        definition = definitions.Values.FirstOrDefault(existing => string.Equals(existing.EntityName, entityName, StringComparison.OrdinalIgnoreCase));
        return definition is not null;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<MasterDefinition> GetAll() => definitions.Values.OrderBy(definition => definition.EntityName, StringComparer.OrdinalIgnoreCase).ToArray();

    private static bool IsValidMasterEntity(Type type) => type is { IsClass: true, IsAbstract: false } && (type.IsPublic || type.IsNestedPublic) && type.GetCustomAttribute<MasterAttribute>() is not null;

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(type => type is not null)!; }
    }

    private static void ValidateRequiredMetadata(MasterDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.EntityName)) throw new InvalidOperationException($"Orion master '{definition.EntityType.FullName}' has no entity name.");
        if (string.IsNullOrWhiteSpace(definition.TableName)) throw new InvalidOperationException($"Orion master '{definition.EntityType.FullName}' has no table name.");
        if (definition.KeyType is null) throw new InvalidOperationException($"Orion master '{definition.EntityType.FullName}' must define a primary key.");
        if (definition.Columns.Count == 0) throw new InvalidOperationException($"Orion master '{definition.EntityType.FullName}' must define at least one mapped column.");
        var duplicateColumns = definition.Columns.GroupBy(column => column.ColumnName, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
        if (duplicateColumns is not null) throw new InvalidOperationException($"Orion master '{definition.EntityType.FullName}' has duplicate column name '{duplicateColumns.Key}'.");
    }
}
