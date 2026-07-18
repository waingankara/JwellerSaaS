using Microsoft.AspNetCore.Identity;

namespace Orion.Framework.Identity;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string passwordHash, string password);
}

public sealed class MicrosoftPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> hasher = new();
    private static readonly object User = new();
    public string HashPassword(string password) => hasher.HashPassword(User, password);
    public bool VerifyPassword(string passwordHash, string password) => hasher.VerifyHashedPassword(User, passwordHash, password) != PasswordVerificationResult.Failed;
}

public interface IPasswordHistoryValidator
{
    Task<bool> IsAllowedAsync(string password, IReadOnlyCollection<string> previousPasswordHashes, CancellationToken cancellationToken);
}

public sealed class PasswordHistoryValidator(IPasswordHasher passwordHasher) : IPasswordHistoryValidator
{
    public Task<bool> IsAllowedAsync(string password, IReadOnlyCollection<string> previousPasswordHashes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(!previousPasswordHashes.Any(hash => passwordHasher.VerifyPassword(hash, password)));
    }
}

public sealed record RefreshTokenIssueRequest(long UserId, long TenantId, string Token, DateTimeOffset ExpiresAt);
public sealed record StoredRefreshToken(long Id, long UserId, long TenantId, string TokenHash, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt);

public interface IRefreshTokenStore
{
    Task IssueAsync(RefreshTokenIssueRequest request, CancellationToken cancellationToken);
    Task RevokeAsync(string tokenHash, CancellationToken cancellationToken);
    Task RotateAsync(string currentTokenHash, RefreshTokenIssueRequest nextToken, CancellationToken cancellationToken);
    Task ExpireAsync(DateTimeOffset utcNow, CancellationToken cancellationToken);
}
