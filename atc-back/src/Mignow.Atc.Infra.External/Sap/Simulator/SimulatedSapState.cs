using System.Text.RegularExpressions;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Domain.ValueObjects;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Um "SAP" em memória carregado de um cenário: fontes (ativo e inativo), locks, erros de sintaxe pelas regras, T000,
/// tabelas do SQL, diário das operações e falhas injetadas (testes). Thread-safe por um lock só. <c>partial</c>:
/// módulos acrescentam estado em <c>SimulatedSapState.&lt;Módulo&gt;.cs</c>.
/// </summary>
public sealed partial class SimulatedSapState
{
    public const string SimulatorUser = "MIGATC";

    private readonly Lock _gate = new();
    private readonly Dictionary<string, SimulatedObject> _objects = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _journal = [];
    private readonly Dictionary<(string Operation, string Target), int> _failures = [];

    public SimulatedSapState(SapScenario scenario)
    {
        Scenario = scenario;
        ClientCategory = scenario.ClientCategory;
        foreach (var o in scenario.Objects)
            _objects[o.Name] = new SimulatedObject
            {
                Ref = new SapObjectRef(o.Name.ToUpperInvariant(), o.Type, UriFor(o.Name, o.Type, o.MainProgram), o.Package.ToUpperInvariant()),
                Source = o.Source,
                ActiveSource = o.Source,
                Inactive = o.Inactive,
                LockedBy = o.LockedBy,
                OpenRequest = o.OpenRequest,
                Dependencies = [.. o.Dependencies ?? []],
                UsedBy = [.. o.UsedBy ?? []],
                MainProgram = o.MainProgram,
            };
    }

    public SapScenario Scenario { get; }

    /// <summary>T000-CCCATEGORY atual (testes trocam para P ou nulo para provar a trava).</summary>
    public string? ClientCategory { get; set; }

    public IReadOnlyList<string> Journal
    {
        get { lock (_gate) return [.. _journal]; }
    }

    /// <summary>Faz a operação falhar <paramref name="times"/> vezes no alvo (nome do objeto).</summary>
    public void InjectFailure(string operation, string target, int times = 1)
    {
        lock (_gate) _failures[(operation, target.ToUpperInvariant())] = times;
    }

    public T WithLock<T>(Func<T> action)
    {
        lock (_gate) return action();
    }

    // Chamar os métodos abaixo dentro de WithLock.

    public SimulatedObject? Get(string name) => _objects.GetValueOrDefault(name.Trim());

    public IEnumerable<SimulatedObject> Objects => _objects.Values;

    public SimulatedObject? FindByUri(string uri)
    {
        var path = uri.Split('#', '?')[0].TrimEnd('/');
        if (path.EndsWith("/source/main", StringComparison.OrdinalIgnoreCase))
            path = path[..^"/source/main".Length];
        return _objects.Values.FirstOrDefault(o => string.Equals(o.Ref.Uri, path, StringComparison.OrdinalIgnoreCase));
    }

    public void Record(string entry) => _journal.Add(entry);

    public bool ShouldFail(string operation, string target)
    {
        var key = (operation, target.ToUpperInvariant());
        if (!_failures.TryGetValue(key, out var remaining) || remaining <= 0)
            return false;
        _failures[key] = remaining - 1;
        return true;
    }

    /// <summary>Erros do fonte pelas regras de sintaxe (linha de comentário <c>*</c> não gera erro, como no SAP).</summary>
    public IReadOnlyList<SyntaxMessage> SyntaxErrorsOf(SimulatedObject obj, string source) =>
        [.. source.Split('\n').Select((line, i) => (line, i))
            .SelectMany(x => Scenario.SyntaxRules
                .Where(r => !x.line.StartsWith('*') && x.line.Contains(r.Pattern, StringComparison.OrdinalIgnoreCase))
                .Select(r => new SyntaxMessage("E", r.Message, x.i + 1, obj.Ref.Uri.Replace("/sap/bc/adt/", string.Empty, StringComparison.Ordinal))))];

