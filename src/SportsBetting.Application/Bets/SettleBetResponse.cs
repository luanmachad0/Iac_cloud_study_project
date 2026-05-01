namespace SportsBetting.Application.Bets;

public sealed record SettleBetResponse(Guid BetId, string Status, DateTime SettledAtUtc);
