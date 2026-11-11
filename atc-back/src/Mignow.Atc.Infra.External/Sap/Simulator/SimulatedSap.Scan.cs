using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Módulo Scan no simulador: <see cref="IMignowScanner"/> sobre os objetos do cenário. O inventário vem da tabela <c>TADIR</c> do
/// cenário (se houver) ou dos próprios objetos; a "classe temporária" é registrada no estado e o lote roda as regex sobre os fontes
/// ativos (<see cref="SimulatedScanEngine"/>), devolvendo a saída no formato do template. Falhas injetáveis (testes): operações
/// <c>SCAN_CREATE</c>, <c>SCAN_ACTIVATE</c>, <c>SCAN_RUN</c> e <c>SCAN_REMOVE</c> (alvo = nome da classe).
/// </summary>
public sealed partial class SimulatedSap : IMignowScanner
{
    public Task<Result<IReadOnlyList<ScanTadirRow>>> InventoryAsync(SapConnection connection, IReadOnlyList<string> types, CancellationToken ct) =>
        With(connection, s =>
        {
            s.Record("SCAN_INVENTORY");
            var wanted = types.Select(t => t.ToUpperInvariant()).ToHashSet(StringComparer.Ordinal);
            IEnumerable<ScanTadirRow> rows;
            var table = s.Scenario.Tables.FirstOrDefault(t => string.Equals(t.Key, "TADIR", StringComparison.OrdinalIgnoreCase)).Value;
            if (table is not null)
                rows = table.Select(r => new ScanTadirRow(V(r, "OBJECT"), V(r, "OBJ_NAME"), V(r, "DEVCLASS"), V(r, "AUTHOR"), V(r, "GENFLAG"), V(r, "DELFLAG")));
            else
                rows = s.Objects.Select(o => (Type: TadirType(o.Ref.Type), o)).Where(x => x.Type is not null)
                    .Select(x => new ScanTadirRow(x.Type!, x.o.Ref.Name, x.o.Ref.Package ?? string.Empty, SimulatedSapState.SimulatorUser, string.Empty, string.Empty));
            IReadOnlyList<ScanTadirRow> result =
            [
                .. rows.Where(r => r.Type.Length > 0 && wanted.Contains(r.Type) && StartsZy(r.Name) || wanted.Contains(r.Type) && StartsZy(r.Package)),
            ];
            return Result.Success(result);
        });

    public Task<Result<ScanSimulation>> SimulateAsync(SapConnection connection, IReadOnlyList<string> classNames, CancellationToken ct) =>
        With(connection, s =>
        {
            s.Record("SCAN_SIMULATE");
            var category = string.IsNullOrWhiteSpace(s.ClientCategory) ? "?" : s.ClientCategory.Trim().ToUpperInvariant();
            var ok = !connection.IsProduction && category is not ("P" or "?");
            var classes = classNames.Select(n =>
            {
                var name = n.ToUpperInvariant();
                return s.TemporaryClassPackage(name) is { } package
                    ? new ScanClassCheck(name, true, package, false, $"classe já existe (pacote {package}): rode a remoção de resíduos se for resíduo da varredura anterior")
                    : new ScanClassCheck(name, false, string.Empty, ScanProcessPlannerCheck(name), null);
            }).ToList();
            return Result.Success(new ScanSimulation(category, string.Empty, ok, classes));
        });

    public async Task<Result<ScanProcessResult>> ExecuteProcessAsync(
        SapConnection connection, ScanProcessRequest request, Func<ScanLotResult, Task<bool>> onLot, CancellationToken ct)
    {
        var guard = await With(connection, s => GuardWrite(s, connection));
        if (guard.IsFailure)
            return guard.Error;
        var state = registry.Get(connection.BaseUrl)!;
        var cls = request.ClassName.ToUpperInvariant();
        var created = false;
        string? error = null;
        ScanClassRemoval removal;
        try
        {
            state.WithLock(() =>
            {
                if (state.ShouldFail("SCAN_CREATE", cls))
                    error = "Criação da classe falhou: HTTP 500: falha simulada";
                else if (state.TemporaryClassPackage(cls) is not null)
                    error = "Criação da classe falhou: HTTP 400: classe já existe";
                else
                {
                    state.AddTemporaryClass(cls);
                    state.Record($"SCAN_CLASS_CREATE {cls}");
                    created = true;
                }
                return true;
            });
            if (created)
                foreach (var lot in request.Lots)
                {
                    if (ct.IsCancellationRequested)
                        break;
                    var result = state.WithLock(() => RunLot(state, cls, lot));
                    if (!await onLot(result) || result.Status != ScanBatchStatus.Ok)
                        break;
                }
        }
        finally
        {
            removal = state.WithLock(() => Remove(state, cls));
        }
        return new ScanProcessResult(cls, created, error, removal);
    }

