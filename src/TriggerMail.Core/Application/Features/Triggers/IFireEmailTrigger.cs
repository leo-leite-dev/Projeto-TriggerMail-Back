using TriggerMail.Core.Application.Abstractions.Result;

namespace TriggerMail.Core.Application.Features.Triggers;

public interface IFireEmailTrigger
{
    Task<Result<Guid>> ExecuteAsync(string alias, string rawBody, string? signatureHeader, object? payload, CancellationToken ct = default);
}