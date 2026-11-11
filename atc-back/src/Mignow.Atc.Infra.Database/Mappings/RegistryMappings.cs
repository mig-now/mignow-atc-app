using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class CustomerMapping : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("TB_CUSTOMER");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Code).HasMaxLength(40);
    }
}

internal sealed class SapSystemMapping : IEntityTypeConfiguration<SapSystem>
{
    public void Configure(EntityTypeBuilder<SapSystem> b)
    {
        b.ToTable("TB_SAP_SYSTEM");
        b.Property(x => x.Sid).HasMaxLength(3).IsRequired();
        b.Property(x => x.Client).HasMaxLength(3).IsRequired();
        b.Property(x => x.Host).HasMaxLength(200).IsRequired();
        b.Property(x => x.Language).HasMaxLength(2).IsRequired();
        b.Property(x => x.CertThumbprint).HasMaxLength(95);
        b.Property(x => x.Author).HasMaxLength(20).IsRequired();
        b.Property(x => x.DefaultRequest).HasMaxLength(20);
        b.Property(x => x.ExemptionApprover).HasMaxLength(40);
        b.Property(x => x.Username).HasMaxLength(40).IsRequired();
        b.Property(x => x.PasswordSecretRef).HasMaxLength(100);
        b.Property(x => x.Release).HasMaxLength(120);
        b.Property(x => x.ClientCategory).HasMaxLength(1);
        b.Property(x => x.LastConnectionTestMessage).HasMaxLength(500);
        b.Ignore(x => x.IsSimulator);
        b.Ignore(x => x.BaseUrl);
        b.Ignore(x => x.DisplayName);
        b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
        b.HasIndex(x => new { x.CustomerId, x.Sid, x.Client }).IsUnique();
    }
}

internal sealed class AiProviderMapping : IEntityTypeConfiguration<AiProvider>
{
    public void Configure(EntityTypeBuilder<AiProvider> b)
    {
        b.ToTable("TB_AI_PROVIDER");
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Model).HasMaxLength(120).IsRequired();
        b.Property(x => x.ApiKeySecretRef).HasMaxLength(100);
    }
}

internal sealed class GeneralSettingsMapping : IEntityTypeConfiguration<GeneralSettings>
{
    public void Configure(EntityTypeBuilder<GeneralSettings> b)
    {
        b.ToTable("TB_GENERAL_SETTINGS");
        b.Property(x => x.KbPath).HasMaxLength(500).IsRequired();
        b.Property(x => x.MignowRulesPath).HasMaxLength(500).IsRequired();
    }
}
