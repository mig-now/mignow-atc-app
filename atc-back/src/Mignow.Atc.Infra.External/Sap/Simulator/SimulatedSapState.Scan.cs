namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Estado do módulo Scan no simulador: classes temporárias existentes (nome → pacote). Chamar dentro de <c>WithLock</c>.</summary>
public sealed partial class SimulatedSapState
{
    private readonly Dictionary<string, string> _temporaryClasses = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Pacote da classe temporária, ou nulo se não existe.</summary>
    public string? TemporaryClassPackage(string name) => _temporaryClasses.GetValueOrDefault(name.Trim());

    public IReadOnlyCollection<string> TemporaryClasses => [.. _temporaryClasses.Keys];

    /// <summary>Testes: deixa um resíduo (classe de varredura anterior) no sistema.</summary>
    public void SeedTemporaryClass(string name, string package = "$TMP") => WithLock(() => _temporaryClasses[name.Trim().ToUpperInvariant()] = package);

    internal void AddTemporaryClass(string name) => _temporaryClasses[name.ToUpperInvariant()] = "$TMP";

    internal bool RemoveTemporaryClass(string name) => _temporaryClasses.Remove(name);
}
