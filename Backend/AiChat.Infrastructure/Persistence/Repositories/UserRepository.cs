using AiChat.Application.Abstractions;
using AiChat.Application.Common.Enums;
using AiChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatDbContext _dbContext;

        public UserRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid id,CancellationToken ct = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _dbContext.Users.AddAsync(user, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        {   
            return await _dbContext.Users.SingleOrDefaultAsync(x => x.UserName == userName && x.IsActive,ct);
        }

        public async Task<User?> FindByExternalIdAsync(AuthenticationProviderEnum authenticationProvider,  string externalId, CancellationToken ct = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(x =>
                    x.AuthProvider == authenticationProvider.ToString() &&
                    x.ExternalId == externalId &&
                    x.IsActive,
                    ct);
        }
    }
}
