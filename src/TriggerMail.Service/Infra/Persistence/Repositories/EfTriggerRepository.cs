using Microsoft.EntityFrameworkCore;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence.Repositories;

public sealed class EfTriggerRepository : ITriggerRepository
{
    private readonly AppDbContext _db;
    public EfTriggerRepository(AppDbContext db) => _db = db;

    public Task<EmailTrigger?> GetByAliasAsync(string alias, CancellationToken ct)
        => _db.EmailTriggers.AsNoTracking().FirstOrDefaultAsync(x => x.Alias == alias, ct);

    public Task<bool> AliasExistsAsync(string alias, CancellationToken ct)
        => _db.EmailTriggers.AnyAsync(x => x.Alias == alias, ct);

    public async Task<EmailTrigger> AddAsync(EmailTrigger entity, CancellationToken ct)
    {
        _db.EmailTriggers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<EmailTrigger> UpdateAsync(EmailTrigger entity, CancellationToken ct)
    {
        _db.EmailTriggers.Update(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }
}