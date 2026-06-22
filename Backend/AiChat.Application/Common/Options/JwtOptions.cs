namespace AiChat.Application.Common.Options
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string Secret { get; set; } = "";
        public int ExpiresMinutes { get; set; } = 480;
    }

}
