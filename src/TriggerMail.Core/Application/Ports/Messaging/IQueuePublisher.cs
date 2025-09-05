using TriggerMail.Core.Contracts.Queue;

namespace TriggerMail.Core.Application.Ports.Messaging;

public interface IQueuePublisher
{
    Task PublishAsync(EmailJob job, CancellationToken ct);
}
