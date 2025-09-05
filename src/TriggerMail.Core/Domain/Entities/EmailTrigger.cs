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

    public void Update(
        string? templateKey = null,
        string? lang = null,
        string? authType = null,
        string? authSecret = null,
        string[]? defaultRecipients = null,
        string? mappingConfigJson = null,
        bool? enabled = null)
    {
        if (!string.IsNullOrWhiteSpace(templateKey)) TemplateKey = templateKey.Trim();
        if (!string.IsNullOrWhiteSpace(lang)) Lang = lang.Trim();
        if (!string.IsNullOrWhiteSpace(authType)) AuthType = authType.Trim().ToLowerInvariant();
        if (authSecret is not null) AuthSecret = authSecret;
        if (defaultRecipients is not null) DefaultRecipients = defaultRecipients;
        if (mappingConfigJson is not null) MappingConfigJson = mappingConfigJson;
        if (enabled.HasValue) Enabled = enabled.Value;
    }
}