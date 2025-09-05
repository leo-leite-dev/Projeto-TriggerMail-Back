using TriggerMail.Core.Application.Payloads;
using UseCaseIFireEmailTrigger = TriggerMail.Core.Application.Features.Triggers.IFireEmailTrigger;

namespace TriggerMail.Core.Application.Ports.Email;

public sealed class FireEmailTriggerPortAdapter : IEmailTriggerPort
{
    private readonly UseCaseIFireEmailTrigger _useCase;

    public FireEmailTriggerPortAdapter(UseCaseIFireEmailTrigger useCase)
    {
        _useCase = useCase;
    }

    public async Task FireAsync(FireEmailRequest request, CancellationToken ct = default)
    {
        var payload = new Dictionary<string, object?>();

        if (request.Payload is not null)
            foreach (var kv in request.Payload) payload[kv.Key] = kv.Value;

        if (request.Recipients is not null) payload["recipients"] = request.Recipients;
        if (!string.IsNullOrWhiteSpace(request.Subject)) payload["subject"] = request.Subject;

        await _useCase.ExecuteAsync(
            alias: request.TriggerAlias,
            rawBody: string.Empty,
            signatureHeader: null,
            payload: payload,
            ct: ct
        );
    }
}