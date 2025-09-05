namespace TriggerMail.Core.Application.Ports.Email;

public interface ITemplateRenderer
{
    Task<(string subject, string html, string? text)> RenderAsync(string templateKey, string lang, object model, CancellationToken ct);
}