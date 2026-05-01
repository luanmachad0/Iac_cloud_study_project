namespace SportsBetting.Application.Bets.Contracts;

public interface IBetEventPublisher
{
    Task PublishBetSettledAsync(BetSettledEvent betSettledEvent, CancellationToken cancellationToken);
}
