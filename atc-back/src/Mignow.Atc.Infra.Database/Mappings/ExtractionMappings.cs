using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class AtcExtractionMapping : IEntityTypeConfiguration<AtcExtraction>
{
    public void Configure(EntityTypeBuilder<AtcExtraction> b)
    {
        b.ToTable("TB_ATC_EXTRACTION");
        b.Property(x => x.Series).HasMaxLength(120).IsRequired();
        b.Property(x => x.DisplayId).HasMaxLength(120).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300);
        b.Property(x => x.VariationJson).AsJsonb();
        b.Property(x => x.ColumnsJson).AsJsonb().IsRequired();
        b.Property(x => x.ExecutionJson).AsJsonb();
        b.HasOne<SapSystem>().WithMany().HasForeignKey(x => x.SapSystemId);
        b.HasIndex(x => new { x.SapSystemId, x.DisplayId });
    }
}

internal sealed class AtcFindingMapping : IEntityTypeConfiguration<AtcFinding>
{
    public void Configure(EntityTypeBuilder<AtcFinding> b)
    {
        b.ToTable("TB_ATC_FINDING");
        b.Property(x => x.CheckTitle).HasMaxLength(300);
        b.Property(x => x.MessageTitle).HasMaxLength(500);
        b.Property(x => x.ObjectName).HasMaxLength(120).IsRequired();
        b.Property(x => x.ObjectType).HasMaxLength(10);
        b.Property(x => x.Package).HasMaxLength(60);
        b.Property(x => x.RefObjectName).HasMaxLength(200);
        b.Property(x => x.RefObjectType).HasMaxLength(10);
        b.Property(x => x.Note).HasMaxLength(40);
        b.Property(x => x.FindingId).HasMaxLength(300);
        b.Property(x => x.AllColumnsJson).AsJsonb().IsRequired();
        b.HasOne<AtcExtraction>().WithMany().HasForeignKey(x => x.ExtractionId);
        b.HasIndex(x => new { x.ExtractionId, x.Seq }).IsUnique();
        b.HasIndex(x => new { x.ExtractionId, x.Priority, x.InBaseline });
        b.HasIndex(x => new { x.ExtractionId, x.ObjectName });
    }
}
