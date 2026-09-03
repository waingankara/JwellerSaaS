namespace Orion.Framework.Metadata.Attributes;

/// <summary>
/// Marks a domain entity as a transactional entity managed by the
/// Orion Transactional Framework.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TransactionalAttribute : Attribute
{
    /// <summary>
    /// Gets the database table name associated with the entity.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Initializes a new transactional entity metadata attribute.
    /// </summary>
    /// <param name="tableName">
    /// The database table name.
    /// </param>
    public TransactionalAttribute(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(
                "Table name cannot be null or empty.",
                nameof(tableName));
        }

        TableName = tableName;
    }
}