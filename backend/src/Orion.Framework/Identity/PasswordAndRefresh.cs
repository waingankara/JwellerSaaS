using Microsoft.AspNetCore.Identity;

namespace Orion.Framework.Identity;

/// <summary>
/// Provides password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(
        string passwordHash,
        string password);
}

/// <summary>
/// Password hasher implementation using Microsoft's
/// ASP.NET Core password hashing algorithm.
/// </summary>
public sealed class MicrosoftPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> hasher = new();

    private static readonly object User = new();

    /// <inheritdoc />
    public string HashPassword(
        string password)
    {
        return hasher.HashPassword(
            User,
            password);
    }

    /// <inheritdoc />
    public bool VerifyPassword(
        string passwordHash,
        string password)
    {
        var result =
            hasher.VerifyHashedPassword(
                User,
                passwordHash,
                password);

        return result !=
               PasswordVerificationResult.Failed;
    }
}

/// <summary>
/// Validates a password against previously used passwords.
/// </summary>
public interface IPasswordHistoryValidator
{
    Task<bool> IsAllowedAsync(
        string password,
        IReadOnlyCollection<string> previousPasswordHashes,
        CancellationToken cancellationToken);
}

/// <summary>
/// Default password history validator.
/// </summary>
public sealed class PasswordHistoryValidator(
    IPasswordHasher passwordHasher)
    : IPasswordHistoryValidator
{
    /// <inheritdoc />
    public Task<bool> IsAllowedAsync(
        string password,
        IReadOnlyCollection<string> previousPasswordHashes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var passwordWasPreviouslyUsed =
            previousPasswordHashes.Any(
                hash =>
                    passwordHasher.VerifyPassword(
                        hash,
                        password));

        return Task.FromResult(
            !passwordWasPreviouslyUsed);
    }
}

/// <summary>
/// Represents a refresh token that is being issued.
/// </summary>
public sealed record RefreshTokenIssueRequest(
    long UserId,
    long TenantId,
    string Token,
    DateTimeOffset ExpiresAt);

/// <summary>
/// Represents a refresh token stored by the application.
/// </summary>
public sealed record StoredRefreshToken(
    long Id,
    long UserId,
    long TenantId,
    string TokenHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt);

/// <summary>
/// Provides persistence operations for refresh tokens.
/// </summary>
public interface IRefreshTokenStore
{
    Task IssueAsync(
        RefreshTokenIssueRequest request,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task RotateAsync(
        string currentTokenHash,
        RefreshTokenIssueRequest nextToken,
        CancellationToken cancellationToken);

    Task ExpireAsync(
        DateTimeOffset utcNow,
        CancellationToken cancellationToken);
}
