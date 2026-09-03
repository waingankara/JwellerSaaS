using System.Collections.Concurrent;
using System.Reflection;
using Orion.Framework.Metadata.Attributes;

namespace Orion.Framework.Metadata;

/// <summary>Thread-safe reflection metadata cache.</summary>
public sealed class ReflectionMetadataCache : IReflectionMetadataCache
{
    private static readonly StringComparer NameComparer = StringComparer.Ordinal;
    private readonly ConcurrentDictionary<Type, Lazy<MasterDefinition>> cache = new();

    /// <inheritdoc />
    public MasterDefinition GetOrAdd<T>() where T : class => GetOrAdd(typeof(T));

    /// <inheritdoc />
    public MasterDefinition GetOrAdd(Type entityType) => cache.GetOrAdd(entityType, type => new Lazy<MasterDefinition>(() => Build(type))).Value;

    private static MasterDefinition Build(Type entityType)
    {
        var master = entityType.GetCustomAttribute<MasterAttribute>();
        var transactional = entityType.GetCustomAttribute<TransactionalAttribute>();

        var columns = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => CreateColumn(property))
            .Where(column => !column.IsIgnored)
            .ToArray();
        var key = columns.FirstOrDefault(column => column.IsPrimaryKey);
        return new MasterDefinition(
            entityType,
            entityType.Name,
            master?.TableName
            ?? transactional?.TableName
            ?? entityType.Name,
            key?.PropertyType,
            columns,
            new SearchDefinition(columns.Where(c => c.IsSearchable).ToArray()),
            new DuplicateDefinition(columns.Where(c => c.IsDuplicateCheck).ToArray()),
            new AuditDefinition(Find(columns, nameof(AuditColumnNames.CreatedBy)), Find(columns, nameof(AuditColumnNames.CreatedDate)), Find(columns, nameof(AuditColumnNames.ModifiedBy)), Find(columns, nameof(AuditColumnNames.ModifiedDate)), Find(columns, nameof(AuditColumnNames.DeletedBy)), Find(columns, nameof(AuditColumnNames.DeletedDate)), Find(columns, nameof(AuditColumnNames.IsDeleted))),
            new PermissionDefinition(entityType.Name),
            new TenantDefinition(Find(columns, nameof(TenantColumnNames.TenantId)), Find(columns, nameof(TenantColumnNames.BranchId))),
            new ValidationDefinition(columns.Where(c => c.IsRequiredForInsert).ToArray(), columns.Where(c => c.IsRequiredForUpdate).ToArray()));
    }

    private static ColumnDefinition CreateColumn(PropertyInfo property)
    {
        var dbColumn = property.GetCustomAttribute<DbColumnAttribute>();
        return new ColumnDefinition(property.Name, property.PropertyType, dbColumn?.ColumnName ?? property.Name, property.GetCustomAttribute<PrimaryKeyAttribute>() is not null, property.GetCustomAttribute<IgnoreColumnAttribute>() is not null, property.GetCustomAttribute<SearchableAttribute>() is not null, property.GetCustomAttribute<DuplicateCheckAttribute>() is not null, property.GetCustomAttribute<RequiredForInsertAttribute>() is not null, property.GetCustomAttribute<RequiredForUpdateAttribute>() is not null, property.GetCustomAttribute<DropdownColumnAttribute>() is not null, property.GetCustomAttribute<ExportIgnoreAttribute>() is not null, property.GetCustomAttribute<ImportIgnoreAttribute>() is not null);
    }

    private static ColumnDefinition? Find(IEnumerable<ColumnDefinition> columns, string propertyName) => columns.FirstOrDefault(column => NameComparer.Equals(column.PropertyName, propertyName));
}
