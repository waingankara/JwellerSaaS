using Orion.Framework.Metadata.Attributes;

namespace JwellerSaaS.Domain.Transactions;

[Transactional("product_variant")]
public sealed class ProductVariant
{
    [PrimaryKey]
    [DbColumn("product_variant_id")]
    public long ProductVariantId { get; set; }

    [DbColumn("tenant_id")]
    public long TenantId { get; set; }

    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("product_id")]
    public long ProductId { get; set; }

    [Searchable]
    [DuplicateCheck]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("sku")]
    public string Sku { get; set; } = string.Empty;

    [Searchable]
    [DbColumn("variant_name")]
    public string VariantName { get; set; } = string.Empty;

    [DbColumn("unit_id")]
    public long? UnitId { get; set; }

    [DbColumn("is_active")]
    public bool IsActive { get; set; } = true;

    [DbColumn("created_by")]
    public long? CreatedBy { get; set; }

    [DbColumn("created_date")]
    public DateTimeOffset? CreatedDate { get; set; }

    [DbColumn("modified_by")]
    public long? ModifiedBy { get; set; }

    [DbColumn("modified_date")]
    public DateTimeOffset? ModifiedDate { get; set; }

    [DbColumn("deleted_by")]
    public long? DeletedBy { get; set; }

    [DbColumn("deleted_date")]
    public DateTimeOffset? DeletedDate { get; set; }

    [DbColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbColumn("row_version")]
    public Guid RowVersion { get; set; }

    [DbColumn("is_system")]
    public bool IsSystem { get; set; }
}