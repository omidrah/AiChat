using AiChat.Domain.Entities;

namespace AiChat.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);

        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task AddAsync(User user, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
