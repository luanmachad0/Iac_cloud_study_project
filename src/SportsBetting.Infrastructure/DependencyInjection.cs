using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsBetting.Application.Bets.Contracts;
using SportsBetting.Infrastructure.Caching;
using SportsBetting.Infrastructure.Messaging;
using SportsBetting.Infrastructure.Persistence;
using SportsBetting.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;

namespace SportsBetting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5432;Database=sports_betting;Username=postgres;Password=postgres";

        services.AddDbContext<SportsBettingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IBetRepository, BetRepository>();
        services.AddScoped<IBetResultCache, RedisBetResultCache>();
        services.AddScoped<IBetEventPublisher, SqsBetEventPublisher>();

        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 1;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddSingleton(CreateS3Client(configuration));
        services.AddSingleton(CreateSqsClient(configuration));
        services.AddHostedService<SqsConsumerBackgroundService>();

        return services;
    }

    private static IAmazonS3 CreateS3Client(IConfiguration configuration)
    {
        var serviceUrl = configuration["AWS:ServiceUrl"];
        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            return new AmazonS3Client(
                new BasicAWSCredentials("test", "test"),
                new AmazonS3Config
                {
                    ServiceURL = serviceUrl,
                    ForcePathStyle = true
                });
        }

        var region = configuration["AWS:Region"] ?? RegionEndpoint.USEast1.SystemName;
        return new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
    }

    private static IAmazonSQS CreateSqsClient(IConfiguration configuration)
    {
        var serviceUrl = configuration["AWS:ServiceUrl"];
        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            return new AmazonSQSClient(
                new BasicAWSCredentials("test", "test"),
                new AmazonSQSConfig
                {
                    ServiceURL = serviceUrl
                });
        }

        var region = configuration["AWS:Region"] ?? RegionEndpoint.USEast1.SystemName;
        return new AmazonSQSClient(RegionEndpoint.GetBySystemName(region));
    }
}