    public Task<Result<IReadOnlyList<ScanClassRemoval>>> RemoveResiduesAsync(SapConnection connection, IReadOnlyList<string> classNames, CancellationToken ct) =>
        With(connection, s =>
        {
            var guard = GuardWrite(s, connection);
            if (guard.IsFailure)
                return Result.Failure<IReadOnlyList<ScanClassRemoval>>(guard.Error);
            IReadOnlyList<ScanClassRemoval> list = [.. classNames.Select(n => Remove(s, n.ToUpperInvariant()))];
            return Result.Success(list);
        });

    private static ScanLotResult RunLot(SimulatedSapState s, string cls, ScanLotRequest lot)
    {
        s.Record($"SCAN_LOT {cls} {lot.Number:D3}");
        if (s.ShouldFail("SCAN_ACTIVATE", cls))
            return new ScanLotResult(lot.Number, ScanBatchStatus.ActivationError, string.Empty, "ativação da classe gerada falhou", ["E: falha simulada"], 0);
        if (s.ShouldFail("SCAN_RUN", cls))
            return new ScanLotResult(lot.Number, ScanBatchStatus.ExecutionError, string.Empty, "HTTP 500: falha simulada", [], 0);
        var output = SimulatedScanEngine.Run(lot, obj => SourcesOf(s, obj));
        return new ScanLotResult(lot.Number, ScanBatchStatus.Ok, output, null, [], 0);
    }

    private static ScanClassRemoval Remove(SimulatedSapState s, string cls)
    {
        var package = s.TemporaryClassPackage(cls);
        if (package is null)
            return new ScanClassRemoval(cls, true, "nao_existia", []);
        if (package != "$TMP")
            return new ScanClassRemoval(cls, false, $"NÃO REMOVIDA: classe existe no pacote {package}, não é a temporária", []);
        if (s.ShouldFail("SCAN_REMOVE", cls))
            return new ScanClassRemoval(cls, false, $"NÃO REMOVIDA: apague a classe {cls} (SE24/Eclipse, pacote $TMP)", ["DELETE: HTTP 500"]);
        s.RemoveTemporaryClass(cls);
        s.Record($"SCAN_CLASS_DELETE {cls}");
        return new ScanClassRemoval(cls, true, "removida", ["DELETE: HTTP 204"]);
    }

    /// <summary>Fontes do objeto TADIR como a classe os lê (ativos): programa/include, classe, interface, grupo de funções + módulos.</summary>
    private static IReadOnlyList<SimulatedScanSource> SourcesOf(SimulatedSapState s, ScanLotObject obj)
    {
        string[] Lines(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var list = new List<SimulatedScanSource>();
        var o = s.Get(obj.Name);
        if (o is null)
            return list;
        string Path(SimulatedObject x) => x.Ref.Uri["/sap/bc/adt/".Length..];
        list.Add(new SimulatedScanSource(Path(o), Lines(o.ActiveSource)));
        if (obj.Type == "FUGR")
            list.AddRange(s.Objects.Where(f => f.Ref.Type == "FUGR/FF" && string.Equals(f.MainProgram, o.Ref.Name, StringComparison.OrdinalIgnoreCase))
                .Select(f => new SimulatedScanSource(Path(f), Lines(f.ActiveSource))));
        return list;
    }

    private static string? TadirType(string adtType) => adtType switch
    {
        _ when adtType.StartsWith("PROG", StringComparison.Ordinal) => "PROG",
        _ when adtType.StartsWith("CLAS", StringComparison.Ordinal) => "CLAS",
        _ when adtType.StartsWith("INTF", StringComparison.Ordinal) => "INTF",
        "FUGR/F" => "FUGR",
        _ => null,
    };

    private static bool StartsZy(string s) => s.StartsWith('Z') || s.StartsWith('Y');

    private static bool ScanProcessPlannerCheck(string name) => System.Text.RegularExpressions.Regex.IsMatch(name, "^[ZY][A-Z0-9_]{1,29}$");

    private static string V(IReadOnlyDictionary<string, string> row, string column) =>
        row.FirstOrDefault(kv => string.Equals(kv.Key, column, StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty;
}
