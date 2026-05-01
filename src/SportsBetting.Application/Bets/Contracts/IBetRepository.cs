using SportsBetting.Domain.Bets;

namespace SportsBetting.Application.Bets.Contracts;

public interface IBetRepository
{
    Task AddAsync(Bet bet, CancellationToken cancellationToken);
    Task<Bet?> GetByIdAsync(Guid betId, CancellationToken cancellationToken);
    Task UpdateAsync(Bet bet, CancellationToken cancellationToken);
}
