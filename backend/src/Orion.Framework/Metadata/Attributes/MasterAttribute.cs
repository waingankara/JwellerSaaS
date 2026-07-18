namespace Orion.Framework.Metadata.Attributes;

/// <summary>Marks a type as a framework master entity.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MasterAttribute(string tableName) : Attribute
{
    /// <summary>Gets the mapped table name.</summary>
    public string TableName { get; } = tableName;
}
