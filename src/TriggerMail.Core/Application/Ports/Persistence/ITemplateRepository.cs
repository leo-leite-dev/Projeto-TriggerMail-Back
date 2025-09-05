using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Core.Application.Ports.Persistence;

public interface ITemplateRepository
{
    Task<EmailTemplate?> GetActiveByKeyAsync(string key, int? version, CancellationToken ct);
    Task<EmailTemplate> UpsertAsync(EmailTemplate template, CancellationToken ct);
}
