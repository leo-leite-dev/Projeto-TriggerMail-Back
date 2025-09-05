using TriggerMail.Core.Application.Payloads;

namespace TriggerMail.Core.Application.Ports.Email;

public interface IEmailTriggerPort
{
    Task FireAsync(FireEmailRequest request, CancellationToken ct = default);
}