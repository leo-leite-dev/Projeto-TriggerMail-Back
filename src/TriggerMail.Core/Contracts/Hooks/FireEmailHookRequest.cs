namespace TriggerMail.Core.Contracts.Hooks;
public sealed record FireEmailHookRequest(
    string[]? Recipients,
    Dictionary<string, object?>? Payload,
    string? Subject
);