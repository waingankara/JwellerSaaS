using JwellerSaaS.Domain.Common;

namespace JwellerSaaS.Domain.Identity;

public sealed class User : BaseAuditEntity
{
    public long TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Language { get; set; }
    public string? Timezone { get; set; }
}

public sealed class Role : BaseAuditEntity
{
    public long TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
}

public sealed class Permission : BaseAuditEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public sealed class UserRole : BaseAuditEntity { public long UserId { get; set; } public long RoleId { get; set; } }
public sealed class RolePermission : BaseAuditEntity { public long RoleId { get; set; } public long PermissionId { get; set; } }
public sealed class UserBranch : BaseAuditEntity { public long UserId { get; set; } public long BranchId { get; set; } public bool IsDefault { get; set; } }

public sealed class PasswordHistory : BaseAuditEntity
{
    public long UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
}

public sealed class RefreshToken : BaseAuditEntity
{
    public long UserId { get; set; }
    public long TenantId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
