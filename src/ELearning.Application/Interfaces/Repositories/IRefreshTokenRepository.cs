using ELearning.Domain.Entities;

namespace ELearning.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task RevokeActiveTokensForUserAsync(
        int userId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default);
}

