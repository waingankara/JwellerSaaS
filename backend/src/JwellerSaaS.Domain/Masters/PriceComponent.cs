using Orion.Framework.Metadata.Attributes;

namespace JwellerSaaS.Domain.Masters;

[Master("price_component")]
public sealed class PriceComponent
{
    [PrimaryKey]
    [DbColumn("price_component_id")]
    public long PriceComponentId { get; set; }

    [DbColumn("tenant_id")]
    public long TenantId { get; set; }

    [Searchable]
    [DuplicateCheck]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("price_component_code")]
    public string PriceComponentCode { get; set; } = string.Empty;

    [Searchable]
    [DuplicateCheck]
    [DropdownColumn]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("price_component_name")]
    public string PriceComponentName { get; set; } = string.Empty;

    [DbColumn("display_order")]
    public int DisplayOrder { get; set; }

    [DbColumn("remarks")]
    public string? Remarks { get; set; }

    [DbColumn("is_active")]
    public bool IsActive { get; set; } = true;

    [DbColumn("created_by")]
    public long? CreatedBy { get; set; }

    [Searchable]
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

    [Searchable]
    [DbColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbColumn("row_version")]
    public Guid RowVersion { get; set; }

    [DbColumn("is_system")]
    public bool IsSystem { get; set; }
}