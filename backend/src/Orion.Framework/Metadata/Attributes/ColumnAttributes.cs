namespace Orion.Framework.Metadata.Attributes;

/// <summary>Overrides the database column name for a property.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DbColumnAttribute(string columnName) : Attribute { public string ColumnName { get; } = columnName; }
/// <summary>Excludes a property from persistence metadata.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreColumnAttribute : Attribute { }
/// <summary>Marks a property as searchable.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SearchableAttribute : Attribute { }
/// <summary>Marks a property as participating in duplicate checks.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DuplicateCheckAttribute : Attribute { }
/// <summary>Marks a property as required for insert validation.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredForInsertAttribute : Attribute { }
/// <summary>Marks a property as required for update validation.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredForUpdateAttribute : Attribute { }
/// <summary>Marks a property as a dropdown display column.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DropdownColumnAttribute : Attribute { }
/// <summary>Excludes a property from export operations.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ExportIgnoreAttribute : Attribute { }
/// <summary>Excludes a property from import operations.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ImportIgnoreAttribute : Attribute { }
/// <summary>Marks a property as the primary key.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PrimaryKeyAttribute : Attribute { }
