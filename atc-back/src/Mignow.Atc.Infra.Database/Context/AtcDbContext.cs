using Microsoft.EntityFrameworkCore;
using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Infra.Database.Context;

/// <summary>
/// Um banco, schema <c>public</c>. Mapeamentos em arquivos por módulo (<c>Mappings/*Mappings.cs</c>) aplicados por
/// <c>ApplyConfigurationsFromAssembly</c>. Convenções: tabelas TB_*, colunas MAIÚSCULAS_COM_SUBLINHADO, enums como texto,
/// FKs sem cascata. Migrations só aqui; o único executor é o DatabaseMigrator.
/// </summary>
public sealed class AtcDbContext(DbContextOptions<AtcDbContext> options) : DbContext(options)
{
    public const string Schema = "public";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtcDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entity.ClrType))
            {
                modelBuilder.Entity(entity.ClrType).HasKey(nameof(EntityBase.Id));
                modelBuilder.Entity(entity.ClrType).Property(nameof(EntityBase.Guid)).ValueGeneratedNever();
                modelBuilder.Entity(entity.ClrType).HasIndex(nameof(EntityBase.Guid)).IsUnique();
                modelBuilder.Entity(entity.ClrType).Property(nameof(EntityBase.CreatedBy)).HasMaxLength(200);
                modelBuilder.Entity(entity.ClrType).Property(nameof(EntityBase.UpdatedBy)).HasMaxLength(200);
            }
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ColumnNames.Of(property.Name));
                if (property.ClrType.IsEnum || Nullable.GetUnderlyingType(property.ClrType)?.IsEnum == true)
                {
                    property.SetProviderClrType(typeof(string));
                    property.SetMaxLength(40);
                }
            }
            foreach (var fk in entity.GetForeignKeys())
                fk.DeleteBehavior = DeleteBehavior.ClientNoAction;
        }
    }
}
