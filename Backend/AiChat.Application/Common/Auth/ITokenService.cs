namespace AiChat.Application.Common.Auth
{
    public interface ITokenService
    {
        string CreateToken(string userId, string userName, IEnumerable<string> roles);
    }
}
