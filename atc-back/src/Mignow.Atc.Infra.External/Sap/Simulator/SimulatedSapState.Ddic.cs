namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Estado DDIC do simulador (carregado do cenário na primeira consulta). Chamar dentro de <c>WithLock</c>.</summary>
public sealed partial class SimulatedSapState
{
    private Dictionary<string, SimulatedDdicObject>? _ddic;

    private Dictionary<string, SimulatedDdicObject> Ddic => _ddic ??= Scenario.DdicObjects.ToDictionary(
        o => DdicKey(o.Kind, o.Name),
        o => new SimulatedDdicObject
        {
            Name = o.Name.ToUpperInvariant(), Kind = o.Kind.ToUpperInvariant(), Package = o.Package, Source = o.Source, ActiveSource = o.Source,
            Inactive = o.Inactive, OpenRequest = o.OpenRequest,
        },
        StringComparer.OrdinalIgnoreCase);

    public static string DdicKey(string kind, string name) => $"{kind.ToUpperInvariant()}:{name.Trim().ToUpperInvariant()}";

    public SimulatedDdicObject? GetDdic(string kind, string name) => Ddic.GetValueOrDefault(DdicKey(kind, name));

    public SimulatedDdicObject AddDdic(string kind, string name, string package)
    {
        var o = new SimulatedDdicObject { Name = name.ToUpperInvariant(), Kind = kind.ToUpperInvariant(), Package = package, Source = string.Empty, ActiveSource = string.Empty, Inactive = true };
        Ddic[DdicKey(kind, name)] = o;
        return o;
    }
}
