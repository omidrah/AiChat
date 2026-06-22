using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;
using AiChat.Application.Common.Auth;

namespace AiChat.Application.Authentications.Commands.Login
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginCommandHandler(IUserRepository users,ITokenService tokenService, IPasswordHasher passwordHasher)
        {
            _users = users;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResultDto?> HandleAsync(LoginCommand command, CancellationToken ct)
        {
            var user = await _users.GetByUserNameAsync(command.UserName);

            if (user is null)
                return null;

             var isValid = _passwordHasher.Verify(command.Password, user.PasswordHash);

            if (!isValid)
                return null;

            var token = _tokenService.CreateToken(
                user.Id.ToString(),
                user.UserName,
                user.Roles);

            return new LoginResultDto
            {
                AccessToken = token,
                UserName = user.UserName,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };
        }
    }
}
