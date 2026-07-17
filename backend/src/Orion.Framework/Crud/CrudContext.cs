using Orion.Framework.Metadata;
using Orion.Framework.Query;

namespace Orion.Framework.Crud;

/// <summary>Context passed through every replaceable CRUD pipeline stage.</summary>
public sealed class CrudContext
{
    /// <summary>Creates a new CRUD context.</summary>
    public CrudContext(Type entityType, CrudOperation operation, object? entity, object? key, QueryDefinition? query, CancellationToken cancellationToken)
    { EntityType = entityType; Operation = operation; Entity = entity; Key = key; Query = query ?? QueryDefinition.Empty; CancellationToken = cancellationToken; }
    /// <summary>Entity CLR type.</summary>
    public Type EntityType { get; }
    /// <summary>Operation being executed.</summary>
    public CrudOperation Operation { get; }
    /// <summary>Entity instance when applicable.</summary>
    public object? Entity { get; }
    /// <summary>Primary key when applicable.</summary>
    public object? Key { get; }
    /// <summary>Query specification.</summary>
    public QueryDefinition Query { get; }
    /// <summary>Resolved metadata.</summary>
    public MasterDefinition? Metadata { get; set; }
    /// <summary>Generated SQL.</summary>
    public string? Sql { get; set; }
    /// <summary>SQL parameters.</summary>
    public object? Parameters { get; set; }
    /// <summary>Rows affected or read.</summary>
    public int AffectedRows { get; set; }
    /// <summary>Correlation id for diagnostics.</summary>
    public string CorrelationId { get; } = Guid.NewGuid().ToString("N");
    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
}
