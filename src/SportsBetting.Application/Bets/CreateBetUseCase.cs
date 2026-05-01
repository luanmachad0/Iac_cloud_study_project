using SportsBetting.Application.Bets.Contracts;
using SportsBetting.Domain.Bets;

namespace SportsBetting.Application.Bets;

public sealed class CreateBetUseCase
{
    private readonly IBetRepository _betRepository;

    public CreateBetUseCase(IBetRepository betRepository)
    {
        _betRepository = betRepository;
    }

    public async Task<CreateBetResponse> ExecuteAsync(
        CreateBetRequest request,
        CancellationToken cancellationToken)
    {
        var bet = new Bet(request.UserId, request.MatchId, request.Amount, request.Prediction);
        await _betRepository.AddAsync(bet, cancellationToken);

        return new CreateBetResponse(bet.Id, bet.Status.ToString());
    }
}
