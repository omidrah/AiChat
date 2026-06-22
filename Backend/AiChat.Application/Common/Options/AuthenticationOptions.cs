namespace AiChat.Application.Common.Options
{
    public sealed class AuthenticationOptions
    {
        public string Mode { get; set; } = "Local";
        public JwtOptions Jwt { get; set; } = new();
    }
}
