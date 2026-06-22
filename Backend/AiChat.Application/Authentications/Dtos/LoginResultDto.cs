
namespace AiChat.Application.Authentications.Dtos
{
    public sealed class LoginResultDto
    {
        public string AccessToken { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public string UserName { get; set; } = "";
    }
}
