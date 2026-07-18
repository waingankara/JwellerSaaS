namespace Orion.Framework.Security;

/// <summary>Provides access to the current request user.</summary>
public interface ICurrentUserAccessor
{
    /// <summary>Gets the current user.</summary>
    CurrentUser CurrentUser { get; }
}
