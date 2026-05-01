namespace SportsBetting.Domain.Bets;

public sealed class Bet
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; }
    public string MatchId { get; private set; }
    public decimal Amount { get; private set; }
    public string Prediction { get; private set; }
    public BetStatus Status { get; private set; } = BetStatus.Pending;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? SettledAtUtc { get; private set; }

    public Bet(string userId, string matchId, decimal amount, string prediction)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(matchId))
        {
            throw new ArgumentException("MatchId is required.", nameof(matchId));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(prediction))
        {
            throw new ArgumentException("Prediction is required.", nameof(prediction));
        }

        UserId = userId.Trim();
        MatchId = matchId.Trim();
        Amount = amount;
        Prediction = prediction.Trim();
    }

    public void Settle(BetStatus status)
    {
        if (status == BetStatus.Pending)
        {
            throw new ArgumentException("Cannot settle as Pending.", nameof(status));
        }

        if (Status != BetStatus.Pending)
        {
            throw new InvalidOperationException("Bet is already settled.");
        }

        Status = status;
        SettledAtUtc = DateTime.UtcNow;
    }

    private Bet()
    {
        UserId = string.Empty;
        MatchId = string.Empty;
        Prediction = string.Empty;
    }
}
