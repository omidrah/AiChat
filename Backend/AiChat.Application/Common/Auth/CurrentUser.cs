using AiChat.Application.Common.Enums;

namespace AiChat.Application.Common.Auth
{
    public sealed class CurrentUser
    {
        public AuthenticationProviderEnum AuthProvider { get; init; }

        public string ExternalId { get; init; } = "";

        public string UserName { get; init; } = "";

        public string? DisplayName { get; init; }

        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    }

}
