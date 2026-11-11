using System.Globalization;
using System.Text.Json.Nodes;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>Porte de <c>gravar_md</c> do <c>cruzar.py</c>: o <c>resumo.md</c> a partir do resumo (<c>resumo.json</c>).</summary>
public static class ScanSummaryMarkdown
{
    public static string Build(JsonObject r)
    {
        var atc = r["base_atc"]!.AsObject();
        var scope = r["escopo"]!.AsObject();
        var read = r["lidos"]!.AsObject();
        var occ = r["ocorrencias"]!.AsObject();
        var mig = r["mignow"]!.AsObject();
        var csv = r["csv_remediacao"]!.AsObject();
        var L = new List<string>
        {
            $"# Varredura MIGNOW – {S(r, "sistema")} – {S(r, "varredura")}",
            "",
            $"Gerado por `cruzar.py` (skill atc-extracao-mignow) em {S(r, "gerado_em")}. Base ATC: `{S(atc, "arquivo")}` "
                + $"({S(atc, "findings")} findings). Regras versão {S(r, "regras_versao")}.",
            "",
            "## Totais",
            "",
            "| Item | Qtde |",
            "|---|---|",
            $"| Objetos no escopo Z/Y | {S(scope, "objetos")} |",
            $"| Objetos lidos | {S(read, "objetos")} |",
            $"| Fontes lidos | {S(read, "fontes")} |",
            $"| Linhas lidas | {S(read, "linhas")} |",
            $"| Ocorrências (refinada) | {S(occ, "total")} |",
            $"| Pendentes | {Get(occ, "pendente")} |",
            $"| Já remediadas (MIG-NOW) | {Get(occ, "ja_remediado")} |",
            $"| Informativas (sem contexto) | {Get(occ, "informativo")} |",
            $"| **MIGNOW (ATC não pegou)** | **{S(mig, "ocorrencias")}** em **{S(mig, "objetos")}** objetos |",
            $"| Linhas no CSV de remediação (P1) | {S(csv, "linhas")} |",
            $"| Erros de leitura de objeto | {S(read, "erros_objeto")} |",
            "",
            "## Cruzamento dos pendentes com o ATC",
            "",
            "| Classe | Qtde |",
            "|---|---|",
        };
        L.AddRange(r["cruzamento_pendentes"]!.AsObject().Select(kv => $"| {kv.Key} | {kv.Value} |"));
        L.Add("");
        if (r["comparacao_original_x_refinada"] is JsonObject c)
        {
            L.Add("## Original x refinada");
            L.Add("");
            L.Add($"Regras originais (linha a linha): {S(c, "original_linhas")} linhas · refinada: {S(c, "refinada_ocorrencias")} ocorrências, "
                + $"{S(c, "refinada_pendentes")} pendentes, {S(c, "refinada_csv")} no CSV.");
            L.Add("");
        }
        L.Add("## Por regra");
        L.Add("");
        L.Add("| Regra | Original (linhas) | Refinada | Pendente | Já remed. | Inform. | ATC pegou | Outra linha | MIGNOW c/ finding | MIGNOW s/ finding |");
        L.Add("|---|---|---|---|---|---|---|---|---|---|");
        foreach (var (rid, node) in r["por_regra"]!.AsObject())
        {
            var x = node!.AsObject();
            L.Add($"| {rid} | {S(x, "original_linhas")} | {S(x, "refinada_total")} | {Get(x, "pendente")} | {Get(x, "ja_remediado")} | "
                + $"{Get(x, "informativo")} | {Get(x, "ATC_PEGOU")} | {Get(x, "ATC_OUTRA_LINHA")} | "
                + $"{Get(x, "MIGNOW_FONTE_COM_FINDING")} | {Get(x, "MIGNOW_FONTE_SEM_FINDING")} |");
        }
        L.Add("");
        L.Add("## Cobertura do ATC por elemento (pendentes)");
        L.Add("");
        L.Add("| Elemento | Pendentes | ATC pegou | MIGNOW | Cobertura ATC |");
        L.Add("|---|---|---|---|---|");
        foreach (var (e, node) in r["cobertura_atc_por_elemento"]!.AsObject())
        {
            var x = node!.AsObject();
            L.Add($"| {e} | {S(x, "pendentes")} | {S(x, "atc_pegou")} | {S(x, "mignow")} | {x["cobertura_atc_pct"]!.GetValue<double>().ToString("F1", CultureInfo.InvariantCulture)}% |");
        }
        L.Add("");
        L.Add("## Top objetos MIGNOW");
        L.Add("");
        L.Add("| Objeto | Ocorrências |");
        L.Add("|---|---|");
        L.AddRange(r["top_objetos_mignow"]!.AsArray().Select(p => $"| {p![0]} | {p[1]} |"));
        L.Add("");
        L.Add("## Top pacotes MIGNOW");
        L.Add("");
        L.Add("| Pacote | Ocorrências |");
        L.Add("|---|---|");
        L.AddRange(r["top_pacotes_mignow"]!.AsArray().Select(p => $"| {p![0]} | {p[1]} |"));
        return string.Join("\n", L) + "\n";
    }

    private static string S(JsonObject o, string key) => o[key] is { } n ? n.ToString() : "None";

    private static string Get(JsonObject o, string key) => o[key] is { } n ? n.ToString() : "0";
}
