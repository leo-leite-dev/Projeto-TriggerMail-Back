using System.Collections.Concurrent;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence.Repositories;

public sealed class InMemoryTemplateRepository : ITemplateRepository
{
    private readonly ConcurrentDictionary<(string key, int ver), EmailTemplate> _store = new();

    public Task<EmailTemplate?> GetActiveByKeyAsync(string key, int? version, CancellationToken ct)
    {
        if (version is null)
        {
            _store.TryGetValue((key, 1), out var t);
            return Task.FromResult(t is { IsActive: true } ? t : null);
        }

        _store.TryGetValue((key, version.Value), out var found);
        return Task.FromResult(found is { IsActive: true } ? found : null);
    }

    public Task<EmailTemplate> UpsertAsync(EmailTemplate template, CancellationToken ct)
    {
        _store[(template.Key, template.Version)] = template;
        return Task.FromResult(template);
    }
}