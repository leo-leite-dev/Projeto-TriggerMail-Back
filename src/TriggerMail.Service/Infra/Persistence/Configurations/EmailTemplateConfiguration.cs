using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence.Configurations;

public sealed class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> e)
    {
        e.ToTable("email_templates");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Key).HasColumnName("key").IsRequired();
        e.Property(x => x.Subject).HasColumnName("subject").IsRequired();
        e.Property(x => x.Html).HasColumnName("html").IsRequired();
        e.Property(x => x.Text).HasColumnName("text");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        e.HasIndex(x => new { x.Key, x.Version }).IsUnique();
    }
}