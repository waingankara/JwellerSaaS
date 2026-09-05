using JwellerSaaS.Contracts.Identity;

namespace JwellerSaaS.Application.Identity;

public interface IIdentityService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
