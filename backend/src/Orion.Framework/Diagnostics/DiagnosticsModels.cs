using Orion.Framework.Metadata;

namespace Orion.Framework.Diagnostics;

/// <summary>Provides development diagnostics for the Orion Framework runtime.</summary>
public interface IOrionDiagnosticsService
{
    /// <summary>Returns a snapshot of registered framework metadata.</summary>
    OrionDiagnosticsSnapshot GetSnapshot();
}

/// <summary>Serializable diagnostics snapshot for development-only endpoints.</summary>
public sealed record OrionDiagnosticsSnapshot(string Framework, string EnvironmentName, DateTimeOffset GeneratedAt, IReadOnlyList<MasterDiagnostics> Masters);

/// <summary>Serializable metadata diagnostics for a registered master.</summary>
public sealed record MasterDiagnostics(string EntityName, string EntityType, string TableName, string? KeyType, int ColumnCount, IReadOnlyList<string> SearchColumns, IReadOnlyList<string> DuplicateColumns, IReadOnlyList<string> RequiredForInsert, IReadOnlyList<string> RequiredForUpdate, string? TenantColumn, string? BranchColumn, string? SoftDeleteColumn);

/// <summary>Default Orion diagnostics service.</summary>
public sealed class OrionDiagnosticsService(IMasterRegistry masterRegistry, Microsoft.Extensions.Hosting.IHostEnvironment environment) : IOrionDiagnosticsService
{
    /// <inheritdoc />
    public OrionDiagnosticsSnapshot GetSnapshot()
    {
        var masters = masterRegistry.GetAll()
            .OrderBy(definition => definition.EntityName, StringComparer.Ordinal)
            .Select(ToDiagnostics)
            .ToArray();

        return new OrionDiagnosticsSnapshot("Orion Framework", environment.EnvironmentName, DateTimeOffset.UtcNow, masters);
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
