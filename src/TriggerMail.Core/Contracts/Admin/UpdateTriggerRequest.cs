namespace TriggerMail.Core.Contracts.Admin;

public sealed class UpdateTriggerRequest
{
    public string? TemplateKey { get; set; }
    public string? Lang { get; set; }
    public string? AuthType { get; set; }
    public string? AuthSecret { get; set; }
    public string[]? DefaultRecipients { get; set; }
    public object? MappingConfig { get; set; }
    public bool? Enabled { get; set; }
}