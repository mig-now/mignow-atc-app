using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class ExemptionBatchMapping : IEntityTypeConfiguration<ExemptionBatch>
{
    public void Configure(EntityTypeBuilder<ExemptionBatch> b)
    {
        b.ToTable("TB_EXEMPTION_BATCH");
        b.Property(x => x.Priorities).HasMaxLength(20).IsRequired();
        b.Property(x => x.Approver).HasMaxLength(40);
        b.Property(x => x.Reason).HasMaxLength(10).IsRequired();
        b.Property(x => x.Validity).HasMaxLength(10).IsRequired();
        b.Property(x => x.JustificationPrefix).HasMaxLength(200);
        b.Property(x => x.DiscoveryJson).AsJsonb();
        b.HasOne<RemediationSession>().WithMany().HasForeignKey(x => x.SessionId);
        b.HasOne<SapSystem>().WithMany().HasForeignKey(x => x.SapSystemId);
        b.HasMany(x => x.Requests).WithOne().HasForeignKey(x => x.BatchId);
        b.Navigation(x => x.Requests).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => x.SessionId);
    }
}

internal sealed class ExemptionRequestMapping : IEntityTypeConfiguration<ExemptionRequest>
{
    public void Configure(EntityTypeBuilder<ExemptionRequest> b)
    {
        b.ToTable("TB_EXEMPTION_REQUEST");
        b.Property(x => x.MarkerId).HasMaxLength(300).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(10).IsRequired();
        b.Property(x => x.Justification).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.Message).HasMaxLength(2000);
        b.HasOne<RemediationItem>().WithMany().HasForeignKey(x => x.ItemId);
        b.HasIndex(x => new { x.BatchId, x.Seq });
        // Itens repetidos da exportação do ATC (mesmo objeto/mensagem) recebem um finding cada: vários pedidos por item
        // (listar.py), então o item não é único no lote; o finding (markerId) é.
        b.HasIndex(x => new { x.BatchId, x.ItemId });
        b.HasIndex(x => new { x.BatchId, x.MarkerId }).IsUnique();
        b.HasIndex(x => x.MarkerId);
    }
}
