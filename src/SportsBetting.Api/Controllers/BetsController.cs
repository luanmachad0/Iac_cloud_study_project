using Microsoft.AspNetCore.Mvc;
using SportsBetting.Application.Bets;

namespace SportsBetting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BetsController : ControllerBase
{
    private readonly CreateBetUseCase _createBetUseCase;
    private readonly ILogger<BetsController> _logger;

    public BetsController(CreateBetUseCase createBetUseCase, ILogger<BetsController> logger)
    {
        _createBetUseCase = createBetUseCase;
        _logger = logger;
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error on settle for bet {BetId}. TraceId: {TraceId}", betId, HttpContext.TraceIdentifier);
            return Problem(title: "Erro ao liquidar aposta", detail: $"TraceId: {HttpContext.TraceIdentifier}", statusCode: StatusCodes.Status500InternalServerError);
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error on result for bet {BetId}. TraceId: {TraceId}", betId, HttpContext.TraceIdentifier);
            return Problem(title: "Erro ao consultar resultado", detail: $"TraceId: {HttpContext.TraceIdentifier}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

public sealed record CreateBetHttpRequest(string UserId, string MatchId, decimal Amount, string Prediction);
public sealed record SettleBetHttpRequest(bool IsWon);
