namespace TriggerMail.Core.Domain.Entities;

public sealed class EmailLog
{
    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public string TemplateKey { get; private set; } = default!;
    public string Lang { get; private set; } = "pt-BR";
    public string Subject { get; private set; } = default!;
    public string[] Recipients { get; private set; } = Array.Empty<string>();
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string Status { get; private set; } = "Queued";
    public string? ProviderMessageId { get; private set; }

    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private EmailLog() { }

    public static EmailLog Start(Guid messageId, string templateKey, string lang, string subject, string[] recipients)
        => new EmailLog
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            TemplateKey = templateKey,
            Lang = lang,
            Subject = subject,
            Recipients = recipients,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Status = "Queued",
            AttemptCount = 0
        };

    public void RegisterAttempt(string? error, string status, DateTimeOffset? nextAttemptAt)
    {
        AttemptCount++;
        LastError = error;
        Status = status;
        NextAttemptAt = nextAttemptAt;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Complete(string providerMessageId, string status)
    {
        ProviderMessageId = providerMessageId;
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}