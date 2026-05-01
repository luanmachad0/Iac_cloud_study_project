using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SportsBetting.Application.Bets.Contracts;
using SportsBetting.Domain.Bets;

namespace SportsBetting.E2ETests;

public class HealthCheckE2ETests : IClassFixture<E2ETestApiFactory>
{
    private readonly HttpClient _httpClient;

    public HealthCheckE2ETests(E2ETestApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk()
    {
        var response = await _httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class E2ETestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBetRepository>();
            services.RemoveAll<IBetResultCache>();
            services.RemoveAll<IBetEventPublisher>();
            services.AddSingleton<IBetRepository, E2EInMemoryBetRepository>();
            services.AddSingleton<IBetResultCache, E2EInMemoryBetResultCache>();
            services.AddSingleton<IBetEventPublisher, E2EInMemoryBetEventPublisher>();
        });
    }
}

public sealed class E2EInMemoryBetRepository : IBetRepository
{
    public Task AddAsync(Bet bet, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<Bet?> GetByIdAsync(Guid betId, CancellationToken cancellationToken) => Task.FromResult<Bet?>(null);
    public Task UpdateAsync(Bet bet, CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class E2EInMemoryBetResultCache : IBetResultCache
{
    public Task<string?> GetAsync(Guid betId, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
    public Task SetAsync(Guid betId, string status, CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class E2EInMemoryBetEventPublisher : IBetEventPublisher
{
    public Task PublishBetSettledAsync(BetSettledEvent betSettledEvent, CancellationToken cancellationToken) => Task.CompletedTask;
}
