using TriggerMail.Core.Application.Abstractions.Result;
using TriggerMail.Core.Application.Ports.Email;
using TriggerMail.Core.Application.Ports.Messaging;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Application.Ports.Security;
using TriggerMail.Core.Contracts.Queue;

namespace TriggerMail.Core.Application.Features.Triggers;

public sealed class FireEmailTriggerImpl : IFireEmailTrigger
{
    private readonly ITriggerRepository _triggers;
    private readonly ITemplateRepository _templates;
    private readonly ISignatureVerifier _auth;
    private readonly ITemplateRenderer _renderer;
    private readonly IQueuePublisher _queue;

    public FireEmailTriggerImpl(
        ITriggerRepository triggers,
        ITemplateRepository templates,
        ISignatureVerifier auth,
        ITemplateRenderer renderer,
        IQueuePublisher queue)
    {
        _triggers = triggers;
        _templates = templates;
        _auth = auth;
        _renderer = renderer;
        _queue = queue;
    }

    public async Task<Result<Guid>> ExecuteAsync(string alias, string rawBody, string? signatureHeader, object? payload)
    {
        if (string.IsNullOrWhiteSpace(alias))
            return Result<Guid>.BadRequest("Alias é obrigatório.");

        var trigger = await _triggers.GetByAliasAsync(alias, CancellationToken.None);
        if (trigger is null || !trigger.Enabled)
            return Result<Guid>.NotFound("Trigger não encontrado ou desabilitado.");

        var authOk = await _auth.IsValidAsync(trigger, rawBody, signatureHeader, CancellationToken.None);
        if (!authOk)
            return Result<Guid>.Unauthorized("Assinatura inválida.");

        var template = await _templates.GetActiveByKeyAsync(trigger.TemplateKey, version: null, CancellationToken.None);
        if (template is null || !template.IsActive)
            return Result<Guid>.NotFound("Template não encontrado/ativo.");

        var model = payload ?? new { };

        // var preview = await _renderer.RenderAsync(template.Key, trigger.Lang, model, CancellationToken.None);

        var job = new EmailJob
        {
            MessageId = Guid.NewGuid(),
            TemplateKey = template.Key,
            Lang = trigger.Lang,
            Model = model,
            DefaultRecipients = trigger.DefaultRecipients
        };

        await _queue.PublishAsync(job, CancellationToken.None);
        return Result<Guid>.Ok(job.MessageId);
    }
}