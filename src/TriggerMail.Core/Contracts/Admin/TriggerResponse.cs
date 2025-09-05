namespace TriggerMail.Core.Contracts.Admin;

public sealed class TriggerResponse
{
    public Guid Id { get; set; }
    public string Alias { get; set; } = default!;
    public string TemplateKey { get; set; } = default!;
    public string Lang { get; set; } = default!;
    public string AuthType { get; set; } = default!;
    public string? AuthSecretMask { get; set; }
    public string[]? DefaultRecipients { get; set; }
    public object? MappingConfig { get; set; }
    public bool Enabled { get; set; }
}