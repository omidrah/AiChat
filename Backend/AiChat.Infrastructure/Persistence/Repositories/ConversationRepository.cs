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

        public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            await _dbContext.Conversations
                .AddAsync(conversation, cancellationToken);
        }

        public async Task<Conversation?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Conversations.Include(x => x.Messages)                
                .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<List<Conversation>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Conversations
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
