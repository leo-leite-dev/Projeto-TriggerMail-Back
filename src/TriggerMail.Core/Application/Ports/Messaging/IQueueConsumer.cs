using TriggerMail.Core.Contracts.Queue;

namespace TriggerMail.Core.Application.Ports.Messaging;

public interface IQueueConsumer
{
    Task<EmailJob> ReadAsync(CancellationToken ct);
}