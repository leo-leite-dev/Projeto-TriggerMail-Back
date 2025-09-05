using System.Text.Json;
using TriggerMail.Core.Application.Abstractions.Result;
using TriggerMail.Core.Application.Ports.Email;
using TriggerMail.Core.Application.Ports.Persistence;

namespace TriggerMail.Core.Application.Features.Triggers;

public sealed class FireEmailTriggerImpl : IFireEmailTrigger
{
    private readonly ITriggerRepository _triggers;
    private readonly IEmailProvider _email;
    private readonly ITemplateRenderer _renderer;

    public FireEmailTriggerImpl(
        ITriggerRepository triggers,
        IEmailProvider email,
        ITemplateRenderer renderer)
    {
        _triggers = triggers;
        _email = email;
        _renderer = renderer;
    }

    public async Task<Result<Guid>> ExecuteAsync(
        string alias,
        string rawBody,
        string? signatureHeader,
        object? payload,
        CancellationToken ct = default)
    {
        // 1) Carrega trigger
        var trig = await _triggers.GetByAliasAsync(alias, ct);
        if (trig is null || !trig.Enabled)
            return Result<Guid>.Failure($"Trigger '{alias}' inexistente ou desabilitado.");

        // 2) Recipients: override via payload -> fallback defaultRecipients
        var recipients = ExtractRecipientsFromPayload(payload);
        if (recipients.Length == 0)
        {
            recipients = (trig.DefaultRecipients ?? Array.Empty<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct()
                .ToArray();
        }
        if (recipients.Length == 0)
            return Result<Guid>.BadRequest("Nenhum destinatário informado e o trigger não possui defaultRecipients.");

        // 3) Model para template (payload inteiro, exceto recipients)
        var model = ExtractModel(payload);

        // 4) Render template -> (subject, html, text)
        var (tplSubject, tplHtml, tplText) = await _renderer.RenderAsync(
            templateKey: trig.TemplateKey,
            lang: trig.Lang,
            model: model,
            ct: ct
        );

        // 5) Subject final (template > fallback simples)
        var subject = string.IsNullOrWhiteSpace(tplSubject)
            ? $"[{trig.Alias}] Notificação"
            : tplSubject!;

        // 6) Envia
        var send = await _email.SendAsync(recipients, subject, tplHtml, tplText, ct);
        if (!send.ok)
            return Result<Guid>.Failure($"Falha ao enviar e-mail: {send.providerMessageId}");

        // 7) Retorna id rastreável
        if (Guid.TryParse(send.providerMessageId, out var id))
            return Result<Guid>.Ok(id);

        return Result<Guid>.Ok(Guid.NewGuid());
    }

    // ===== Helpers =====
    private static string[] ExtractRecipientsFromPayload(object? payload)
    {
        try
        {
            if (payload is null) return Array.Empty<string>();

            if (payload is IDictionary<string, object?> dict &&
                dict.TryGetValue("recipients", out var rec))
                return NormalizeRecipients(rec);

            if (payload is string json)
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("recipients", out var node))
                    return NormalizeRecipients(node);
            }
        }
        catch { /* ignora parsing errors */ }

        return Array.Empty<string>();
    }

    private static Dictionary<string, object?> ExtractModel(object? payload)
    {
        var model = new Dictionary<string, object?>();

        if (payload is IDictionary<string, object?> dict)
        {
            foreach (var kv in dict)
                if (!string.Equals(kv.Key, "recipients", StringComparison.OrdinalIgnoreCase))
                    model[kv.Key] = kv.Value;
            return model;
        }

        if (payload is string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (string.Equals(prop.Name, "recipients", StringComparison.OrdinalIgnoreCase))
                        continue;
                    model[prop.Name] = JsonElementToObject(prop.Value);
                }
            }
            catch { /* ignore */ }
        }

        return model;
    }

    private static string[] NormalizeRecipients(object? value)
    {
        if (value is null) return Array.Empty<string>();

        if (value is IEnumerable<string> arr)
            return arr.Where(NotEmpty).Select(Trim).Distinct().ToArray();

        if (value is string s)
        {
            var parts = s.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Where(NotEmpty).Select(Trim).Distinct().ToArray();
        }

        if (value is JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.Array)
            {
                var list = new List<string>();
                foreach (var item in el.EnumerateArray())
                    if (item.ValueKind == JsonValueKind.String)
                        list.Add(item.GetString()!);
                return list.Where(NotEmpty).Select(Trim).Distinct().ToArray();
            }
            if (el.ValueKind == JsonValueKind.String)
                return NormalizeRecipients(el.GetString());
        }

        return Array.Empty<string>();

        static bool NotEmpty(string s) => !string.IsNullOrWhiteSpace(s);
        static string Trim(string s) => s.Trim();
    }

    private static object? JsonElementToObject(JsonElement el) =>
        el.ValueKind switch
        {
            JsonValueKind.Null => (object?)null,
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var i64) ? i64 :
                                    el.TryGetDouble(out var dbl) ? dbl : el.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object?>>(el.GetRawText()),
            JsonValueKind.Array => JsonSerializer.Deserialize<List<object?>>(el.GetRawText()),
            _ => el.GetRawText()
        };
}