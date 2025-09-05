using Microsoft.EntityFrameworkCore;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence;

public sealed class EfEmailLogRepository : IEmailLogRepository
{
    private readonly AppDbContext _db;
    public EfEmailLogRepository(AppDbContext db) => _db = db;

    public async Task<Guid> StartAsync(EmailLog log, CancellationToken ct)
    {
        _db.EmailLogs.Add(log);
        await _db.SaveChangesAsync(ct);
        return log.Id;
    }

    public async Task CompleteAsync(Guid logId, string providerMessageId, string status, CancellationToken ct)
    {
        var entity = await _db.EmailLogs.FirstOrDefaultAsync(x => x.Id == logId, ct);
        if (entity is null) return;
        entity.Complete(providerMessageId, status);
        _db.EmailLogs.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RegisterAttemptAsync(Guid logId, string? error, string status, DateTimeOffset? nextAttemptAt, CancellationToken ct)
    {
        var entity = await _db.EmailLogs.FirstOrDefaultAsync(x => x.Id == logId, ct);
        if (entity is null) return;
        entity.RegisterAttempt(error, status, nextAttemptAt);
        _db.EmailLogs.Update(entity);
        await _db.SaveChangesAsync(ct);
    }
}