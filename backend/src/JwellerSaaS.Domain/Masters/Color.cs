using Orion.Framework.Metadata.Attributes;

namespace JwellerSaaS.Domain.Masters;

[Master("color")]
public sealed class Color
{
    [PrimaryKey]
    [DbColumn("color_id")]
    public long ColorId { get; set; }

    [Searchable]
    [DuplicateCheck]
    [RequiredForInsert]
    [RequiredForUpdate]
    [DropdownColumn]
    [DbColumn("color_name")]
    public string ColorName { get; set; } = string.Empty;

    [DbColumn("is_active")]
    public bool IsActive { get; set; } = true;
}
