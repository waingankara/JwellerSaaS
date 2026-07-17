using Orion.Framework.Metadata;

namespace Orion.Framework.Audit;

/// <summary>Provides audit metadata for registered entities.</summary>
public interface IAuditEngine { AuditDefinition GetAuditDefinition<T>() where T : class; }

/// <summary>Default audit engine using cached metadata.</summary>
public sealed class AuditEngine(IReflectionMetadataCache cache) : IAuditEngine { public AuditDefinition GetAuditDefinition<T>() where T : class => cache.GetOrAdd<T>().Audit; }
