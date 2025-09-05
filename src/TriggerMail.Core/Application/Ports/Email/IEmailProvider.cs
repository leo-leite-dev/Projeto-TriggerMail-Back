namespace TriggerMail.Core.Application.Ports.Email;

public interface IEmailProvider
{
    Task<(bool ok, string providerMessageId)> SendAsync(IEnumerable<string> to, string subject, string html, string? text, CancellationToken ct);
}