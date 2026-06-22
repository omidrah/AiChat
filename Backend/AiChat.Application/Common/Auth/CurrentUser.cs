namespace AiChat.Application.Common.Auth
{
    public sealed class CurrentUser
    {
        public string UserId { get; init; } = "";
        public string UserName { get; init; } = "";
        public string? DisplayName { get; init; }
        public string AuthType { get; init; } = "";
        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    }

}
