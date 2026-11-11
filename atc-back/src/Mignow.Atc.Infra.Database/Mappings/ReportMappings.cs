using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Report;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class ReportHighlightsMapping : IEntityTypeConfiguration<ReportHighlights>
{
    public void Configure(EntityTypeBuilder<ReportHighlights> b)
    {
        b.ToTable("TB_REPORT_HIGHLIGHTS");
        b.Property(x => x.Highlights).HasJsonConversion().IsRequired();
        b.Property(x => x.Pending).HasJsonConversion().IsRequired();
        b.Property(x => x.Premise).HasJsonConversion();
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId);
        b.HasIndex(x => x.CustomerId).IsUnique();
    }
}
