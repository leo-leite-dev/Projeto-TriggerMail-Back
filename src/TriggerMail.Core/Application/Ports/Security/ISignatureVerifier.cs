using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Core.Application.Ports.Security;

public interface ISignatureVerifier
{
    Task<bool> IsValidAsync(EmailTrigger trigger, string rawBody, string? signatureHeader, CancellationToken ct);
}