using System.Text.Json;
using System.Text.RegularExpressions;
using TriggerMail.Core.Application.Ports.Email;
using TriggerMail.Core.Application.Ports.Persistence;

namespace TriggerMail.Service.Infra.Templates;

public sealed class SimpleTemplateRenderer : ITemplateRenderer
{
    private readonly ITemplateRepository _templates;

    public SimpleTemplateRenderer(ITemplateRepository templates) => _templates = templates;

    public async Task<(string subject, string html, string? text)> RenderAsync(string templateKey, string lang, object model, CancellationToken ct)
    {
        var template = await _templates.GetActiveByKeyAsync(templateKey, null, ct)
            ?? throw new InvalidOperationException("Template não encontrado/ativo.");

        string Render(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var dict = ToFlatDict(model);
            return Regex.Replace(input, "{{\\s*([a-zA-Z0-9_]+)\\s*}}", m =>
            {
                var key = m.Groups[1].Value;
                return dict.TryGetValue(key, out var val) ? val ?? string.Empty : string.Empty;
            });
        }

        return (Render(template.Subject), Render(template.Html), Render(template.Text ?? ""));
    }

    private static Dictionary<string, string?> ToFlatDict(object model)
    {
        var json = JsonSerializer.Serialize(model);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in root.EnumerateObject())
                dict[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => prop.Value.ToString()
                };
        }
        return dict;
    }
}