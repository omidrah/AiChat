
namespace AiChat.Application.Authentications.Dtos
{
    public class LoginResultDto
    {
        public string AccessToken { get; set; } = "";

        public string RefreshToken { get; set; } = "";

        public string UserName { get; set; } = "";

        public DateTime ExpiresAt { get; set; }
    }
}
