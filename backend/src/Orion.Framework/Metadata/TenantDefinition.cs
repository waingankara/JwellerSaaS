namespace Orion.Framework.Metadata;

/// <summary>Defines detected tenant columns.</summary>
public sealed record TenantDefinition(ColumnDefinition? TenantId, ColumnDefinition? BranchId);
