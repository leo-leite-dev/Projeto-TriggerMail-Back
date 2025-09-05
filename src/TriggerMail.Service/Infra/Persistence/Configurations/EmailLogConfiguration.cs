using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence.Configurations;

public sealed class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> e)
    {
        e.ToTable("email_logs");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.MessageId).HasColumnName("message_id").IsRequired();
        e.Property(x => x.TemplateKey).HasColumnName("template_key").IsRequired();
        e.Property(x => x.Lang).HasColumnName("lang").HasDefaultValue("pt-BR");
        e.Property(x => x.Subject).HasColumnName("subject").IsRequired();

        e.Property(x => x.Recipients)
            .HasColumnName("recipients")
            .HasColumnType("text[]");

        e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        e.Property(x => x.Status).HasColumnName("status").HasDefaultValue("Queued");
        e.Property(x => x.ProviderMessageId).HasColumnName("provider_message_id");

        e.Property(x => x.AttemptCount).HasColumnName("attempt_count").HasDefaultValue(0);
        e.Property(x => x.LastError).HasColumnName("last_error");
        e.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
        e.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        e.HasIndex(x => x.MessageId).IsUnique();
    }
}