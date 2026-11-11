using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mignow.Atc.Infra.Database.Context;

/// <summary>Único ponto autorizado a migrar: Dev/Local sempre; demais ambientes só com Database:MigrateOnStartup.</summary>
public sealed class DatabaseMigrator(AtcDbContext db, IHostEnvironment env, IOptions<DatabaseOptions> options, ILogger<DatabaseMigrator> logger)
{
    public async Task MigrateAsync(CancellationToken ct = default)
    {
        if (!db.Database.IsRelational())
            return;
        if (!env.IsDevelopment() && !env.IsEnvironment("Local") && !options.Value.MigrateOnStartup)
        {
            logger.LogInformation("Migração no startup desligada neste ambiente.");
            return;
        }
        await db.Database.MigrateAsync(ct);
    }
}
