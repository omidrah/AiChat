using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;

namespace AiChat.Application.Authentications.Queries.GetCurrentUser
{
    public  class GetCurrentUserQueryHandler
    {
        private readonly ICurrentUserService _currentUserService;

        public GetCurrentUserQueryHandler(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }
        public async Task<CurrentUserDto?> HandleAsync(GetCurrentUserQuery query, CancellationToken ct)
        {
            var user = _currentUserService.GetCurrentUser();

            if (user is null)
                throw new UnauthorizedAccessException();

            return new CurrentUserDto
            {
                AuthProvider = user.AuthProvider,
                ExternalId = user.ExternalId,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Roles = user.Roles
            };
        }
    }
}
