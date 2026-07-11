using AiChat.Domain.Entities;

namespace AiChat.Application.Abstractions
{
    public interface IUserResolver
    {
        Task<User> GetCurrentUserAsync(
            CancellationToken ct = default);
    }
}
