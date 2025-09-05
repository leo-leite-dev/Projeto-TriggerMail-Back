using System.Collections.Concurrent;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence;

public sealed class InMemoryTriggerRepository : ITriggerRepository
{
    private readonly ConcurrentDictionary<string, EmailTrigger> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<EmailTrigger?> GetByAliasAsync(string alias, CancellationToken ct)
        => Task.FromResult(_store.TryGetValue(alias, out var t) ? t : null);

    public Task<bool> AliasExistsAsync(string alias, CancellationToken ct)
        => Task.FromResult(_store.ContainsKey(alias));

    public Task<EmailTrigger> AddAsync(EmailTrigger entity, CancellationToken ct)
    {
        _store[entity.Alias] = entity;
        return Task.FromResult(entity);
    }

    public Task<EmailTrigger> UpdateAsync(EmailTrigger entity, CancellationToken ct)
    {
        _store[entity.Alias] = entity;
        return Task.FromResult(entity);
    }
}