namespace Orion.Framework.Metadata;

/// <summary>Registers and resolves framework master metadata.</summary>
public interface IMasterRegistry
{
    /// <summary>Registers a master entity type.</summary>
    MasterDefinition Register<T>() where T : class;

    /// <summary>Gets metadata for a previously registered master entity type.</summary>
    MasterDefinition Get<T>() where T : class;

    /// <summary>Gets all registered master metadata definitions.</summary>
    IReadOnlyCollection<MasterDefinition> GetAll();
}
