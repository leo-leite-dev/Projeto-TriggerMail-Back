namespace TriggerMail.Core.Contracts.Templates;

public sealed class UpsertTemplateRequest
{
    public string Key { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Html { get; set; } = default!;
    public string? Text { get; set; }
    public int? Version { get; set; }         
    public bool? IsActive { get; set; }       
}