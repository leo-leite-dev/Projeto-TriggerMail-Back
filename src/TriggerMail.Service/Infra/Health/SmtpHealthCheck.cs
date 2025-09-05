using MailKit.Net.Smtp;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TriggerMail.Service.Infra.Health;

public sealed class SmtpHealthCheck : IHealthCheck
{
    private readonly IConfiguration _cfg;
    public SmtpHealthCheck(IConfiguration cfg) => _cfg = cfg;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var sec = _cfg.GetSection("Smtp");
            var host = sec["Host"] ?? "localhost";
            var port = int.TryParse(sec["Port"], out var p) ? p : 1025;
            var useSsl = bool.TryParse(sec["UseSsl"], out var ssl) && ssl;
            var user = sec["Username"];
            var pass = sec["Password"];

            using var cli = new SmtpClient();
            var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

            await cli.ConnectAsync(host, port, useSsl, linked.Token);
            if (!string.IsNullOrWhiteSpace(user))
                await cli.AuthenticateAsync(user, pass, linked.Token);
            await cli.DisconnectAsync(true, linked.Token);

            return HealthCheckResult.Healthy("SMTP OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("SMTP indisponível: " + ex.Message);
        }
    }
}