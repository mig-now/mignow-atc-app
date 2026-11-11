using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class RunMapping : IEntityTypeConfiguration<Run>
{
    /// <summary>Uma execução ativa por ambiente: índice único parcial.</summary>
    public const string ActiveRunIndex = "UX_RUN_ACTIVE_PER_SAP_SYSTEM";

    public void Configure(EntityTypeBuilder<Run> b)
    {
        b.ToTable("TB_RUN");
        b.Property(x => x.ParametersJson).AsJsonb().IsRequired();
        b.Property(x => x.ResultJson).AsJsonb();
        b.Property(x => x.StartedBy).HasMaxLength(200).IsRequired();
        b.Property(x => x.CancelledBy).HasMaxLength(200);
        b.Property(x => x.FailureCode).HasMaxLength(100);
        b.Property(x => x.AiCost).HasPrecision(18, 10);
        b.Ignore(x => x.IsActive);
        b.HasOne<SapSystem>().WithMany().HasForeignKey(x => x.SapSystemId);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId);
        b.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.RunId);
        b.Navigation(x => x.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => x.SapSystemId).IsUnique().HasDatabaseName(ActiveRunIndex)
            .HasFilter("\"STATUS\" IN ('Queued', 'Running', 'Cancelling')");
        b.HasIndex(x => new { x.Status, x.QueuedAt });
        b.HasIndex(x => x.SubjectId);
    }
}

internal sealed class RunStepMapping : IEntityTypeConfiguration<RunStep>
{
    public void Configure(EntityTypeBuilder<RunStep> b)
    {
        b.ToTable("TB_RUN_STEP");
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.MessageCode).HasMaxLength(100);
        b.HasIndex(x => new { x.RunId, x.Code }).IsUnique();
    }
}

internal sealed class RunAiCallMapping : IEntityTypeConfiguration<RunAiCall>
{
    public void Configure(EntityTypeBuilder<RunAiCall> b)
    {
        b.ToTable("TB_RUN_AI_CALL");
        b.Property(x => x.Model).HasMaxLength(120).IsRequired();
        b.Property(x => x.Cost).HasPrecision(18, 10);
        b.Property(x => x.ErrorCode).HasMaxLength(100);
        b.Property(x => x.ItemRef).HasMaxLength(200);
        b.HasOne<Run>().WithMany().HasForeignKey(x => x.RunId);
        b.HasIndex(x => x.RunId);
    }
}

internal sealed class RunEventMapping : IEntityTypeConfiguration<RunEvent>
{
    public void Configure(EntityTypeBuilder<RunEvent> b)
    {
        b.ToTable("TB_RUN_EVENT");
        b.Property(x => x.StepCode).HasMaxLength(40);
        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.ArgsJson).AsJsonb();
        b.Property(x => x.ItemRef).HasMaxLength(200);
        b.HasOne<Run>().WithMany().HasForeignKey(x => x.RunId);
        b.HasIndex(x => new { x.RunId, x.Id });
    }
}
