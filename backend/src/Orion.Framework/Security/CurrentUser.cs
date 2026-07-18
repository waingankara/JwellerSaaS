namespace Orion.Framework.Security;

public sealed record CurrentUser(
    long? UserId,
    string? Username,
    string? Email,
    long? TenantId,
    long? BranchId,
    IReadOnlySet<string> Roles,
    IReadOnlySet<string> Permissions,
    string? Language,
    string? Timezone)
{
    public static CurrentUser Anonymous { get; } = new(null, null, null, null, null, new HashSet<string>(), new HashSet<string>(), null, null);
}
