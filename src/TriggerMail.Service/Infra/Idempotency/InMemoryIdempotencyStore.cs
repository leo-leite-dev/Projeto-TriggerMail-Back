using System.Collections.Concurrent;

namespace TriggerMail.Service.Infra.Idempotency;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<(string alias, string key), (Guid messageId, DateTimeOffset expires)> _dict
        = new();

    public bool TryGet(string alias, string key, out Guid messageId)
    {
        messageId = default;
        if (_dict.TryGetValue((alias, key), out var entry))
        {
            if (entry.expires > DateTimeOffset.UtcNow)
            {
                messageId = entry.messageId;
                return true;
            }
            _dict.TryRemove((alias, key), out _);
        }
        return false;
    }

    public void Put(string alias, string key, Guid messageId, TimeSpan ttl)
    {
        _dict[(alias, key)] = (messageId, DateTimeOffset.UtcNow.Add(ttl));
    }
}