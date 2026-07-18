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

public sealed class PermissionHandler(ICurrentUserAccessor currentUserAccessor, IPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var allowed = await permissionService.HasPermissionAsync(currentUserAccessor.CurrentUser, requirement.Permission, CancellationToken.None).ConfigureAwait(false);
        if (allowed) context.Succeed(requirement);
    }
}
