using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mignow.Atc.Infra.Database.Context;

/// <summary>Só para o <c>dotnet ef migrations add</c> (não abre conexão).</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AtcDbContext>
{
    public AtcDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AtcDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time", n => n.MigrationsHistoryTable("__EFMigrationsHistory", AtcDbContext.Schema))
            .Options);
}
