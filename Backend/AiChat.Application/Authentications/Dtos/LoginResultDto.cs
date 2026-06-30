
namespace AiChat.Application.Authentications.Dtos
{
    public class LoginResultDto
    {
        public string access_token { get; set; } = "";

        public string refresh_token { get; set; } = "";

        public string UserName { get; set; } = "";

        public DateTime ExpiresAt { get; set; }
    }
}
