using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class RemediationSessionMapping : IEntityTypeConfiguration<RemediationSession>
{
    public void Configure(EntityTypeBuilder<RemediationSession> b)
    {
        b.ToTable("TB_REMEDIATION_SESSION");
        b.Property(x => x.SourceRef).HasMaxLength(300);
        b.Property(x => x.Priorities).HasMaxLength(20).IsRequired();
        b.Property(x => x.Request).HasMaxLength(20);
        b.Property(x => x.Author).HasMaxLength(20).IsRequired();
        b.Property(x => x.CsvColumnsJson).AsJsonb().IsRequired();
        b.Property(x => x.ExcludedRowsJson).AsJsonb().IsRequired();
        b.Property(x => x.CsvEncoding).HasMaxLength(20).IsRequired();
        b.Property(x => x.CsvDelimiter).HasMaxLength(5).IsRequired();
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId);
        b.HasOne<SapSystem>().WithMany().HasForeignKey(x => x.SapSystemId);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.SessionId);
        b.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new { x.SapSystemId, x.Archived });
    }
}

internal sealed class RemediationItemMapping : IEntityTypeConfiguration<RemediationItem>
{
    public void Configure(EntityTypeBuilder<RemediationItem> b)
    {
        b.ToTable("TB_REMEDIATION_ITEM");
        b.Property(x => x.ObjectName).HasMaxLength(120).IsRequired();
        b.Property(x => x.ObjectType).HasMaxLength(10).IsRequired();
        b.Property(x => x.RefObject).HasMaxLength(200);
        b.Property(x => x.RefType).HasMaxLength(10);
        b.Property(x => x.Check).HasMaxLength(300);
        b.Property(x => x.Message).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(40);
        b.Property(x => x.Text).HasMaxLength(500);
        b.Property(x => x.Package).HasMaxLength(60);
        b.Property(x => x.Classification).HasMaxLength(20);
        b.Property(x => x.Priority).HasMaxLength(5);
        b.Property(x => x.FindingId).HasMaxLength(300);
        b.Property(x => x.Origin).HasMaxLength(10).IsRequired();
        b.Property(x => x.RawRowJson).AsJsonb().IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Obs).IsRequired();
        b.Property(x => x.StatusHistory).HasJsonConversion().IsRequired();
        b.Property(x => x.TriageFlags).HasJsonConversion().IsRequired();
        b.Property(x => x.Preparation).HasJsonConversion();
        b.Property(x => x.Proposal).HasJsonConversion();
        b.Property(x => x.Application).HasJsonConversion();
        b.Property(x => x.ApplicationResult).HasMaxLength(40);
        b.Property(x => x.AwaitingApproval).HasDefaultValue(false);
        b.HasIndex(x => new { x.SessionId, x.N }).IsUnique();
        b.HasIndex(x => new { x.SessionId, x.Status });
        b.HasIndex(x => new { x.SessionId, x.ObjectName });
    }
}

internal sealed class SourceCacheMapping : IEntityTypeConfiguration<SourceCache>
{
    public void Configure(EntityTypeBuilder<SourceCache> b)
    {
        b.ToTable("TB_SOURCE_CACHE");
        b.Property(x => x.ObjectName).HasMaxLength(120).IsRequired();
        b.Property(x => x.IncludeName).HasMaxLength(200).IsRequired();
        b.Property(x => x.AdtUri).HasMaxLength(500).IsRequired();
        b.Property(x => x.Source).IsRequired();
        b.Property(x => x.Hash).HasMaxLength(64).IsRequired();
        b.HasOne<SapSystem>().WithMany().HasForeignKey(x => x.SapSystemId);
        // Um por ambiente, objeto lido e endereço ADT: o mesmo include lido a partir de dois objetos (cache por objeto, como a
        // pasta fontes/<OBJ>/ da skill) fica nos dois.
        b.HasIndex(x => new { x.SapSystemId, x.ObjectName, x.AdtUri }).IsUnique();
        b.HasIndex(x => new { x.SapSystemId, x.ObjectName });
    }
}

internal sealed class TriageRuleMapping : IEntityTypeConfiguration<TriageRule>
{
    public void Configure(EntityTypeBuilder<TriageRule> b)
    {
        b.ToTable("TB_TRIAGE_RULE");
        b.Property(x => x.Key).HasMaxLength(40).IsRequired();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.Parameters).HasJsonConversion().IsRequired();
        b.Property(x => x.Effect).HasMaxLength(20).IsRequired();
        b.Property(x => x.Scope).HasMaxLength(20).IsRequired();
        b.Property(x => x.RefObjects).HasJsonConversion().IsRequired();
        b.Property(x => x.ObjectPattern).HasMaxLength(120);
        b.Property(x => x.MessagePattern).HasMaxLength(200);
        b.Property(x => x.Obs).HasMaxLength(500);
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.Key).IsUnique();
    }
}
