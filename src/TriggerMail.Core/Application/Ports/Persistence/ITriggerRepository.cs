using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Core.Application.Ports.Persistence;

public interface ITriggerRepository
{
    Task<EmailTrigger?> GetByAliasAsync(string alias, CancellationToken ct);
    Task<bool> AliasExistsAsync(string alias, CancellationToken ct);
    Task<EmailTrigger> AddAsync(EmailTrigger entity, CancellationToken ct);
    Task<EmailTrigger> UpdateAsync(EmailTrigger entity, CancellationToken ct);
}
