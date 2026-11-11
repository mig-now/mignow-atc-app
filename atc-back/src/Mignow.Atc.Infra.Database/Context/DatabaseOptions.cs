namespace Mignow.Atc.Infra.Database.Context;

/// <summary>Seção <c>Database</c>.</summary>
public sealed class DatabaseOptions
{
    public const string Section = "Database";

    /// <summary>Fora de Development/Local, a migração no startup só acontece com esta opção ligada.</summary>
    public bool MigrateOnStartup { get; set; }

    /// <summary>Carga de demonstração (cliente DEMO e ambiente simulado). Ligada em Development.</summary>
    public bool SeedDemoData { get; set; }
}
