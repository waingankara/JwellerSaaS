namespace Orion.Framework.Metadata;

/// <summary>Defines a mapped database column.</summary>
public sealed record ColumnDefinition(string PropertyName, Type PropertyType, string ColumnName, bool IsPrimaryKey, bool IsIgnored, bool IsSearchable, bool IsDuplicateCheck, bool IsRequiredForInsert, bool IsRequiredForUpdate, bool IsDropdownColumn, bool IsExportIgnored, bool IsImportIgnored);
