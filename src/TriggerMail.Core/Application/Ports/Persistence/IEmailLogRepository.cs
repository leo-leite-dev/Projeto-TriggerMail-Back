using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Core.Application.Ports.Persistence;

public interface IEmailLogRepository
{
    Task<Guid> StartAsync(EmailLog log, CancellationToken ct);
    Task CompleteAsync(Guid logId, string providerMessageId, string status, CancellationToken ct);
    Task RegisterAttemptAsync(Guid logId, string? error, string status, DateTimeOffset? nextAttemptAt, CancellationToken ct);
}