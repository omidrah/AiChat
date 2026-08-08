using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;
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
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public void Add(User user) => _dbContext.Users.Add(user);

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        {   
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName && x.IsActive,ct);
        }

        public async Task<User?> FindByExternalIdAsync(
      AuthenticationProviderEnum authenticationProvider,
      string externalId,
      CancellationToken ct = default)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.AuthProvider == authenticationProvider.ToString() &&
                x.ExternalId == externalId &&
                x.IsActive, ct);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbContext.Users
                .OrderBy(x => x.UserName)
                .ToListAsync(ct);
        }
    }
}
