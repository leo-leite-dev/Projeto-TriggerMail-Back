using TriggerMail.Core.Domain.Base;

namespace TriggerMail.Core.Domain.Entities;

public sealed class EmailTemplate : Entity
{
    public string Key { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public string Html { get; private set; } = default!;
    public string? Text { get; private set; }
    public int Version { get; private set; } = 1;
    public bool IsActive { get; private set; } = true;

    private EmailTemplate() { }

    public static EmailTemplate Create(string key, string subject, string html, string? text, int? version, bool? isActive)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key inválida", nameof(key));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject inválido", nameof(subject));

        if (string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("Html inválido", nameof(html));

        return new EmailTemplate
        {
            Key = key.Trim(),
            Subject = subject,
            Html = html,
            Text = text,
            Version = version.GetValueOrDefault(1),
            IsActive = isActive ?? true
        };
    }
}