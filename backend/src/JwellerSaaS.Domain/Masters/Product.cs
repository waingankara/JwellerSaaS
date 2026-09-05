using Orion.Framework.Metadata.Attributes;

namespace JwellerSaaS.Domain.Masters;

/// <summary>
/// Represents a jewellery product/design within a tenant catalogue.
/// </summary>
[Master("product")]
public sealed class Product
{
    [PrimaryKey]
    [DbColumn("product_id")]
    public long ProductId { get; set; }

    [DbColumn("tenant_id")]
    public long TenantId { get; set; }

    [Searchable]
    [DuplicateCheck]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("subcategory_id")]
    public long SubCategoryId { get; set; }

    [Searchable]
    [DuplicateCheck]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("product_code")]
    public string ProductCode { get; set; } = string.Empty;

    [Searchable]
    [DuplicateCheck]
    [DropdownColumn]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DbColumn("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Searchable]
    [DbColumn("description")]
    public string? Description { get; set; }

    [DbColumn("product_type_id")]
    public long? ProductTypeId { get; set; }

    [DbColumn("brand_id")]
    public long? BrandId { get; set; }

    [DbColumn("collection_id")]
    public long? CollectionId { get; set; }

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