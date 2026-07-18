namespace Orion.Framework.Metadata;

/// <summary>Canonical audit column names detected by Orion.</summary>
public static class AuditColumnNames
{
    public const string CreatedBy = nameof(CreatedBy);
    public const string CreatedDate = nameof(CreatedDate);
    public const string ModifiedBy = nameof(ModifiedBy);
    public const string ModifiedDate = nameof(ModifiedDate);
    public const string DeletedBy = nameof(DeletedBy);
    public const string DeletedDate = nameof(DeletedDate);
    public const string IsDeleted = nameof(IsDeleted);
}

/// <summary>Canonical tenancy column names detected by Orion.</summary>
public static class TenantColumnNames
{
    public const string TenantId = nameof(TenantId);
    public const string BranchId = nameof(BranchId);
}
