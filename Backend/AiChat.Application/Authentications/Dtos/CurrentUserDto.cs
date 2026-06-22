namespace AiChat.Application.Authentications.Dtos
{
    public sealed class CurrentUserDto
    {
        public string UserId { get; init; } = "";
        public string UserName { get; init; } = "";
        public string? DisplayName { get; init; }
        public string AuthType { get; init; } = "";
        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    }
}
