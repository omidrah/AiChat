using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;
using AiChat.Domain.Entities;

namespace AiChat.Application.Authentications.Commands.Login
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginCommandHandler(
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResultDto?> HandleAsync(LoginCommand command, CancellationToken ct)
        {
            var user = await _users.GetByUserNameAsync(command.UserName,ct);

            if (user is null)
                return null;

             var isValid = _passwordHasher.Verify(command.Password, user.PasswordHash);

            if (!isValid)
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
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserName = user.UserName,
                ExpiresAt = accessTokenExpiresAt
            };
        }
    }
}
