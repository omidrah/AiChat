using AiChat.Application.Authentications.Dtos;

namespace AiChat.Application.Common
{
    public sealed class CurrentUser
    {
        public AuthenticationProviderEnum AuthProvider { get; init; }
        public Guid? UserId { get; set; }

        public string? ExternalId { get; init; } = "";

        public string UserName { get; init; } = "";

        public string? DisplayName { get; init; }

        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    }

}
