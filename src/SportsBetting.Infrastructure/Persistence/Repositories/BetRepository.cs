using SportsBetting.Application.Bets.Contracts;
using SportsBetting.Domain.Bets;

namespace SportsBetting.Infrastructure.Persistence.Repositories;

public sealed class BetRepository : IBetRepository
{
    private readonly SportsBettingDbContext _dbContext;

    public BetRepository(SportsBettingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Bet bet, CancellationToken cancellationToken)
    {
        await _dbContext.Bets.AddAsync(bet, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Bet?> GetByIdAsync(Guid betId, CancellationToken cancellationToken)
    {
        return await _dbContext.Bets.FindAsync([betId], cancellationToken);
    }

    public async Task UpdateAsync(Bet bet, CancellationToken cancellationToken)
    {
        _dbContext.Bets.Update(bet);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
