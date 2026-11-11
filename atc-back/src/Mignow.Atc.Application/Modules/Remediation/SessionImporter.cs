namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Porte de <c>cmd_importar</c> (atc.py): acha as colunas pelos nomes aceitos, filtra pelo escopo de prioridades, corrige o cabeçalho
/// deslocado do SATC (procura o par tipo conhecido + nome depois da coluna da nota) e classifica o finding (Ambiguous/Functional/Technical).
/// O número do item é a posição da linha no CSV (a partir de 1), mesmo para as linhas fora do escopo.
/// </summary>
public static class SessionImporter
{
    public const string ColFindingId = "Finding ID";
    public const string ColStatus = "Status Remediação";
    public const string AllPriorities = "todas";
    public const string ErrColumnsMissing = "Session.CsvColumnsMissing";
    public const string ErrPriorityColumnMissing = "Session.CsvPriorityColumnMissing";

    /// <summary>Tipos de objeto conhecidos (<c>OBJ_TYPES</c>: chaves de <c>ROBT_CAT</c> + extras).</summary>
    public static readonly IReadOnlySet<string> ObjTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "TABL", "VIEW", "DDLS", "STOB", "FUNC", "TRAN", "PROG", "CLAS", "INTF", "DTEL", "DOMA", "TTYP", "STRU",
        "FUGR", "SHLP", "ENHO", "MSAG", "TABU", "DEVC", "XSLT", "BADI", "ENHS", "METH", "FORM",
    };

    private static readonly string[] Classifications = ["Ambiguous", "Functional", "Technical"];

    /// <summary>"1,2" → escopo ordenado e sem repetição (como o <c>sorted({...})</c>); vazio/nulo = todas (nulo).</summary>
    public static IReadOnlyList<string>? ParseScope(string? priorities)
    {
        if (string.IsNullOrWhiteSpace(priorities) || priorities.Trim().Equals(AllPriorities, StringComparison.OrdinalIgnoreCase))
            return null;
        var scope = priorities.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0).Distinct().Order(StringComparer.Ordinal).ToList();
        return scope.Count == 0 ? null : scope;
    }

    public static ImportOutcome Import(CsvDocument doc, string? priorities)
    {
        var header = doc.Header;
        var ixObj = AtcCsv.Col(header, "Object name", "Objeto", "Obj. Name");
        var ixType = AtcCsv.Col(header, "Obj.", "Object Type", "Tipo");
        var ixRef = AtcCsv.Col(header, "Referenced Object", "Objeto referenciado");
        var ixRobt = AtcCsv.Col(header, "RObT", "Ref. Obj. Type");
        var ixCheck = AtcCsv.Col(header, "Check Title");
        var ixMsg = AtcCsv.Col(header, "Check Message");
        var ixNote = AtcCsv.Col(header, "Note", "Nota");
        var ixText = AtcCsv.Col(header, "Short Text");
        var ixPackage = AtcCsv.Col(header, "Package", "Pacote");
        var ixFinding = AtcCsv.Col(header, ColFindingId);
        var ixPrio = AtcCsv.Col(header, "Priority", "Prioridade");
        if (ixObj is null || ixType is null)
            return new ImportOutcome([], null, ErrColumnsMissing, header);
        var scope = ParseScope(priorities);
        if (scope is not null && ixPrio is null)
            return new ImportOutcome([], null, ErrPriorityColumnMissing, header);

        var rows = new List<ImportedRow>();
        Dictionary<string, int> byPriority = [];
        int outOfScope = 0, shifted = 0, ignored = 0;
        var n = 0;
        foreach (var r in doc.Rows)
        {
            n++;
            string G(int? ix) => ix is { } k && k < r.Count ? r[k].Trim() : string.Empty;
            var prio = G(ixPrio);
            var prioKey = prio.Length == 0 ? "?" : prio;
            byPriority[prioKey] = byPriority.GetValueOrDefault(prioKey) + 1;
            if (scope is not null && !scope.Contains(prio))
            {
                outOfScope++;
                continue;
            }
            var reference = G(ixRef);
            var robt = G(ixRobt);
            if (!ObjTypes.Contains(robt.ToUpperInvariant()))
            {
                // Exportação do ATC com o cabeçalho deslocado em relação aos dados: procura (tipo conhecido, nome) a partir da coluna da nota.
                var start = ixNote is { } nk ? nk + 1 : 0;
                for (var k = start; k < r.Count - 1; k++)
                {
                    if (ObjTypes.Contains(r[k].Trim().ToUpperInvariant()) && r[k + 1].Trim().Length > 0)
                    {
                        robt = r[k].Trim().ToUpperInvariant();
                        reference = r[k + 1].Trim();
                        shifted++;
                        break;
                    }
                }
            }
            var classification = r.Select(v => v.Trim()).FirstOrDefault(v => Classifications.Contains(v)) ?? string.Empty;
            var obj = G(ixObj);
            var type = G(ixType);
            if (obj.Length == 0 || type.Length == 0)
            {
                ignored++; // linha em branco/incompleta: o script cria item vazio; aqui não há o que remediar
                continue;
            }
            rows.Add(new ImportedRow(n, obj, type, reference, robt, G(ixCheck), G(ixMsg), G(ixNote), G(ixText), G(ixPackage), classification,
                G(ixFinding), prio, r));
        }
        var byType = rows.GroupBy(i => i.ObjectType).ToDictionary(g => g.Key, g => g.Count());
        var summary = new ImportSummary(rows.Count, byType, rows.Select(i => (i.ObjectType, i.ObjectName)).Distinct().Count(),
            scope is null ? AllPriorities : string.Join(',', scope), byPriority, outOfScope, shifted, ignored);
        return new ImportOutcome(rows, summary, null, header);
    }
}