    public static string UriFor(string name, string type, string? group = null)
    {
        var n = Uri.EscapeDataString(name.ToLowerInvariant());
        return type switch
        {
            "PROG/I" => $"/sap/bc/adt/programs/includes/{n}",
            "CLAS/OC" => $"/sap/bc/adt/oo/classes/{n}",
            "INTF/OI" => $"/sap/bc/adt/oo/interfaces/{n}",
            "FUGR/F" => $"/sap/bc/adt/functions/groups/{n}",
            "FUGR/FF" => $"/sap/bc/adt/functions/groups/{Uri.EscapeDataString((group ?? "unknown").ToLowerInvariant())}/fmodules/{n}",
            _ => $"/sap/bc/adt/programs/programs/{n}",
        };
    }

    public bool IsCustomer(SimulatedObject obj) => SapObjectName.IsCustomer(obj.Ref.Package);

    /// <summary>
    /// SQL simulado: <c>SELECT a, b | * FROM tabela [WHERE c = 'v' AND …]</c>. T000 sai da categoria do cenário quando a
    /// tabela não está no cenário. Outro formato → nulo (não suportado).
    /// </summary>
    public SqlResult? Select(string sql, string client, int maxRows)
    {
        var m = SelectRegex().Match(sql.Replace('\n', ' '));
        if (!m.Success)
            return null;
        var table = m.Groups[2].Value.ToUpperInvariant();
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows;
        if (Scenario.Tables.FirstOrDefault(t => string.Equals(t.Key, table, StringComparison.OrdinalIgnoreCase)) is { Value: { } found })
            rows = found;
        else if (table == "T000")
            rows = ClientCategory is null ? [] : [new Dictionary<string, string> { ["MANDT"] = client, ["CCCATEGORY"] = ClientCategory }];
        else
            rows = [];
        var filters = new List<(string Column, HashSet<string> Values)>();
        if (m.Groups[3].Success)
            foreach (var cond in AndRegex().Split(m.Groups[3].Value.Trim()))
            {
                var c = ConditionRegex().Match(cond.Trim());
                if (c.Success)
                {
                    filters.Add((c.Groups[1].Value, new HashSet<string>([c.Groups[2].Value], StringComparer.OrdinalIgnoreCase)));
                    continue;
                }
                var i = InRegex().Match(cond.Trim());
                if (!i.Success)
                    return null;
                filters.Add((i.Groups[1].Value, new HashSet<string>(
                    QuotedRegex().Matches(i.Groups[2].Value).Select(q => q.Groups[1].Value.Replace("''", "'")), StringComparer.OrdinalIgnoreCase)));
            }
        var columns = m.Groups[1].Value.Trim() == "*"
            ? rows.SelectMany(r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase).Select(k => k.ToUpperInvariant()).ToList()
            : m.Groups[1].Value.Split(',').Select(c => c.Trim().ToUpperInvariant()).ToList();
        var selected = rows
            .Where(r => filters.All(f => f.Values.Contains(Value(r, f.Column))))
            .Take(Math.Max(0, maxRows))
            .Select(r => (IReadOnlyDictionary<string, string>)columns.ToDictionary(c => c, c => Value(r, c), StringComparer.OrdinalIgnoreCase))
            .ToList();
        return new SqlResult(columns, selected);
    }

    private static string Value(IReadOnlyDictionary<string, string> row, string column) =>
        row.FirstOrDefault(kv => string.Equals(kv.Key, column, StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty;

    [GeneratedRegex(@"^\s*SELECT\s+(.+?)\s+FROM\s+([A-Za-z0-9_/]+)(?:\s+WHERE\s+(.+?))?\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SelectRegex();

    [GeneratedRegex(@"\s+AND\s+", RegexOptions.IgnoreCase)]
    private static partial Regex AndRegex();

    [GeneratedRegex(@"^([A-Za-z0-9_~]+)\s*=\s*'([^']*)'$")]
    private static partial Regex ConditionRegex();

    [GeneratedRegex(@"^([A-Za-z0-9_~]+)\s+IN\s*\((.*)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex InRegex();

    [GeneratedRegex(@"'((?:[^']|'')*)'")]
    private static partial Regex QuotedRegex();
}
