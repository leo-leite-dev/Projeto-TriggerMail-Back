namespace TriggerMail.Core.Application.Ports.Email;

public interface ITemplateRenderer
{
    Task<(string subject, string html, string? text)> RenderAsync(string templateKey, string lang, IDictionary<string, object?> model, CancellationToken ct = default);
}