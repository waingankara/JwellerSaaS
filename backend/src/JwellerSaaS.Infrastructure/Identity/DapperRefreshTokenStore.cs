using System.Security.Cryptography;
using System.Text;
using Dapper;
using Orion.Framework.Data;
using Orion.Framework.Identity;

namespace JwellerSaaS.Infrastructure.Identity;

public sealed class DapperRefreshTokenStore(IConnectionFactory connectionFactory) : IRefreshTokenStore
{
    public async Task IssueAsync(RefreshTokenIssueRequest request, CancellationToken cancellationToken)
    {
        const string sql = "insert into refresh_token (user_id, tenant_id, token_hash, expires_at, created_by, created_date, is_deleted, row_version) values (@UserId, @TenantId, @TokenHash, @ExpiresAt, @UserId, @CreatedDate, false, 1);";
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new { request.UserId, request.TenantId, TokenHash = Hash(request.Token), request.ExpiresAt, CreatedDate = DateTimeOffset.UtcNow }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task RevokeAsync(string tokenHash, CancellationToken cancellationToken)
    {
        const string sql = "update refresh_token set revoked_at = @RevokedAt, modified_date = @RevokedAt where token_hash = @TokenHash and revoked_at is null;";
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new { TokenHash = tokenHash, RevokedAt = DateTimeOffset.UtcNow }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task RotateAsync(string currentTokenHash, RefreshTokenIssueRequest nextToken, CancellationToken cancellationToken)
    {
        const string sql = "update refresh_token set revoked_at = @Now, replaced_by_token_hash = @NextHash, modified_date = @Now where token_hash = @CurrentHash and revoked_at is null; insert into refresh_token (user_id, tenant_id, token_hash, expires_at, created_by, created_date, is_deleted, row_version) values (@UserId, @TenantId, @NextHash, @ExpiresAt, @UserId, @Now, false, 1);";
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new { CurrentHash = currentTokenHash, NextHash = Hash(nextToken.Token), nextToken.UserId, nextToken.TenantId, nextToken.ExpiresAt, Now = DateTimeOffset.UtcNow }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task ExpireAsync(DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
        const string sql = "update refresh_token set revoked_at = @Now, modified_date = @Now where expires_at <= @Now and revoked_at is null;";
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Now = utcNow }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
