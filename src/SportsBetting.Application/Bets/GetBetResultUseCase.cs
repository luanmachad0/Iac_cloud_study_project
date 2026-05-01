using SportsBetting.Application.Bets.Contracts;

namespace SportsBetting.Application.Bets;

public sealed class GetBetResultUseCase
{
    private readonly IBetRepository _betRepository;
    private readonly IBetResultCache _betResultCache;

    public GetBetResultUseCase(IBetRepository betRepository, IBetResultCache betResultCache)
    {
        _betRepository = betRepository;
        _betResultCache = betResultCache;
    }

    public async Task<GetBetResultResponse> ExecuteAsync(Guid betId, CancellationToken cancellationToken)
    {
        var cachedStatus = await _betResultCache.GetAsync(betId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedStatus))
        {
            return new GetBetResultResponse(betId, cachedStatus);
        }

        var bet = await _betRepository.GetByIdAsync(betId, cancellationToken);
        if (bet is null)
        {
            throw new KeyNotFoundException($"Bet {betId} was not found.");
        }

        var status = bet.Status.ToString();
        await _betResultCache.SetAsync(betId, status, cancellationToken);
        return new GetBetResultResponse(betId, status);
    }
}
