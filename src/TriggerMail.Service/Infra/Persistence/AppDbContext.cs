using Microsoft.EntityFrameworkCore;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<EmailTrigger> EmailTriggers => Set<EmailTrigger>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}