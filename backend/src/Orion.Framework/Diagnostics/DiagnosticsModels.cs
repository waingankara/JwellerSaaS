using Orion.Framework.Metadata;

namespace Orion.Framework.Diagnostics;

/// <summary>Provides development diagnostics for the Orion Framework runtime.</summary>
public interface IOrionDiagnosticsService
{
    /// <summary>Returns a snapshot of registered framework metadata.</summary>
    OrionDiagnosticsSnapshot GetSnapshot();

    /// <summary>Attempts to return diagnostics for one registered entity.</summary>
    bool TryGetEntity(string entityName, out MasterDiagnostics? diagnostics);
}

/// <summary>Serializable diagnostics snapshot for development-only endpoints.</summary>
public sealed record OrionDiagnosticsSnapshot(string Framework, string Environment, DateTimeOffset GeneratedAt, int EntityCount, IReadOnlyList<MasterDiagnostics> Entities);

/// <summary>Serializable metadata diagnostics for a registered master.</summary>
public sealed record MasterDiagnostics(string EntityName, string EntityType, string TableName, string? PrimaryKeyType, int ColumnCount, IReadOnlyList<string> SearchableColumns, IReadOnlyList<string> DuplicateColumns, IReadOnlyList<string> RequiredInsertColumns, IReadOnlyList<string> RequiredUpdateColumns, string? TenantColumn, string? BranchColumn, string? SoftDeleteColumn);

/// <summary>Default Orion diagnostics service.</summary>
public sealed class OrionDiagnosticsService(IMasterRegistry masterRegistry, Microsoft.Extensions.Hosting.IHostEnvironment environment) : IOrionDiagnosticsService
{
    /// <inheritdoc />
    public OrionDiagnosticsSnapshot GetSnapshot()
    {
        var entities = masterRegistry.All.Select(ToDiagnostics).ToArray();
        return new OrionDiagnosticsSnapshot("Orion Framework", environment.EnvironmentName, DateTimeOffset.UtcNow, entities.Length, entities);
    }

    /// <inheritdoc />
    public bool TryGetEntity(string entityName, out MasterDiagnostics? diagnostics)
    {
        var definition = masterRegistry.Find(entityName);

        if (definition is not null)
        {
            diagnostics = ToDiagnostics(definition);
            return true;
        }

        diagnostics = null;
        return false;
    }

    private static MasterDiagnostics ToDiagnostics(MasterDefinition definition) => new(
        definition.EntityName,
        definition.EntityType.FullName ?? definition.EntityType.Name,
        definition.TableName,
        definition.KeyType?.Name,
        definition.Columns.Count,
        definition.Search.Columns.Select(column => column.ColumnName).ToArray(),
        definition.Duplicate.Columns.Select(column => column.ColumnName).ToArray(),
        definition.Validation.RequiredForInsert.Select(column => column.ColumnName).ToArray(),
        definition.Validation.RequiredForUpdate.Select(column => column.ColumnName).ToArray(),
        definition.Tenant.TenantId?.ColumnName,
        definition.Tenant.BranchId?.ColumnName,
        definition.Audit.IsDeleted?.ColumnName);
}
