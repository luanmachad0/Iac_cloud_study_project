using Microsoft.AspNetCore.Mvc;
using SportsBetting.Application.Bets;

namespace SportsBetting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BetsController : ControllerBase
{
    private readonly CreateBetUseCase _createBetUseCase;

    public BetsController(CreateBetUseCase createBetUseCase)
    {
        _createBetUseCase = createBetUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBetHttpRequest request, CancellationToken cancellationToken)
    {
        var response = await _createBetUseCase.ExecuteAsync(
            new CreateBetRequest(request.UserId, request.MatchId, request.Amount, request.Prediction),
            cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = response.BetId }, response);
    }

    [HttpPost("{betId:guid}/settle")]
    public async Task<IActionResult> Settle(
        Guid betId,
        [FromBody] SettleBetHttpRequest request,
        [FromServices] SettleBetUseCase settleBetUseCase,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await settleBetUseCase.ExecuteAsync(
                new SettleBetRequest(betId, request.IsWon),
                cancellationToken);

            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{betId:guid}/result")]
    public async Task<IActionResult> GetResult(
        Guid betId,
        [FromServices] GetBetResultUseCase getBetResultUseCase,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await getBetResultUseCase.ExecuteAsync(betId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

public sealed record CreateBetHttpRequest(string UserId, string MatchId, decimal Amount, string Prediction);
public sealed record SettleBetHttpRequest(bool IsWon);
