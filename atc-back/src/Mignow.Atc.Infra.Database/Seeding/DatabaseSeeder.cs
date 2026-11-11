using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Exceptions;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Seeding;

/// <summary>
/// Seed idempotente em runtime: parâmetros, provedores de IA (Haiku/Sonnet/Opus 5.5 por uso + Fake) e, em Development,
/// a carga de demonstração (cliente DEMO e ambiente <c>sim://migracao-tipica</c>). Corrida entre réplicas é tolerada.
/// </summary>
public sealed class DatabaseSeeder(AtcDbContext db, EfDataStore store, IOptions<DatabaseOptions> options, ILogger<DatabaseSeeder> logger)
{
    public const string DemoCustomerCode = "DEMO";
    public const string DemoHost = "sim://migracao-tipica";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        try
        {
            await SeedCoreAsync(ct);
            if (options.Value.SeedDemoData)
                await SeedDemoAsync(ct);
        }
        catch (ConflictException)
        {
            logger.LogInformation("Seed concorrente detectado; outra instância já gravou.");
        }
    }

    private async Task SeedCoreAsync(CancellationToken ct)
    {
        if (!await db.Set<GeneralSettings>().AnyAsync(ct))
            db.Add(GeneralSettings.CreateDefault());
        if (!await db.Set<AiProvider>().AnyAsync(ct))
        {
            // Um só modelo para todos os usos, padrão Claude Opus (decisão de 09.11.2026). A chave é cadastrada na ⚙; o custo
            // é o que o serviço informa.
            var claude = AiProvider.Create("Claude Opus 5.5", AiProviderKind.Anthropic, "claude-opus-5-5");
            var fake = AiProvider.Create("Fake (desenvolvimento)", AiProviderKind.Fake, "fake");
            db.AddRange(claude, fake);
            // Em desenvolvimento/demonstração o padrão é o Fake (sem chave nem rede); nos demais, o Claude Opus.
            (options.Value.SeedDemoData ? fake : claude).MarkDefault(true);
        }
        await store.SaveChangesAsync(ct);
    }

    private async Task SeedDemoAsync(CancellationToken ct)
    {
        if (await db.Set<Customer>().AnyAsync(c => c.Code == DemoCustomerCode, ct))
            return;
        var customer = Customer.Create("Cliente Demonstração", DemoCustomerCode);
        var system = SapSystem.Create(customer, "SBX", "100", DemoHost, 0, false, "EN", null, false, ProjectType.Conversion, "AUT-DEV", null, null,
            "MIGATC");
        db.AddRange(customer, system);
        await store.SaveChangesAsync(ct);
        logger.LogInformation("Carga de demonstração criada (cliente DEMO, ambiente {Host}).", DemoHost);
    }
}
