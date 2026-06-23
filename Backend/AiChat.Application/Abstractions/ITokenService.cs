using AiChat.Domain.Entities;

namespace AiChat.Application.Abstractions
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(
           User user,
           IEnumerable<string> roles);

        (string Token, DateTime ExpiresAt) GenerateRefreshToken();
        string HashRefreshToken(string token);
    }

}
