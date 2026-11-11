using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Porte de <c>cmd_conferir_r1</c> + <c>R1_CONSULTAS</c>/<c>sql_sap</c>: como o objeto sem código usa o referenciado, no dicionário (DD02L, DD03L,
/// DD40L, DD30L, DD01L, DD04L, DD26S; só SELECT pelo <see cref="ISapSql"/>). Referência só de chave/verificação a tabela ativa = falso positivo
/// (Isenção); estrutura inteira, referenciado inativo/ausente ou uso não localizado = volta a pendente para análise.
/// </summary>
public sealed class R1Checker(ISapSql sql)
{
    private const int MaxRows = 1000;

    private sealed record Query(string Sql, Func<IReadOnlyDictionary<string, string>, string, string> Nature, Func<IReadOnlyDictionary<string, string>, string, string> Description);

    private static readonly Query Table = new(
        "SELECT fieldname, checktable, reftable, precfield, comptype FROM dd03l WHERE tabname = '{obj}' "
        + "AND as4local = 'A' AND ( checktable = '{ref}' OR reftable = '{ref}' OR precfield = '{ref}' )",
        (r, _) => Get(r, "FIELDNAME").StartsWith('.') ? "estrutura" : "chave",
        (r, reference) => $"campo {Get(r, "FIELDNAME")}: "
                          + (Get(r, "FIELDNAME").StartsWith('.') ? "include" : Get(r, "CHECKTABLE") == reference ? "check table" : "tabela de referência"));

    private static readonly Dictionary<string, Query> Queries = new(StringComparer.Ordinal)
    {
        ["TABL"] = Table,
        ["STRU"] = Table,
        ["TTYP"] = new("SELECT typename, rowtype FROM dd40l WHERE typename = '{obj}' AND rowtype = '{ref}'",
            (_, _) => "estrutura", (_, _) => "tipo de linha da categoria de tabela"),
        ["SHLP"] = new("SELECT shlpname, selmethod, texttab FROM dd30l WHERE shlpname = '{obj}' AND ( selmethod = '{ref}' OR texttab = '{ref}' )",
            (_, _) => "chave", (r, reference) => Get(r, "SELMETHOD") == reference ? "método de seleção" : "tabela de textos"),
        ["DOMA"] = new("SELECT domname, entitytab FROM dd01l WHERE domname = '{obj}' AND entitytab = '{ref}'",
            (_, _) => "chave", (_, _) => "tabela de valores do domínio"),
        ["DTEL"] = new("SELECT rollname, domname, reftype FROM dd04l WHERE rollname = '{obj}' AND domname = '{ref}'",
            (_, _) => "chave", (_, _) => "domínio do elemento de dados"),
        ["VIEW"] = new("SELECT viewname, tabname FROM dd26s WHERE viewname = '{obj}' AND tabname = '{ref}'",
            (_, _) => "estrutura", (_, _) => "tabela base da view"),
    };

    /// <summary>Itens que a R1 marcou (<c>obs</c> começando por "Triagem R1"), ou os escolhidos.</summary>
    public static IReadOnlyList<RemediationItem> Targets(IEnumerable<RemediationItem> items, IReadOnlyCollection<int>? ns) =>
        [.. items.Where(i => ns is { Count: > 0 } ? ns.Contains(i.N) : i.Obs.StartsWith("Triagem R1", StringComparison.Ordinal))];

    public async Task<IReadOnlyList<R1Check>> CheckAsync(SapConnection connection, IReadOnlyList<RemediationItem> items, CancellationToken ct)
    {
        var active = new Dictionary<string, bool>(StringComparer.Ordinal);
        var result = new List<R1Check>();
        foreach (var it in items)
        {
            var obj = it.ObjectName.ToUpperInvariant();
            var reference = (it.RefObject ?? string.Empty).ToUpperInvariant();
            var type = it.ObjectType.ToUpperInvariant();
            var note = it.Note ?? string.Empty;
            var usages = new List<(string Nature, string Description)>();
            try
            {
                if (!active.ContainsKey(reference))
                    active[reference] = (await Run(connection, $"SELECT tabname FROM dd02l WHERE tabname = '{reference}' AND as4local = 'A'", ct)).Count > 0;
                if (Queries.TryGetValue(type, out var q))
                    usages = [.. (await Run(connection, q.Sql.Replace("{obj}", obj, StringComparison.Ordinal).Replace("{ref}", reference, StringComparison.Ordinal), ct))
                        .Select(r => (q.Nature(r, reference), q.Description(r, reference)))];
            }
            catch (InvalidOperationException e)
            {
                result.Add(new R1Check(it.N, obj, type, reference, note, null, R1Check.Error, e.Message));
                continue;
            }
            var evidence = string.Join("; ", usages.Select(u => u.Description).Distinct().Order(StringComparer.Ordinal));
            string proposal, reason;
            if (!active[reference])
            {
                proposal = R1Check.Pending;
                reason = $"{reference} não está ativo no DD02L: objeto referenciado removido/alterado – analisar";
            }
            else if (usages.Count == 0)
            {
                proposal = R1Check.Pending;
                reason = $"uso de {reference} em {type} {obj} não localizado no dicionário – conferir na SE11";
            }
            else if (usages.All(u => u.Nature == "chave"))
            {
                proposal = R1Check.Exemption;
                reason = $"Conferência R1: {type} {obj} referencia {reference} só como chave/verificação ({evidence}); {reference} segue ativo no S/4 e não há código no objeto – falso positivo do check (nota {note})";
            }
            else
            {
                proposal = R1Check.Pending;
                reason = $"{type} {obj} usa {reference} como estrutura ({evidence}): impacto depende dos programas que usam o tipo – analisar";
            }
            result.Add(new R1Check(it.N, obj, type, reference, note, evidence, proposal, reason));
        }
        return result;
    }

    /// <summary>Grava o que o script grava (<c>marcar</c>): Isenção com o motivo, ou volta a pendente com "Conferência R1: …".</summary>
    public static void Apply(RemediationItem item, R1Check check, DateTimeOffset at)
    {
        if (check.Proposal == R1Check.Exemption)
            item.MarkStatus(RemediationStatus.Exemption, check.Reason, at);
        else if (check.Proposal == R1Check.Pending)
            item.MarkStatus(RemediationStatus.Pending, "Conferência R1: " + check.Reason, at);
    }

    private async Task<List<IReadOnlyDictionary<string, string>>> Run(SapConnection connection, string statement, CancellationToken ct)
    {
        var r = await sql.SelectAsync(connection, statement, MaxRows, ct);
        if (r.IsFailure)
            throw new InvalidOperationException(r.Error.Message);
        return [.. r.Value.Rows];
    }

    private static string Get(IReadOnlyDictionary<string, string> row, string column) =>
        row.TryGetValue(column, out var v) ? v : row.FirstOrDefault(kv => kv.Key.Equals(column, StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty;
}
