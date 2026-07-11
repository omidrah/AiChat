using AiChat.Application.Abstractions;
using AiChat.Application.Common.Auth;
using AiChat.Application.Common.Enums;
using AiChat.Domain.Entities;

namespace AiChat.Api.Services
{
    public sealed class UserResolver : IUserResolver
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;


        public UserResolver(
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        public async Task<User> GetCurrentUserAsync(
            CancellationToken ct = default)
        {
            var current =
                _currentUserService.GetCurrentUser();


            if (current == null)
                throw new UnauthorizedAccessException();



            // -----------------------------
            // Local JWT
            // -----------------------------
            if (current.AuthProvider == AuthenticationProviderEnum.Local)
            {
                if (!Guid.TryParse(
                    current.ExternalId,
                    out var userId))
                {
                    throw new UnauthorizedAccessException(
                        "Invalid user id in token.");
                }


                var user =
                    await _userRepository
                        .GetByIdAsync(userId, ct);


                if (user == null)
                {
                    throw new UnauthorizedAccessException(
                        "User does not exist.");
                }


                return user;
            }



            // -----------------------------
            // Windows Authentication
            // -----------------------------
            if (current.AuthProvider == AuthenticationProviderEnum.Windows)
            {

                var user =
                    await _userRepository
                        .FindByExternalIdAsync(
                            AuthenticationProviderEnum.Windows,
                            current.ExternalId,
                            ct);

                if (user != null)
                    return user;

                // اولین ورود Windows User
                user = new User
                {
                    Id = Guid.NewGuid(),

                    UserName = ExtractUserName(
                        current.UserName),

                    DisplayName = current.DisplayName,

                    ExternalId = current.ExternalId,

                    AuthProvider =
                        AuthenticationProviderEnum.Windows
                        .ToString(),

                    IsActive = true
                };


                await _userRepository.AddAsync(
                    user,
                    ct);


                await _userRepository.SaveChangesAsync(ct);


                return user;
            }


            throw new UnauthorizedAccessException(
                "Unsupported authentication provider.");
        }



        private static string ExtractUserName(
            string userName)
        {
            // COMPANY\omid
            // تبدیل به omid

            var index = userName.LastIndexOf('\\');

            return index >= 0
                ? userName[(index + 1)..]
                : userName;
        }
    }
}
