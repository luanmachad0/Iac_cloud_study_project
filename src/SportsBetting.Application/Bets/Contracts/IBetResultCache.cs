namespace SportsBetting.Application.Bets.Contracts;

public interface IBetResultCache
{
    Task<string?> GetAsync(Guid betId, CancellationToken cancellationToken);
    Task SetAsync(Guid betId, string status, CancellationToken cancellationToken);
}
