using Prometheus;

namespace TriggerMail.Service.Infra.Observability;

public static class MetricsRegistry
{
    public static readonly Counter TriggersReceivedTotal =
        Metrics.CreateCounter("triggermail_triggers_received_total", "Total de chamadas ao /hooks/{alias}.");

    public static readonly Counter IdempotentHitsTotal =
        Metrics.CreateCounter("triggermail_idempotent_hits_total", "Requests idempotentes (mesmo Idempotency-Key).");

    public static readonly Counter JobsEnqueuedTotal =
        Metrics.CreateCounter("triggermail_jobs_enqueued_total", "Jobs enfileirados com sucesso.");

    public static readonly Counter EmailsSentTotal =
        Metrics.CreateCounter("triggermail_emails_sent_total", "E-mails enviados com sucesso.");

    public static readonly Counter EmailsFailedTotal =
        Metrics.CreateCounter("triggermail_emails_failed_total", "Falhas permanentes de envio.");

    public static readonly Counter EmailsDeadLetterTotal =
        Metrics.CreateCounter("triggermail_emails_deadletter_total", "Falhas transitórias que atingiram o máximo de tentativas.");

    public static readonly Counter EmailRetriesTotal =
        Metrics.CreateCounter("triggermail_email_retries_total", "Tentativas de reenvio (backoff).");

    public static readonly Histogram EmailSendDurationSeconds =
        Metrics.CreateHistogram("triggermail_email_send_duration_seconds", "Duração do envio (render + SMTP).",
            new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.02, 1.8, 10) }); // ~20ms..>s
}