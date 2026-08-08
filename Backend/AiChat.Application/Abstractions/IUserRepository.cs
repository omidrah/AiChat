using AiChat.Application.Authentications.Dtos;
using AiChat.Domain.Entities;

namespace AiChat.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);

        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<List<User>> GetAllAsync(CancellationToken ct = default);

        void Add(User user);

        Task SaveChangesAsync(CancellationToken ct = default);

        Task<User?> FindByExternalIdAsync(AuthenticationProviderEnum provider, string externalId, CancellationToken ct = default);

    }
}
