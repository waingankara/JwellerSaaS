namespace Orion.Framework.Security;

/// <summary>Represents the authenticated user for the current request.</summary>
public sealed record CurrentUser(long? UserId, string? UserName, IReadOnlySet<string> Roles)
{
    /// <summary>Gets an unauthenticated user context.</summary>
    public static CurrentUser Anonymous { get; } = new(null, null, new HashSet<string>());
}
