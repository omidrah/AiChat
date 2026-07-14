using AiChat.Application.Conversations.Dtos;

namespace AiChat.Application.Abstractions
{
    public interface IActiveDirectoryAuthService
    {
        Task<ActiveDirectoryUserInfo?> ValidateAsync(
            string userName,
            string password,
            CancellationToken ct = default);
    }

}
