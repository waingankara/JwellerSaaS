namespace Orion.Framework.Metadata;

/// <summary>Defines detected audit columns.</summary>
public sealed record AuditDefinition(ColumnDefinition? CreatedBy, ColumnDefinition? CreatedDate, ColumnDefinition? ModifiedBy, ColumnDefinition? ModifiedDate, ColumnDefinition? DeletedBy, ColumnDefinition? DeletedDate, ColumnDefinition? IsDeleted);
