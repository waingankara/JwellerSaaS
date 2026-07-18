namespace Orion.Framework.Metadata;

/// <summary>Defines insert and update validation requirements.</summary>
public sealed record ValidationDefinition(IReadOnlyList<ColumnDefinition> RequiredForInsert, IReadOnlyList<ColumnDefinition> RequiredForUpdate);
