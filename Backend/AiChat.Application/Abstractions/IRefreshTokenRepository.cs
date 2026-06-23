using AiChat.Domain.Entities;

namespace AiChat.Application.Abstractions
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);

        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash,CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}