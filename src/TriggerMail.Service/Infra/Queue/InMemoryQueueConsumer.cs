using System.Threading.Channels;
using TriggerMail.Core.Application.Ports.Messaging;
using TriggerMail.Core.Contracts.Queue;

namespace TriggerMail.Service.Infra.Queue;

public sealed class InMemoryQueueConsumer : IQueueConsumer
{
    private readonly Channel<EmailJob> _channel;
    public InMemoryQueueConsumer(Channel<EmailJob> channel) => _channel = channel;
    public async Task<EmailJob> ReadAsync(CancellationToken ct)
        => await _channel.Reader.ReadAsync(ct);
}