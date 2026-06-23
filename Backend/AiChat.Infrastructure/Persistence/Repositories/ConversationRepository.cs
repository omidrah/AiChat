using AiChat.Application.Abstractions;
using AiChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ChatDbContext _dbContext;

        public ConversationRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Conversation conversation, CancellationToken ct = default)
        {
            await _dbContext.Conversations.AddAsync(conversation, ct);
        }

        public async Task<Conversation?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.Conversations.Include(x => x.Messages)               
                .SingleOrDefaultAsync(c => c.Id == id, ct);
        }
        public async Task<List<Conversation>> GetPagedAsync(int page,int pageSize, CancellationToken ct)
        {
            return await _dbContext.Conversations
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }
        public async Task<List<Conversation>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbContext.Conversations.Include(x => x.Messages).OrderByDescending(x => x.UpdatedAt).ToListAsync(ct);
        }

        public Task DeleteAsync(Conversation conversation, CancellationToken ct = default)
        {
            _dbContext.Conversations.Remove(conversation);

            return Task.CompletedTask;
        }

        public async Task<List<Conversation>>SearchAsync(string text)
        {
            return await _dbContext.Conversations
                .Where(x => x.Title.Contains(text))
                .ToListAsync();
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Conversation?> GetConversationForUserAsync(Guid conversationId, Guid userId, CancellationToken ct)
        {
            return await _dbContext.Conversations.Include(x => x.Messages)
                   .FirstOrDefaultAsync(x =>   x.Id == conversationId &&   x.UserId == userId, ct);
        }

        public async Task<List<Conversation>> GetAllConversationsForUserAsync(Guid userId, CancellationToken ct)
        {
            return await _dbContext.Conversations
                           .Where(x => x.UserId == userId)
                           .OrderByDescending(x => x.CreatedAt)
                           .ToListAsync(ct);
        }

        public async Task<bool> DeleteConversationByUserAsync(Guid conversationId, Guid userId, CancellationToken ct)
        {
            var conversation = await _dbContext.Conversations
                    .FirstOrDefaultAsync(x =>
                        x.Id == conversationId &&
                        x.UserId == userId,
                        ct);

            if (conversation is null)
                return false;

            _dbContext.Conversations.Remove(conversation);
            return true;
        }
    }
}
