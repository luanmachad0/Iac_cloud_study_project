namespace SportsBetting.Infrastructure.Configuration;

public sealed class AwsOptions
{
    public string? Region { get; init; }
    public string? ServiceUrl { get; init; }
    public string? SqsQueueUrl { get; init; }
}
