namespace SportsBetting.Application.Bets;

public sealed record SettleBetRequest(Guid BetId, bool IsWon);
