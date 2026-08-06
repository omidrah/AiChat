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
            /*
             * مرحله اول:
             * ابتدا کاربر را از دیتابیس پیدا می‌کنیم.
             * این کار باعث می‌شود کاربر Local درگیر تأخیر AD نشود.
             */
            var existingUser = await _users.GetByUserNameAsync(command.UserName, ct);
            if (existingUser is not null)
            {
                /*
                 * کاربران Local باید PasswordHash داشته باشند.
                 * کاربران AD که به‌صورت خودکار ساخته شده‌اند،
                 * معمولاً PasswordHash خالی دارند.
                 */
                if (!string.IsNullOrWhiteSpace(existingUser.PasswordHash))
                {
                    var isLocalPasswordValid = _passwordHasher.Verify(
                        command.Password,
                        existingUser.PasswordHash);

                    if (isLocalPasswordValid)
                    {
                        user = existingUser;
                    }
                    else
                    {
                        /*
                         * کاربر Local است اما رمز اشتباه است.
                         * برای جلوگیری از احراز هویت ناخواسته با AD،
                         * دیگر سراغ AD نمی‌رویم.
                         */
                        return null;
                    }
                }
            }
            /*
             * اگر کاربر Local با رمز صحیح پیدا نشد،
             * فقط در صورت فعال بودن AD، آن را بررسی می‌کنیم.
             */

            if (user is null && isActiveDirectoryEnabled)
            {
                var adUser = await _adAuth.ValidateAsync(command.UserName, command.Password, ct);

                if (adUser is not null)
                {

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
            }

            // اگر در هیچ‌کدام از روش‌ها احراز هویت با موفقیت انجام نشد
            if (user is null)
                return null;

            return await CreateLoginResultAsync(user, ct);
        }

        private async Task<LoginResultDto?> CreateLoginResultAsync(User user, CancellationToken ct)
        {
            var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user, user.Roles);
            var (refreshToken, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();

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
