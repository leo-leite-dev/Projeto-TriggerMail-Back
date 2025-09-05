using TriggerMail.Core.Domain.Base;

namespace TriggerMail.Core.Domain.Entities;

public sealed class EmailLog : Entity
{
    public Guid MessageId { get; private set; }
    public string TemplateKey { get; private set; } = default!;
    public string Lang { get; private set; } = "pt-BR";
    public string Subject { get; private set; } = default!;
    public string[] Recipients { get; private set; } = Array.Empty<string>();
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public string Status { get; private set; } = "Queued"; 
    public string? ProviderMessageId { get; private set; }

    private EmailLog() { }

    public static EmailLog Start(Guid messageId, string templateKey, string lang, string subject, IEnumerable<string> recipients)
        => new()
        {
            MessageId = messageId,
            TemplateKey = templateKey,
            Lang = lang,
            Subject = subject,
            Recipients = recipients.ToArray(),
            Status = "Queued"
        };

    public void Complete(string providerMessageId, string status)
    {
        ProviderMessageId = providerMessageId;
        Status = status;
    }
}
