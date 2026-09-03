using System.Collections.Concurrent;
using System.Reflection;
using Orion.Framework.Metadata.Attributes;

namespace Orion.Framework.Metadata;

/// <summary>
/// Metadata registry for transactional entities.
/// </summary>
public sealed class TransactionalRegistry(
    IReflectionMetadataCache metadataCache)
{
    private readonly ConcurrentDictionary<Type, MasterDefinition> definitions = new();

    /// <summary>
    /// Gets all registered transactional entity definitions.
    /// </summary>
    public IReadOnlyCollection<MasterDefinition> All =>
        definitions.Values.ToArray();

    /// <summary>
    /// Registers a transactional entity type.
    /// </summary>
    public MasterDefinition Register(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        var attribute =
            entityType.GetCustomAttribute<TransactionalAttribute>();

        if (attribute is null)
        {
            throw new InvalidOperationException(
                $"Entity '{entityType.FullName}' is not marked with TransactionalAttribute.");
        }

        var definition = metadataCache.GetOrAdd(entityType);

        if (definition.KeyType is null)
        {
            throw new InvalidOperationException(
                $"Transactional entity '{entityType.FullName}' does not define a primary key.");
        }

        if (string.IsNullOrWhiteSpace(attribute.TableName))
        {
            throw new InvalidOperationException(
                $"Transactional entity '{entityType.FullName}' does not define a table name.");
        }

        var existingEntity = definitions.Values.FirstOrDefault(
            existing =>
                string.Equals(
                    existing.EntityName,
                    definition.EntityName,
                    StringComparison.OrdinalIgnoreCase)
                && existing.EntityType != definition.EntityType);

        if (existingEntity is not null)
        {
            throw new InvalidOperationException(
                $"Duplicate transactional entity name '{definition.EntityName}' found on " +
                $"'{existingEntity.EntityType.FullName}' and " +
                $"'{definition.EntityType.FullName}'.");
        }

        return definitions.GetOrAdd(entityType, definition);
    }

    /// <summary>
    /// Registers a transactional entity type.
    /// </summary>
    public MasterDefinition Register<T>() where T : class =>
        Register(typeof(T));

    /// <summary>
    /// Discovers transactional entities from the supplied assemblies.
    /// </summary>
    public IReadOnlyCollection<MasterDefinition> DiscoverFromAssemblies(
        IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var discovered = assemblies
            .Distinct()
            .SelectMany(GetLoadableTypes)
            .Where(IsValidTransactionalEntity)
            .OrderBy(
                type => type.FullName,
                StringComparer.Ordinal)
            .Select(Register)
            .ToArray();

        return discovered;
    }

    /// <summary>
    /// Finds a transactional entity by entity name or table name.
    /// </summary>
    public MasterDefinition? Find(string name) =>
        definitions.Values.FirstOrDefault(
            definition =>
                string.Equals(
                    definition.EntityName,
                    name,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    definition.TableName,
                    name,
                    StringComparison.OrdinalIgnoreCase));

    private static bool IsValidTransactionalEntity(Type type)
    {
        return type.IsClass
            && !type.IsAbstract
            && type.GetCustomAttribute<TransactionalAttribute>() is not null;
    }

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
}