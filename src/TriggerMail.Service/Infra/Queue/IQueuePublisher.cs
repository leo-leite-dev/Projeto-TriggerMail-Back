using System.Threading.Channels;
using TriggerMail.Core.Application.Ports.Messaging;
using TriggerMail.Core.Contracts.Queue;

namespace TriggerMail.Service.Infra.Queue;

public sealed class InMemoryQueue : IQueuePublisher
{
    private readonly Channel<EmailJob> _channel;

    public InMemoryQueue(Channel<EmailJob> channel) => _channel = channel;

    public Task PublishAsync(EmailJob job, CancellationToken ct)
        => _channel.Writer.WriteAsync(job, ct).AsTask();
}