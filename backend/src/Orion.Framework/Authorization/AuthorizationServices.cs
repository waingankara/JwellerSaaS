using Microsoft.AspNetCore.Authorization;
using Orion.Framework.Security;

namespace Orion.Framework.Authorization;

/// <summary>
/// Defines standard Orion permission names.
/// </summary>
public static class PermissionConstants
{
    public const string Administrator =
        "admin.full_access";

    public const string IdentityRead =
        "identity.read";

    public const string IdentityWrite =
        "identity.write";

    public const string TenantRead =
        "tenant.read";

    public const string TenantWrite =
        "tenant.write";
}

/// <summary>
/// Provides permission evaluation for the current user.
/// </summary>
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        CurrentUser user,
        string permission,
        CancellationToken cancellationToken);
}

/// <summary>
/// Provides Orion authorization operations.
/// </summary>
public interface IOrionAuthorizationService
{
    Task<bool> AuthorizePermissionAsync(
        CurrentUser user,
        string permission,
        CancellationToken cancellationToken);

    Task<bool> AuthorizeRoleAsync(
        CurrentUser user,
        string role,
        CancellationToken cancellationToken);
}

/// <summary>
/// Default implementation of permission evaluation.
/// </summary>
public sealed class PermissionService : IPermissionService
{
    /// <inheritdoc />
    public Task<bool> HasPermissionAsync(
        CurrentUser user,
        string permission,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            permission);

        cancellationToken.ThrowIfCancellationRequested();

        var isAdministrator =
            user.Permissions.Contains(
                PermissionConstants.Administrator);

        var hasPermission =
            user.Permissions.Contains(
                permission);

        var allowed =
            isAdministrator ||
            hasPermission;

        return Task.FromResult(
            allowed);
    }
}

/// <summary>
/// Default Orion authorization service.
/// </summary>
public sealed class OrionAuthorizationService(
    IPermissionService permissionService)
    : IOrionAuthorizationService
{
    /// <inheritdoc />
    public Task<bool> AuthorizePermissionAsync(
        CurrentUser user,
        string permission,
        CancellationToken cancellationToken)
    {
        return permissionService.HasPermissionAsync(
            user,
            permission,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> AuthorizeRoleAsync(
        CurrentUser user,
        string role,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            role);

        cancellationToken.ThrowIfCancellationRequested();

        var hasRole =
            user.Roles.Contains(role);

        return Task.FromResult(
            hasRole);
    }
}

/// <summary>
/// Represents an authorization requirement
/// for a specific permission.
/// </summary>
public sealed class PermissionRequirement(
    string permission)
    : IAuthorizationRequirement
{
    public string Permission { get; } =
        permission;
}

/// <summary>
/// Handles permission-based authorization requirements.
/// </summary>
public sealed class PermissionHandler(
    ICurrentUserAccessor currentUserAccessor,
    IPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var currentUser =
            currentUserAccessor.CurrentUser;

        var allowed =
            await permissionService
                .HasPermissionAsync(
                    currentUser,
                    requirement.Permission,
                    CancellationToken.None)
                .ConfigureAwait(false);

        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}
