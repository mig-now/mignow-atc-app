using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Scan;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class MignowScanMapping : IEntityTypeConfiguration<MignowScan>
{
    public void Configure(EntityTypeBuilder<MignowScan> b)
    {
        b.ToTable("TB_MIGNOW_SCAN");
        b.Property(x => x.RulesSnapshotJson).AsJsonb().IsRequired();
        b.Property(x => x.ScopeCountsJson).AsJsonb();
        b.Property(x => x.SummaryJson).AsJsonb();
        b.Property(x => x.Batches).HasJsonConversion().IsRequired();
        b.HasOne<SapSystem>().WithMany().HasForeignKey(x => x.SapSystemId);
        b.HasOne<AtcExtraction>().WithMany().HasForeignKey(x => x.ExtractionId);
        b.HasIndex(x => x.SapSystemId);
    }
}

internal sealed class MignowOccurrenceMapping : IEntityTypeConfiguration<MignowOccurrence>
{
    public void Configure(EntityTypeBuilder<MignowOccurrence> b)
    {
        b.ToTable("TB_MIGNOW_OCCURRENCE");
        b.Property(x => x.RuleId).HasMaxLength(60).IsRequired();
        b.Property(x => x.ObjectName).HasMaxLength(120).IsRequired();
        b.Property(x => x.ObjectType).HasMaxLength(10).IsRequired();
        b.Property(x => x.Include).HasMaxLength(200);
        b.Property(x => x.AdtSource).HasMaxLength(500);
        b.Property(x => x.Cross).HasMaxLength(40);
        b.Property(x => x.Class).HasMaxLength(20);
        b.Property(x => x.Element).HasMaxLength(200).IsRequired();
        b.HasOne<MignowScan>().WithMany().HasForeignKey(x => x.ScanId);
        b.HasIndex(x => new { x.ScanId, x.RuleId });
        b.HasIndex(x => new { x.ScanId, x.ObjectName });
        b.HasIndex(x => new { x.ScanId, x.Batch });
    }
}
