namespace TriggerMail.Service.Infra.Email;

public sealed class RetryOptions
{
    public int MaxAttempts { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 5;
    public int MaxDelaySeconds { get; set; } = 60;
}
