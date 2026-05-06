using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
        var connectionString = ResolvePostgresConnectionString(configuration);

        services.AddDbContext<SportsBettingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IBetRepository, BetRepository>();
        services.AddScoped<IBetEventPublisher, SqsBetEventPublisher>();
        ConfigureCaching(services, configuration);

        services.AddSingleton(CreateS3Client(configuration));
        services.AddSingleton(CreateSqsClient(configuration));
        services.AddHostedService<SqsConsumerBackgroundService>();

        return services;
    }

    private static void ConfigureCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IBetResultCache, InMemoryBetResultCache>();
            return;
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 1;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddScoped<IBetResultCache, RedisBetResultCache>();
    }

    private static string ResolvePostgresConnectionString(IConfiguration configuration)
    {
        var configuredValue =
            configuration["DATABASE_URL"]
            ?? configuration.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return "Host=localhost;Port=5432;Database=sports_betting;Username=postgres;Password=postgres";
        }

        if (!configuredValue.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !configuredValue.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return configuredValue;
        }

        return ConvertPostgresUrlToConnectionString(configuredValue);
    }

    private static string ConvertPostgresUrlToConnectionString(string postgresUrl)
    {
        var uri = new Uri(postgresUrl);
        var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.TrimEntries);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty
        };

        var query = uri.Query.TrimStart('?');
        if (!string.IsNullOrWhiteSpace(query))
        {
            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length != 2)
                {
                    continue;
                }

                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                var normalized = key.Replace("_", " ", StringComparison.Ordinal);
                builder[normalized] = value;
            }
        }

        return builder.ConnectionString;
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
