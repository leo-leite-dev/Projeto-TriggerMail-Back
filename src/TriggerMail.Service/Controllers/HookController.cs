using Microsoft.AspNetCore.Mvc;
using TriggerMail.Core.Application.Payloads;
using TriggerMail.Core.Application.Ports.Email;

[ApiController]
[Route("api/hooks/email")]
public sealed class HookController : ControllerBase
{
    private readonly IEmailTriggerPort _fire;

    public HookController(IEmailTriggerPort fire) => _fire = fire;

    [HttpPost("{alias}")]
    public async Task<IActionResult> FireByAlias(
        [FromRoute] string alias,
        [FromBody] FireEmailHookRequest body,
        CancellationToken ct)
    {
        var req = new FireEmailRequest(
            TriggerAlias: alias,
            Recipients: body.Recipients,
            Payload: body.Payload,
            Subject: body.Subject
        );

        await _fire.FireAsync(req, ct);

        return Accepted(new
        {
            alias,
            overriddenRecipients = body.Recipients?.Length ?? 0,
            hasPayload = body.Payload is not null
        });
    }
}