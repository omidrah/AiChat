using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;
using AiChat.Application.Common;
using System.Security.Claims;

namespace AiChat.Api.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public CurrentUser? GetCurrentUser()
        {
            var principal = Principal;

            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var identity = principal.Identity;

            var providerValue = principal.FindFirstValue("auth_provider");

            if (!Enum.TryParse<AuthenticationProviderEnum>(providerValue, true, out var provider))
            {
                provider = AuthenticationProviderEnum.Local;
            }

            var userName =
                principal.FindFirstValue(ClaimTypes.Name) ??
                principal.FindFirstValue("name") ??
                identity.Name;


            if (string.IsNullOrWhiteSpace(userName))
                throw new UnauthorizedAccessException("User name was not found.");

            string? externalId = null; //in AD mode
            Guid? userId = null; //in local mode

            if (provider == AuthenticationProviderEnum.Local)
            {
                // JWT صادر شده توسط خود AiChat

                var userIdInClaim =
                    principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    principal.FindFirstValue("sub") ??
                    principal.FindFirstValue("userId") ??
                    throw new UnauthorizedAccessException("JWT does not contain user id.");

                if (!Guid.TryParse(userIdInClaim, out var parsedUserId))
                    throw new UnauthorizedAccessException("JWT user id is invalid.");
                userId = parsedUserId;
            }
            else
            {
                // Active Dirctory Authentication
                externalId = principal.FindFirstValue("external_id")
                    ?? throw new UnauthorizedAccessException("ExternalId claim was not found.");
            }

            var roles = principal.Claims
                   .Where(x => x.Type == ClaimTypes.Role)
                   .Select(x => x.Value)
                   .ToArray();


            return new CurrentUser
            {
                AuthProvider = provider,
                UserId = userId,
                ExternalId = externalId!,
                UserName = userName,
                DisplayName = principal.FindFirstValue(ClaimTypes.GivenName) ?? userName,
                Roles = roles
            };
        }
    }
}
