using AiChat.Application.Common.Auth;
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
        public Guid? UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                var id =
                    user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    user?.FindFirstValue("sub") ??
                    user?.FindFirstValue("userId");

                return Guid.TryParse(id, out var userId) ? userId : null;
            }
        }

        public string? UserName
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                return
                    user?.Identity?.Name ??
                    user?.FindFirstValue(ClaimTypes.Name) ??
                    user?.FindFirstValue("name") ??
                    user?.FindFirstValue("unique_name");
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public CurrentUser? GetCurrentUser()
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var userName = principal.Identity.Name;

            if (string.IsNullOrWhiteSpace(userName))
                return null;

            var roles = principal.Claims
                .Where(x => x.Type == ClaimTypes.Role)
                .Select(x => x.Value)
                .ToArray();

            return new CurrentUser
            {
                UserId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? userName,
                UserName = userName,
                DisplayName = principal.FindFirstValue(ClaimTypes.GivenName) ?? userName,
                AuthType = principal.Identity.AuthenticationType ?? "",
                Roles = roles
            };
        }
    }
}
