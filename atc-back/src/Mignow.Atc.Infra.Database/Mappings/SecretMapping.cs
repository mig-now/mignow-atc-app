using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mignow.Atc.Infra.Database.Context;
using Mignow.Atc.Infra.Database.Secrets;

namespace Mignow.Atc.Infra.Database.Mappings;

internal sealed class SecretMapping : IEntityTypeConfiguration<StoredSecret>
{
    public void Configure(EntityTypeBuilder<StoredSecret> b)
    {
        b.ToTable("TB_SECRET");
        b.HasKey(x => x.Id);
        b.Property(x => x.Ref).HasMaxLength(100).IsRequired();
        b.Property(x => x.Purpose).HasMaxLength(120).IsRequired();
        b.HasIndex(x => x.Ref).IsUnique();
    }
}
