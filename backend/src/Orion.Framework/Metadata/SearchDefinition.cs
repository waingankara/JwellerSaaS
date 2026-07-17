namespace Orion.Framework.Metadata;

/// <summary>Defines searchable columns for a master entity.</summary>
public sealed record SearchDefinition(IReadOnlyList<ColumnDefinition> Columns);
