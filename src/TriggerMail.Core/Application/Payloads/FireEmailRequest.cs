namespace TriggerMail.Core.Application.Payloads;

public sealed record FireEmailRequest(
    string TriggerAlias,
    string[]? Recipients,
    IDictionary<string, object?>? Payload,
    string? Subject = null
);