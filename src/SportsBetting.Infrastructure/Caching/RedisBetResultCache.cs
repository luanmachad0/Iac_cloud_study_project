using SportsBetting.Application.Bets.Contracts;
using StackExchange.Redis;

namespace SportsBetting.Infrastructure.Caching;

public sealed class RedisBetResultCache : IBetResultCache
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisBetResultCache(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<string?> GetAsync(Guid betId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var db = _connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(GetCacheKey(betId));
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetAsync(Guid betId, string status, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var db = _connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(GetCacheKey(betId), status, TimeSpan.FromHours(1));
    }

    private static string GetCacheKey(Guid betId) => $"bet:result:{betId}";
}
