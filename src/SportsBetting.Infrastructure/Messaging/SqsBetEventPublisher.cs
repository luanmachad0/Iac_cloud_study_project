using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SportsBetting.Application.Bets.Contracts;

namespace SportsBetting.Infrastructure.Messaging;

public sealed class SqsBetEventPublisher : IBetEventPublisher
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string? _queueUrl;
    private readonly ILogger<SqsBetEventPublisher> _logger;

    public SqsBetEventPublisher(
        IAmazonSQS sqsClient,
        IConfiguration configuration,
        ILogger<SqsBetEventPublisher> logger)
    {
        _sqsClient = sqsClient;
        _queueUrl = configuration["AWS:SqsQueueUrl"];
        _logger = logger;
    }

    public async Task PublishBetSettledAsync(BetSettledEvent betSettledEvent, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_queueUrl))
        {
            return;
        }

        var body = JsonSerializer.Serialize(betSettledEvent);
        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = body
        };

        try
        {
            await _sqsClient.SendMessageAsync(request, cancellationToken);
        }
        catch (QueueDoesNotExistException)
        {
            _logger.LogWarning(
                "Bet settled event not published because queue does not exist at {QueueUrl}.",
                _queueUrl);
        }
        catch (AmazonSQSException exception) when (exception.ErrorCode == "AWS.SimpleQueueService.NonExistentQueue")
        {
            _logger.LogWarning(
                "Bet settled event not published because queue does not exist at {QueueUrl}.",
                _queueUrl);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Bet settled event was not published to SQS. QueueUrl: {QueueUrl}",
                _queueUrl);
        }
    }
}
