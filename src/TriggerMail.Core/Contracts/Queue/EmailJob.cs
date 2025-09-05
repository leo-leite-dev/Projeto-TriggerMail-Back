namespace TriggerMail.Core.Contracts.Queue;

public sealed class EmailJob
{
    public Guid MessageId { get; set; }
    public string TemplateKey { get; set; } = default!;
    public string Lang { get; set; } = "pt-BR";
    public object Model { get; set; } = default!;
    public string[]? DefaultRecipients { get; set; }
}