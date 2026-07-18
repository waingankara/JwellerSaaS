namespace Orion.Framework.Permissions;

/// <summary>Checks framework permissions for a resource and action.</summary>
public interface IPermissionEngine
{
    /// <summary>Determines whether the current user can perform an action.</summary>
    Task<bool> HasPermissionAsync(string resourceName, string actionName, CancellationToken cancellationToken);
}
