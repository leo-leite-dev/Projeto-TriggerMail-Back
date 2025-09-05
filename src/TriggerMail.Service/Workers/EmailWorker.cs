using Microsoft.Extensions.Options;
using MailKit;
using MailKit.Net.Smtp;
using TriggerMail.Core.Application.Ports.Email;
using TriggerMail.Core.Application.Ports.Messaging;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Domain.Entities;
using TriggerMail.Service.Infra.Email;

namespace TriggerMail.Service.Workers;

public sealed class EmailWorker : BackgroundService
{
    private readonly ILogger<EmailWorker> _log;
    private readonly IQueueConsumer _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RetryOptions _retry;

    public EmailWorker(
        ILogger<EmailWorker> log,
        IQueueConsumer queue,
        IServiceScopeFactory scopeFactory,
        IOptions<RetryOptions> retry)
    {
        _log = log;
        _queue = queue;
        _scopeFactory = scopeFactory;
        _retry = retry.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("EmailWorker iniciado (maxAttempts={Max}, baseDelay={Base}s).", _retry.MaxAttempts, _retry.BaseDelaySeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.ReadAsync(stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var renderer  = scope.ServiceProvider.GetRequiredService<ITemplateRenderer>();
                var provider  = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
                var templates = scope.ServiceProvider.GetRequiredService<ITemplateRepository>();
                var emailLogs = scope.ServiceProvider.GetRequiredService<IEmailLogRepository>();

                var template = await templates.GetActiveByKeyAsync(job.TemplateKey, null, stoppingToken)
                               ?? throw new InvalidOperationException($"Template '{job.TemplateKey}' não encontrado/ativo.");

                var (subject, html, text) = await renderer.RenderAsync(job.TemplateKey, job.Lang, job.Model, stoppingToken);

                var logStart = EmailLog.Start(job.MessageId, job.TemplateKey, job.Lang, subject, job.DefaultRecipients ?? Array.Empty<string>());
                var logId = await emailLogs.StartAsync(logStart, stoppingToken);

                var attempt = 0;
                string? providerId = null;
                for (;;)
                {
                    attempt++;
                    try
                    {
                        var send = await provider.SendAsync(job.DefaultRecipients ?? Array.Empty<string>(), subject, html, text, stoppingToken);
                        providerId = send.providerMessageId;

                        await emailLogs.CompleteAsync(logId, providerId, "Sent", stoppingToken);
                        _log.LogInformation("Email enviado (attempt={Attempt}) msgId={MessageId} providerId={ProviderId}",
                            attempt, job.MessageId, providerId);
                        break; 
                    }
                    catch (Exception ex) when (IsTransient(ex))
                    {
                        if (attempt >= _retry.MaxAttempts)
                        {
                            await emailLogs.RegisterAttemptAsync(logId, ex.Message, "DeadLetter", null, stoppingToken);
                            _log.LogError(ex, "Envio falhou de forma transitória mas atingiu MaxAttempts. msgId={MessageId}", job.MessageId);
                            break;
                        }

                        var delay = ComputeBackoff(attempt, _retry.BaseDelaySeconds, _retry.MaxDelaySeconds);
                        await emailLogs.RegisterAttemptAsync(logId, ex.Message, "Retrying", DateTimeOffset.UtcNow.AddSeconds(delay), stoppingToken);
                        _log.LogWarning(ex, "Erro transitório (tentativa {Attempt}/{Max}). Aguardando {Delay}s. msgId={MessageId}",
                            attempt, _retry.MaxAttempts, delay, job.MessageId);
                        await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        await emailLogs.RegisterAttemptAsync(logId, ex.Message, "Failed", null, stoppingToken);
                        _log.LogError(ex, "Erro permanente no envio. msgId={MessageId}", job.MessageId);
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _log.LogInformation("EmailWorker cancelado.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Erro inesperado no worker.");
            }
        }
    }

    private static int ComputeBackoff(int attempt, int baseSeconds, int maxSeconds)
    {
        var sec = Math.Min(maxSeconds, (int)Math.Pow(2, attempt - 1) * baseSeconds);
        return Math.Max(1, sec);
    }

    private static bool IsTransient(Exception ex) =>
        ex is SmtpCommandException sce && (int)sce.StatusCode >= 400 && (int)sce.StatusCode < 500
        || ex is SmtpProtocolException
        || ex is ServiceNotConnectedException
        || ex is ServiceNotAuthenticatedException
        || ex is IOException
        || ex is TimeoutException;
}