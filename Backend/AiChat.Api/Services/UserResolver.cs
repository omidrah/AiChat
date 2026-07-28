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

        public async Task<User> GetCurrentUserAsync( CancellationToken ct = default)                
        {
            var current = _currentUserService.GetCurrentUser();

            if (current == null)
                throw new UnauthorizedAccessException();

            if (current.AuthProvider == AuthenticationProviderEnum.Local)
            {
                var localUser = await _userRepository.GetByIdAsync(current.UserId!.Value, ct);

                if (localUser is null || !localUser.IsActive)
                    throw new UnauthorizedAccessException("Local user was not found.");

                return localUser;
            }

            var user = await _userRepository.FindByExternalIdAsync(current.AuthProvider, current.ExternalId!, ct);

            if (user != null)
                return user;

            user = User.CreateUser(
                current.UserName,
                null,
                current.DisplayName,
                current.ExternalId ?? null,
                current.AuthProvider.ToString());

            _userRepository.Add(user);

            await _userRepository.SaveChangesAsync(ct);

            return user;
        }
    }
}
