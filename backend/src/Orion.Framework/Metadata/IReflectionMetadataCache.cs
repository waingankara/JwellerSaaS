namespace Orion.Framework.Metadata;

/// <summary>Caches reflected master metadata so each entity type is reflected only once.</summary>
public interface IReflectionMetadataCache
{
    /// <summary>Gets cached metadata for the specified entity type.</summary>
    MasterDefinition GetOrAdd(Type entityType);

    /// <summary>Gets cached metadata for the specified entity type.</summary>
    MasterDefinition GetOrAdd<T>() where T : class;
}
