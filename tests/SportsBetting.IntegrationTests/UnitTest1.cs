using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SportsBetting.Application.Bets.Contracts;
using SportsBetting.Domain.Bets;

namespace SportsBetting.IntegrationTests;

public class BetsApiTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _httpClient;

    public BetsApiTests(TestApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task PostBets_ShouldReturnCreated_WhenPayloadIsValid()
    {
        var payload = new
        {
            userId = "user-1",
            matchId = "match-1",
            amount = 25.50m,
            prediction = "TeamA"
        };

        var response = await _httpClient.PostAsJsonAsync("/api/bets", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task SettleAndGetResult_ShouldReturnWon()
    {
        var createPayload = new
        {
            userId = "user-1",
            matchId = "match-1",
            amount = 40.00m,
            prediction = "TeamA"
        };

        var createResponse = await _httpClient.PostAsJsonAsync("/api/bets", createPayload);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateBetIntegrationResponse>();
        Assert.NotNull(created);

        var settleResponse = await _httpClient.PostAsJsonAsync($"/api/bets/{created.BetId}/settle", new { isWon = true });
        Assert.Equal(HttpStatusCode.OK, settleResponse.StatusCode);

        var resultResponse = await _httpClient.GetAsync($"/api/bets/{created.BetId}/result");
        var result = await resultResponse.Content.ReadFromJsonAsync<GetBetResultIntegrationResponse>();

        Assert.Equal(HttpStatusCode.OK, resultResponse.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Won", result.Status);
    }
}

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBetRepository>();
            services.RemoveAll<IBetResultCache>();
            services.RemoveAll<IBetEventPublisher>();
            services.AddSingleton<IBetRepository, InMemoryBetRepository>();
            services.AddSingleton<IBetResultCache, InMemoryBetResultCache>();
            services.AddSingleton<IBetEventPublisher, InMemoryBetEventPublisher>();
        });
    }
}

public sealed class InMemoryBetRepository : IBetRepository
{
    private readonly List<Bet> _bets = [];

    public Task AddAsync(Bet bet, CancellationToken cancellationToken)
    {
        _bets.Add(bet);
        return Task.CompletedTask;
    }

    public Task<Bet?> GetByIdAsync(Guid betId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_bets.FirstOrDefault(x => x.Id == betId));
    }

    public Task UpdateAsync(Bet bet, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public sealed class InMemoryBetResultCache : IBetResultCache
{
    private readonly Dictionary<Guid, string> _cache = [];

    public Task<string?> GetAsync(Guid betId, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(betId, out var value);
        return Task.FromResult(value);
    }

    public Task SetAsync(Guid betId, string status, CancellationToken cancellationToken)
    {
        _cache[betId] = status;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryBetEventPublisher : IBetEventPublisher
{
    public List<BetSettledEvent> Events { get; } = [];

    public Task PublishBetSettledAsync(BetSettledEvent betSettledEvent, CancellationToken cancellationToken)
    {
        Events.Add(betSettledEvent);
        return Task.CompletedTask;
    }
}

public sealed record CreateBetIntegrationResponse(Guid BetId, string Status);
public sealed record GetBetResultIntegrationResponse(Guid BetId, string Status);
