using AiChat.Application.Common.Auth;
using AiChat.Application.Common.Enums;
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
        private ClaimsPrincipal? Principal =>  _httpContextAccessor.HttpContext?.User; 

        public bool IsAuthenticated =>  Principal?.Identity?.IsAuthenticated == true;

        public CurrentUser? GetCurrentUser()
        {
            var principal = Principal;

            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var identity = principal.Identity;


            // تشخیص نوع احراز هویت
            var provider = identity.AuthenticationType switch
            {
                "Bearer" => AuthenticationProviderEnum.Local,
                "Negotiate" => AuthenticationProviderEnum.Windows,
                "NTLM" => AuthenticationProviderEnum.Windows,
                _ => throw new UnauthorizedAccessException(
                    $"Unsupported authentication type '{identity.AuthenticationType}'.")
            };

            var userName =
                principal.FindFirstValue(ClaimTypes.Name) ??
                principal.FindFirstValue("name") ??
                identity.Name;


            if (string.IsNullOrWhiteSpace(userName))
                throw new UnauthorizedAccessException("User name was not found.");      

            string externalId;

            if (provider == AuthenticationProviderEnum.Local)
            {
                // JWT صادر شده توسط خود AiChat

                externalId =
                    principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    principal.FindFirstValue("sub") ??
                    principal.FindFirstValue("userId") ??
                    throw new UnauthorizedAccessException("JWT does not contain user id.");
            }
            else
            {
                // Windows Authentication

                externalId = identity.Name! ?? throw new UnauthorizedAccessException("Windows identity name was not found."); 
            }

            var roles = principal.Claims
           .Where(x => x.Type == ClaimTypes.Role)
           .Select(x => x.Value)
           .ToArray();


            return new CurrentUser
            {
                AuthProvider = provider,
                ExternalId = externalId,
                UserName = userName,
                DisplayName = principal.FindFirstValue(ClaimTypes.GivenName) ?? userName,
                Roles = roles
            };
        }
    }
}
