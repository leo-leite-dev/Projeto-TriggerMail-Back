using Microsoft.AspNetCore.Mvc;
using System.Text;
using TriggerMail.Core.Application.Features.Triggers;
using TriggerMail.Core.Contracts.Hooks;

namespace TriggerMail.Service.Controllers;

[ApiController]
[Route("hooks")]
public sealed class HookController : ControllerBase
{
    private readonly IFireEmailTrigger _useCase;

    public HookController(IFireEmailTrigger useCase) => _useCase = useCase;

    [HttpPost("{alias}")]
    public async Task<IActionResult> FireAsync([FromRoute] string alias, [FromBody] FireTriggerRequest req)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Signature"].FirstOrDefault();

        var result = await _useCase.ExecuteAsync(alias, rawBody, signature, req.Payload);
        if (result.IsSuccess)
            return Ok(new { queued = true, messageId = result.Value });

        var err = result.Error!;
        return Problem(err.Message, statusCode: err.StatusCode ?? 400, title: err.Code);
    }
}