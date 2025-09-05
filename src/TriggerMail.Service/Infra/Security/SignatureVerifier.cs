using System.Security.Cryptography;
using System.Text;
using TriggerMail.Core.Application.Ports.Security;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Security;

public sealed class SignatureVerifier : ISignatureVerifier
{
    public Task<bool> IsValidAsync(EmailTrigger trigger, string rawBody, string? signatureHeader, CancellationToken ct)
    {
        return trigger.AuthType.ToLowerInvariant() switch
        {
            "none" => Task.FromResult(true),
            "token" => Task.FromResult(!string.IsNullOrWhiteSpace(signatureHeader) &&
                                       signatureHeader == trigger.AuthSecret),
            "hmac" => Task.FromResult(VerifyHmac(trigger.AuthSecret, rawBody, signatureHeader)),
            _ => Task.FromResult(false)
        };
    }

    private static bool VerifyHmac(string? secret, string body, string? header)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(header)) return false;
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(body));
        var signature = Convert.ToHexString(hash).ToLowerInvariant();
        return SlowEquals(signature, header.Trim().ToLowerInvariant());
    }

    private static bool SlowEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}