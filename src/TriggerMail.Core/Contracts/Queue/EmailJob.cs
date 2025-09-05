namespace TriggerMail.Core.Contracts.Queue;

public sealed class EmailJob
{
    public Guid MessageId { get; set; }
    public string TemplateKey { get; set; } = default!;
    public string Lang { get; set; } = "pt-BR";

    public IDictionary<string, object?> Model { get; set; } = new Dictionary<string, object?>();

    public string[]? DefaultRecipients { get; set; }
}