namespace AiChat.Application.Authentications.Dtos
{
    public sealed class CurrentUserDto
    {
        public AuthenticationProviderEnum AuthProvider { get; init; }

        public string ExternalId { get; init; } = "";

        public string UserName { get; init; } = "";

        public string? DisplayName { get; init; }

        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    }
}
