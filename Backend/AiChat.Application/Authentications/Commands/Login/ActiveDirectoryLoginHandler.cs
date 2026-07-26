namespace AiChat.Application.Authentications.Commands.Login;

using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;
using AiChat.Domain.Entities;

public sealed class ActiveDirectoryLoginHandler
{
    private readonly IActiveDirectoryAuthService _adAuth;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;

    public ActiveDirectoryLoginHandler(
        IActiveDirectoryAuthService adAuth,
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        ITokenService tokenService)
    {
        _adAuth = adAuth;
        _users = users;
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
    }

    public async Task<LoginResultDto?> HandleAsync(LoginCommand command, CancellationToken ct)
    {
        var adUser = await _adAuth.ValidateAsync(command.UserName, command.Password, ct);

        if (adUser is null)
            return null;

        var user = await _users.FindByExternalIdAsync(
            AiChat.Application.Common.Enums.AuthenticationProviderEnum.ActiveDirectory,
            adUser.ExternalId,
            ct);

        if (user is null)
        {
            user = User.CreateUser
            (
                adUser.UserName,
                null,
                adUser.ExternalId,
                "ActiveDirectory"
            );

            _users.Add(user);
            await _users.SaveChangesAsync(ct);
        }

        var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user, user.Roles);
        var (refreshToken, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        }, ct);

        await _refreshTokens.SaveChangesAsync(ct);

        return new LoginResultDto
        {
            access_token = accessToken,
            refresh_token = refreshToken,
            UserName = user.UserName,
            ExpiresAt = accessTokenExpiresAt
        };
    }
}

