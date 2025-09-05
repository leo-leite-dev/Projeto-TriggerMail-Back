using MailKit.Net.Smtp;
using MimeKit;
using TriggerMail.Core.Application.Ports.Email;

namespace TriggerMail.Service.Infra.Email;

public sealed class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpOptions _opt;
    public SmtpEmailProvider(IConfiguration cfg)
    {
        _opt = cfg.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
    }

    public async Task<(bool ok, string providerMessageId)> SendAsync(
        IEnumerable<string> to, string subject, string html, string? text, CancellationToken ct)
    {
        var message = new MimeMessage();

        var fromName = string.IsNullOrWhiteSpace(_opt.FromName) ? _opt.FromEmail : _opt.FromName;
        message.From.Add(new MailboxAddress(fromName, _opt.FromEmail));

        foreach (var addr in to.Where(a => !string.IsNullOrWhiteSpace(a)))
            message.To.Add(new MailboxAddress(addr, addr));

        message.Subject = subject;

        var body = new BodyBuilder { HtmlBody = html, TextBody = text ?? string.Empty };
        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_opt.Host, _opt.Port, _opt.UseSsl, ct);

        if (!string.IsNullOrWhiteSpace(_opt.Username))
            await client.AuthenticateAsync(_opt.Username, _opt.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        return (true, message.MessageId ?? Guid.NewGuid().ToString());
    }
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; } = false;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "noreply@triggermail.local";
    public string? FromName { get; set; } = "TriggerMail";
}