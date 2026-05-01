namespace SportsBetting.Application.Bets;

public sealed record CreateBetRequest(
    string UserId,
    string MatchId,
    decimal Amount,
    string Prediction);
