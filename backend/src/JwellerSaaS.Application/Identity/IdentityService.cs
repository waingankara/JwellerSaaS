using JwellerSaaS.Contracts.Identity;
using Orion.Framework.Authentication;
using Orion.Framework.Identity;

namespace JwellerSaaS.Application.Identity;

public sealed class IdentityService(
    IIdentityRepository identityRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator)
    : IIdentityService
{
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var username = request.Username.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException(
                "Username is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "Password is required.",
                nameof(request));
        }

        var user =
            await identityRepository.FindUserAsync(
                username,
                request.TenantId,
                cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var passwordHash =
            await identityRepository.GetPasswordHashAsync(
                user.UserId!.Value,
                cancellationToken)
            .ConfigureAwait(false);

        if (passwordHash is null ||
            !passwordHasher.VerifyPassword(
                passwordHash,
                request.Password))
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var tokenPair =
            tokenGenerator.Generate(user);

        return new LoginResponse(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshTokenExpiresAt);
    }
}
