using AiChat.Domain.Entities;

namespace AiChat.Application.Abstractions
{
    public interface IConversationRepository
    {
        Task AddAsync(
            Conversation conversation,
            CancellationToken cancellationToken = default);

        Task<Conversation?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<Conversation>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
