namespace AiChat.Application.Dtos
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public string UserName { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }


}
