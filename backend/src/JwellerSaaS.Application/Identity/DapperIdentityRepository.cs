using Dapper;
using JwellerSaaS.Application.Identity;
using Orion.Framework.Data;
using Orion.Framework.Security;

namespace JwellerSaaS.Infrastructure.Identity;

public sealed class DapperIdentityRepository(
    IConnectionFactory connectionFactory)
    : IIdentityRepository
{
    public async Task<CurrentUser?> FindUserAsync(
        string username,
        long tenantId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            select
                u.user_id as UserId,
                u.username as Username,
                u.email as Email,
                u.tenant_id as TenantId,
                ub.branch_id as BranchId,
                u.language as Language,
                u.timezone as Timezone,
                coalesce(
                    array_agg(distinct r.role_code)
                        filter (where r.role_id is not null),
                    '{}'
                ) as Roles,
                coalesce(
                    array_agg(distinct p.permission_code)
                        filter (where p.permission_id is not null),
                    '{}'
                ) as Permissions
            from "user" u
            left join user_role ur
                on ur.user_id = u.user_id
                and ur.is_deleted = false
            left join role r
                on r.role_id = ur.role_id
                and r.is_deleted = false
            left join role_permission rp
                on rp.role_id = r.role_id
                and rp.is_deleted = false
            left join permission p
                on p.permission_id = rp.permission_id
                and p.is_deleted = false
            left join user_branch ub
                on ub.user_id = u.user_id
                and ub.is_deleted = false
                and ub.is_default = true
            where
                u.tenant_id = @TenantId
                and upper(u.username) = upper(@Username)
                and u.is_active = true
                and u.is_deleted = false
            group by
                u.user_id,
                u.username,
                u.email,
                u.tenant_id,
                ub.branch_id,
                u.language,
                u.timezone;
            """;

        using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        var row =
            await connection.QuerySingleOrDefaultAsync<IdentityUserRow>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        TenantId = tenantId,
                        Username = username
                    },
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false);

        if (row is null)
        {
            return null;
        }

        return new CurrentUser(
            row.UserId,
            row.Username,
            row.Email,
            row.TenantId,
            row.BranchId,
            new HashSet<string>(
                row.Roles ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(
                row.Permissions ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase),
            row.Language,
            row.Timezone);
    }

    public async Task<string?> GetPasswordHashAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            select password_hash
            from "user"
            where
                user_id = @UserId
                and is_active = true
                and is_deleted = false;
            """;

        using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                sql,
                new { UserId = userId },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    private sealed class IdentityUserRow
    {
        public long UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public long TenantId { get; init; }
        public long? BranchId { get; init; }
        public string? Language { get; init; }
        public string? Timezone { get; init; }
        public string[]? Roles { get; init; }
        public string[]? Permissions { get; init; }
    }
}
