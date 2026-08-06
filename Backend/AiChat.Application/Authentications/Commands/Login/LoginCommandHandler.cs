using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;
using AiChat.Application.Common.Enums;
using AiChat.Domain.Entities;

namespace AiChat.Application.Authentications.Commands.Login
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IActiveDirectoryAuthService _adAuth;

        public LoginCommandHandler(
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            IActiveDirectoryAuthService adAuth)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _adAuth = adAuth;
        }

        public async Task<LoginResultDto?> HandleAsync(LoginCommand command, bool isActiveDirectoryEnabled, CancellationToken ct)
        {

            User? user = null;
            if (isActiveDirectoryEnabled)
            {
                var adUser = await _adAuth.ValidateAsync(command.UserName, command.Password, ct);

                if (adUser is null)
                    return null;

                 user = await _users.FindByExternalIdAsync(
                    AuthenticationProviderEnum.ActiveDirectory,
                    adUser.ExternalId,
                    ct);

                if (user is null)
                {
                    user = User.CreateUser
                    (
                        adUser.UserName,
                        string.Empty,
                        adUser.DisplayName,
                        adUser.ExternalId,
                        AuthenticationProviderEnum.ActiveDirectory.ToString()
                    );

                    _users.Add(user);
                    await _users.SaveChangesAsync(ct);
                }

            }
            if (user is null)
            {
                var localUser = await _users.GetByUserNameAsync(command.UserName, ct);

                var isValidPassword = _passwordHasher.Verify(command.Password, localUser.PasswordHash);

                if (isValidPassword)
                {
                    user = localUser;
                }
            }

            // اگر در هیچ‌کدام از روش‌ها احراز هویت با موفقیت انجام نشد
            if (user is null)
                return null;

            var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user, user.Roles);
            var (refreshToken, refreshTokenExpiresAt) =  _tokenService.GenerateRefreshToken();

            var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);

            await _refreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
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
}
