namespace Orion.Framework.Metadata;

/// <summary>Defines duplicate-check columns for a master entity.</summary>
public sealed record DuplicateDefinition(IReadOnlyList<ColumnDefinition> Columns);
