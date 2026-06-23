using AiChat.Application.Abstractions;
using AiChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ChatDbContext _dbContext;

        public RefreshTokenRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken ct = default)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken, ct);
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken ct = default)
        {
            return await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
