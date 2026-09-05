using Orion.Framework.Metadata.Attributes;

namespace JwellerSaaS.Domain.Masters;

[Master("making_charge_type")]
public sealed class MakingChargeType
{
    [PrimaryKey]
    [DbColumn("making_charge_type_id")]
    public long MakingChargeTypeId { get; set; }

    [DbColumn("tenant_id")]
    public long TenantId { get; set; }

    [Searchable]
    [DuplicateCheck]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("making_charge_type_code")]
    public string MakingChargeTypeCode { get; set; } = string.Empty;

    [Searchable]
    [DuplicateCheck]
    [DropdownColumn]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("making_charge_type_name")]
    public string MakingChargeTypeName { get; set; } = string.Empty;

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