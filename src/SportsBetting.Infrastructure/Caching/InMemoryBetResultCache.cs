using System.Collections.Concurrent;
using SportsBetting.Application.Bets.Contracts;

namespace SportsBetting.Infrastructure.Caching;

public sealed class InMemoryBetResultCache : IBetResultCache
{
    private readonly ConcurrentDictionary<Guid, string> _cache = new();

    public Task<string?> GetAsync(Guid betId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache.TryGetValue(betId, out var status);
        return Task.FromResult(status);
    }

    public Task SetAsync(Guid betId, string status, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache[betId] = status;
        return Task.CompletedTask;
    }
}
