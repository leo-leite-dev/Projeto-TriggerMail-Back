using TriggerMail.Core.Domain.Base;

namespace TriggerMail.Core.Domain.Entities;

public sealed class EmailTrigger : Entity
{
    public string Alias { get; private set; } = default!;
    public string TemplateKey { get; private set; } = default!;
    public string Lang { get; private set; } = "pt-BR";
    public string AuthType { get; private set; } = "hmac";
    public string? AuthSecret { get; private set; }
    public string[]? DefaultRecipients { get; private set; }
    public string? MappingConfigJson { get; private set; }
    public bool Enabled { get; private set; } = true;

    private EmailTrigger() { }

    public static EmailTrigger Create(
        string alias,
        string templateKey,
        string lang,
        string authType,
        string? authSecret,
        string[]? defaultRecipients,
        string? mappingConfigJson,
        bool enabled = true)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException("Alias inválido", nameof(alias));

        if (string.IsNullOrWhiteSpace(templateKey))
            throw new ArgumentException("TemplateKey inválido", nameof(templateKey));

        if (string.IsNullOrWhiteSpace(lang))
            lang = "pt-BR";

        return new EmailTrigger
        {
            Alias = alias.Trim(),
            TemplateKey = templateKey.Trim(),
            Lang = lang.Trim(),
            AuthType = authType?.Trim().ToLowerInvariant() ?? "hmac",
            AuthSecret = authSecret,
            DefaultRecipients = defaultRecipients,
            MappingConfigJson = mappingConfigJson,
            Enabled = enabled
        };
    }
}