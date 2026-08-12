using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Orion.Framework.Security;

namespace Orion.Framework.Authorization;

public static class PermissionConstants
{
    public const string Administrator = "admin.full_access";
    public const string IdentityRead = "identity.read";
    public const string IdentityWrite = "identity.write";
    public const string TenantRead = "tenant.read";
    public const string TenantWrite = "tenant.write";
}

public interface IPermissionService { Task<bool> HasPermissionAsync(CurrentUser user, string permission, CancellationToken cancellationToken); }
public interface IOrionAuthorizationService
{
    Task<bool> AuthorizePermissionAsync(CurrentUser user, string permission, CancellationToken cancellationToken);
    Task<bool> AuthorizeRoleAsync(CurrentUser user, string role, CancellationToken cancellationToken);
}

public sealed class PermissionService : IPermissionService
{
    public Task<bool> HasPermissionAsync(CurrentUser user, string permission, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        cancellationToken.ThrowIfCancellationRequested();
        var allowed = user.Permissions.Contains(PermissionConstants.Administrator) || user.Permissions.Contains(permission);
        return Task.FromResult(allowed);
    }
}

public sealed class OrionAuthorizationService(IPermissionService permissionService) : IOrionAuthorizationService
{
    public Task<bool> AuthorizePermissionAsync(CurrentUser user, string permission, CancellationToken cancellationToken) => permissionService.HasPermissionAsync(user, permission, cancellationToken);
    public Task<bool> AuthorizeRoleAsync(CurrentUser user, string role, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Roles.Contains(role));
    }
}

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PermissionHandler(IPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = CurrentUserFromPrincipal(context.User);
        var allowed = await permissionService.HasPermissionAsync(user, requirement.Permission, CancellationToken.None).ConfigureAwait(false);
        if (allowed) context.Succeed(requirement);
    }

    private static CurrentUser CurrentUserFromPrincipal(ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub") ?? string.Empty;
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email") ?? string.Empty;
        var tenantId = long.TryParse(principal.FindFirstValue("tenant_id"), out var parsedTenantId) ? parsedTenantId : 0L;
        var branchId = long.TryParse(principal.FindFirstValue("branch_id"), out var parsedBranchId) ? parsedBranchId : null;
        var roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Concat(principal.FindAll("role").Select(claim => claim.Value)).ToArray();
        var permissions = principal.FindAll("permission").Select(claim => claim.Value).Concat(principal.FindAll("permissions").Select(claim => claim.Value)).ToArray();
        return new CurrentUser(long.TryParse(id, out var parsedUserId) ? parsedUserId : null, email, email, tenantId == 0L ? null : tenantId, branchId, roles.ToHashSet(StringComparer.Ordinal), permissions.ToHashSet(StringComparer.Ordinal), null, null);
    }
}
