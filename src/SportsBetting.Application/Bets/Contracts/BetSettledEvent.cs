namespace SportsBetting.Application.Bets.Contracts;

public sealed record BetSettledEvent(
    string Version,
    Guid BetId,
    string UserId,
    string MatchId,
    decimal Amount,
    string Status,
    DateTime SettledAtUtc);
