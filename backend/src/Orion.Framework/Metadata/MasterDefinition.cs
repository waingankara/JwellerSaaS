namespace Orion.Framework.Metadata;

/// <summary>Defines framework metadata for a registered master entity.</summary>
public sealed record MasterDefinition(Type EntityType, string EntityName, string TableName, Type? KeyType, IReadOnlyList<ColumnDefinition> Columns, SearchDefinition Search, DuplicateDefinition Duplicate, AuditDefinition Audit, PermissionDefinition Permission, TenantDefinition Tenant, ValidationDefinition Validation);
