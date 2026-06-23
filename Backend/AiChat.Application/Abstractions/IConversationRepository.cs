using AiChat.Domain.Entities;

namespace AiChat.Application.Abstractions
{
    public interface IConversationRepository
    {
        Task AddAsync(Conversation conversation,CancellationToken ct = default);

        Task<Conversation?> GetAsync(Guid id,CancellationToken ct = default);

        Task<List<Conversation>> GetPagedAsync(int page, int pageSize, CancellationToken ct=default);

        Task<List<Conversation>> GetAllAsync(CancellationToken ct = default);

        Task DeleteAsync(Conversation conversation, CancellationToken ct = default);

        Task<List<Conversation>>SearchAsync(string text);

        Task<Conversation?> GetConversationForUserAsync(Guid conversationId, Guid userId, CancellationToken ct);

        Task<List<Conversation>> GetAllConversationsForUserAsync(Guid userId, CancellationToken ct);

        Task<bool> DeleteConversationByUserAsync(Guid conversationId, Guid userId, CancellationToken ct);


        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
