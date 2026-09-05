using Orion.Framework.Security;

namespace JwellerSaaS.Application.Identity;

public interface IIdentityRepository
{
    Task<CurrentUser?> FindUserAsync(
        string username,
        long tenantId,
        CancellationToken cancellationToken = default);

    Task<string?> GetPasswordHashAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
