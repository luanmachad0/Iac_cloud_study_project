using SportsBetting.Application.Bets.Contracts;
using SportsBetting.Domain.Bets;

namespace SportsBetting.Application.Bets;

public sealed class SettleBetUseCase
{
    private const string EventVersion = "1.0";

    private readonly IBetRepository _betRepository;
    private readonly IBetResultCache _betResultCache;
    private readonly IBetEventPublisher _betEventPublisher;

    public SettleBetUseCase(
        IBetRepository betRepository,
        IBetResultCache betResultCache,
        IBetEventPublisher betEventPublisher)
    {
        _betRepository = betRepository;
        _betResultCache = betResultCache;
        _betEventPublisher = betEventPublisher;
    }

    public async Task<SettleBetResponse> ExecuteAsync(SettleBetRequest request, CancellationToken cancellationToken)
    {
        var bet = await _betRepository.GetByIdAsync(request.BetId, cancellationToken);
        if (bet is null)
        {
            throw new KeyNotFoundException($"Bet {request.BetId} was not found.");
        }

        bet.Settle(request.IsWon ? BetStatus.Won : BetStatus.Lost);
        await _betRepository.UpdateAsync(bet, cancellationToken);

        var status = bet.Status.ToString();
        await _betResultCache.SetAsync(bet.Id, status, cancellationToken);

        var settledAt = bet.SettledAtUtc ?? DateTime.UtcNow;
        await _betEventPublisher.PublishBetSettledAsync(
            new BetSettledEvent(EventVersion, bet.Id, bet.UserId, bet.MatchId, bet.Amount, status, settledAt),
            cancellationToken);

        return new SettleBetResponse(bet.Id, status, settledAt);
    }
}
