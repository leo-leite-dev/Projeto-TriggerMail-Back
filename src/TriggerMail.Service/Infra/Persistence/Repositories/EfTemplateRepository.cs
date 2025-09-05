using Microsoft.EntityFrameworkCore;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence.Repositories;

public sealed class EfTemplateRepository : ITemplateRepository
{
    private readonly AppDbContext _db;
    public EfTemplateRepository(AppDbContext db) => _db = db;

    public async Task<EmailTemplate?> GetActiveByKeyAsync(string key, int? version, CancellationToken ct)
    {
        if (version is null)
            return await _db.EmailTemplates.AsNoTracking()
                .Where(t => t.Key == key && t.IsActive && t.Version == 1)
                .FirstOrDefaultAsync(ct);

        return await _db.EmailTemplates.AsNoTracking()
            .Where(t => t.Key == key && t.IsActive && t.Version == version.Value)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EmailTemplate> UpsertAsync(EmailTemplate template, CancellationToken ct)
    {
        var exists = await _db.EmailTemplates.AnyAsync(t => t.Key == template.Key && t.Version == template.Version, ct);
        if (!exists) _db.EmailTemplates.Add(template);
        else _db.EmailTemplates.Update(template);

        await _db.SaveChangesAsync(ct);
        return template;
    }
}
