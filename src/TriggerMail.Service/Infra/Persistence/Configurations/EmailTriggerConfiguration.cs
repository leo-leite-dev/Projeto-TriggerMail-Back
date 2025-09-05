using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriggerMail.Core.Domain.Entities;

namespace TriggerMail.Service.Infra.Persistence.Configurations;

public sealed class EmailTriggerConfiguration : IEntityTypeConfiguration<EmailTrigger>
{
    public void Configure(EntityTypeBuilder<EmailTrigger> e)
    {
        e.ToTable("email_triggers");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Alias).HasColumnName("alias").IsRequired();
        e.Property(x => x.TemplateKey).HasColumnName("template_key").IsRequired();
        e.Property(x => x.Lang).HasColumnName("lang").HasDefaultValue("pt-BR");
        e.Property(x => x.AuthType).HasColumnName("auth_type").HasDefaultValue("hmac");
        e.Property(x => x.AuthSecret).HasColumnName("auth_secret");
        e.Property(x => x.MappingConfigJson).HasColumnName("mapping_config_json");
        e.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true);

        e.Property(x => x.DefaultRecipients)
         .HasColumnName("default_recipients")
         .HasColumnType("text[]");

        e.HasIndex(x => x.Alias).IsUnique();
    }
}