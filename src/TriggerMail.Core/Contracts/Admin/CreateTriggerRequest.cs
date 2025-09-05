namespace TriggerMail.Core.Contracts.Admin;

public sealed class CreateTriggerRequest
{
    public string Alias { get; set; } = default!;
    public string TemplateKey { get; set; } = default!;
    public string Lang { get; set; } = "pt-BR";
    public string AuthType { get; set; } = "hmac";
    public string? AuthSecret { get; set; }
    public string[]? DefaultRecipients { get; set; }
    public object? MappingConfig { get; set; }
    public bool Enabled { get; set; } = true;
}