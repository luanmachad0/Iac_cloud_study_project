using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportsBetting.Application.Bets.Contracts;
using System.Text.Json;

namespace SportsBetting.Infrastructure.Messaging;

public sealed class SqsConsumerBackgroundService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string? _sqsQueueUrl;
    private readonly ILogger<SqsConsumerBackgroundService> _logger;

    public SqsConsumerBackgroundService(
        IAmazonSQS sqsClient,
        IConfiguration configuration,
        ILogger<SqsConsumerBackgroundService> logger)
    {
        _sqsClient = sqsClient;
        _sqsQueueUrl = configuration["AWS:SqsQueueUrl"];
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_sqsQueueUrl))
        {
            _logger.LogInformation("SQS consumer disabled because queue URL is empty.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(
                    new ReceiveMessageRequest
                    {
                        QueueUrl = _sqsQueueUrl,
                        MaxNumberOfMessages = 5,
                        WaitTimeSeconds = 10
                    },
                    stoppingToken);

                foreach (var message in response.Messages)
                {
                    var payload = JsonSerializer.Deserialize<BetSettledEvent>(message.Body);
                    if (payload is not null)
                    {
                        _logger.LogInformation(
                            "Consumed BetSettledEvent v{Version} for bet {BetId} with status {Status}",
                            payload.Version,
                            payload.BetId,
                            payload.Status);
                    }

                    await _sqsClient.DeleteMessageAsync(
                        _sqsQueueUrl,
                        message.ReceiptHandle,
                        stoppingToken);
                }
            }
            catch (QueueDoesNotExistException)
            {
                _logger.LogWarning(
                    "SQS queue not found at {QueueUrl}. Consumer will retry in 15 seconds.",
                    _sqsQueueUrl);
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (AmazonSQSException exception) when (exception.ErrorCode == "AWS.SimpleQueueService.NonExistentQueue")
            {
                _logger.LogWarning(
                    "SQS queue does not exist at {QueueUrl}. Consumer will retry in 15 seconds.",
                    _sqsQueueUrl);
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unexpected error consuming SQS queue. Consumer will retry in 10 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
