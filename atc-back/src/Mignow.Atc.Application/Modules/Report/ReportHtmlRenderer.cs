using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static Mignow.Atc.Application.Modules.Report.ReportFormat;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// Porte do <c>gerar.py</c> (skill atc-resultado): renderiza o relatório final do cliente (identidade MIGNOW) a partir do
/// <see cref="ReportData"/> e do modelo <c>modelo/relatorio.html</c>. Duas visões: <c>findings</c> (unidade = finding) e
/// <c>objetos</c> (unidade = objeto; resolvido quando todos os findings dele estão). Pura: o modelo entra como texto.
/// </summary>
public static class ReportHtmlRenderer
{
    public const string ViewFindings = "findings";
    public const string ViewObjects = "objetos";

    private static readonly Dictionary<string, string> Names = new()
    {
        ["PROG"] = "programas", ["FUGR"] = "grupos de funções", ["CLAS"] = "classes", ["SSFO"] = "Smart Forms", ["TABL"] = "tabelas",
        ["SHLP"] = "ajudas de pesquisa", ["ENHO"] = "enhancements", ["TTYP"] = "tipos de tabela", ["AQSG"] = "InfoSets",
        ["AQQU"] = "queries", ["DOMA"] = "domínios", ["LDBA"] = "bancos de dados lógicos", ["FUGS"] = "extensões de grupo de funções",
        ["FUGX"] = "extensões de grupo de funções", ["INTF"] = "interfaces", ["DTEL"] = "elementos de dados", ["VIEW"] = "visões",
    };
    private static readonly Dictionary<string, string> Prio = new() { ["P1"] = "prioridade 1", ["P2"] = "prioridade 2", ["P3"] = "prioridade 3" };
    private static readonly JsonSerializerOptions JsOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static string Name(string t) => Names.GetValueOrDefault(t, t);

    public static bool IsView(string? v) => v is ViewFindings or ViewObjects;

    public static RenderedReport Render(ReportData d, string view, string template)
    {
        var ob = Objects(d.Items);
        var burndown = d.Evolution.Points
            .Where(p => view == ViewFindings || p.PendingObjects is not null)
            .Select(p => new Dictionary<string, object?>
            {
                ["t"] = p.T, ["rot"] = p.Label, ["pend"] = view == ViewFindings ? p.Pending : p.PendingObjects, ["ciclo"] = p.Cycle,
                ["tot"] = view == ViewFindings ? p.Total : p.TotalObjects,
            }).ToList();
        var js = new Dictionary<string, object?>
        {
            ["visao"] = view, ["unidade"] = view == ViewFindings ? "findings" : "objetos",
            ["total"] = view == ViewFindings ? d.Items.Count : d.TotalObjects, ["burndown"] = burndown, ["linhas"] = Rows(d, view, ob),
        };
        var body = string.Join("\n", Hero(d, view, ob), "<main><div class=\"wrap\">", Start(d, view, ob), Burndown(d, view), Result(d, view, ob), Comparison(d),
            Application(d, view, ob), Exemption(d, view, ob), Knowledge(d), Next(d), List(d, view), Process(d), Costs(d, view), "</div></main>",
            $"<footer><div class=\"wrap\"><span><span class=\"brand\">MIGNOW</span> · Mignow · Adaptação de código custom S/4HANA</span><span>Gerado em {E(DateTimeOffset.Parse(d.GeneratedAt, CultureInfo.InvariantCulture).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture))} pela skill atc-resultado a partir dos registros das skills de extração, remediação e isenção</span></div></footer>");
        var title = $"Remediação ATC {d.Customer.Name}" + (view == ViewObjects ? " · objetos" : "");
        var fragment = template.Replace("{{TITULO}}", E(title), StringComparison.Ordinal).Replace("<!--CORPO-->", body, StringComparison.Ordinal)
            .Replace("/*DADOS*/null", JsonSerializer.Serialize(js, JsOptions).Replace("</", "<\\/", StringComparison.Ordinal), StringComparison.Ordinal);
        var cut = fragment.IndexOf("</style>", StringComparison.Ordinal);
        var head = cut < 0 ? fragment : fragment[..cut];
        var rest = cut < 0 ? "" : fragment[(cut + "</style>".Length)..];
        var doc = "<!doctype html>\n<html lang=\"pt-BR\"><head><meta charset=\"utf-8\">"
                  + "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, viewport-fit=cover\">\n" + head
                  + "\nbody { margin: 0; } [hidden] { display: none !important; } img { max-width: 100%; }\n"
                  + ":root { padding-top: env(safe-area-inset-top, 0px); padding-bottom: env(safe-area-inset-bottom, 0px); }\n"
                  + "</style></head><body>" + rest + "</body></html>\n";
        return new RenderedReport(view, doc, fragment, view == ViewFindings ? d.Items.Count : d.TotalObjects, burndown.Count);
    }

    public static IReadOnlyList<ReportObject> Objects(IReadOnlyList<ReportData.ItemRow> items)
    {
        var res = new List<ReportObject>();
        foreach (var g in items.GroupBy(i => (i.Type, i.Obj)))
        {
            var its = g.ToList();
            int C(string k) => its.Count(x => x.Solution == k);
            var corr = C("KB") + C("Análise");
            var cat = C("Pendente") > 0 ? "Pendente" : C("Isenção") == its.Count ? "Isenção" : C("Isenção") > 0 ? "Corrigido + isenção" : corr > 0 ? "Corrigido" : "Cancelado";
            res.Add(new ReportObject(g.Key.Obj, g.Key.Type, its.Count, C("KB"), C("Análise"), C("Isenção"), C("Pendente"), cat,
                [.. its.Where(x => x.System == "ATIVADO").Select(x => x.Request).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
                its.Any(x => x.System == "PEDIDO"), string.Join("; ", its.Select(x => x.Check).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))));
        }
        return res;
    }

    private static Dictionary<string, int> Count(IEnumerable<string> xs) => xs.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

    private static int Of(Dictionary<string, int> c, string k) => c.GetValueOrDefault(k);

    private static string Hero(ReportData d, string v, IReadOnlyList<ReportObject> ob)
    {
        var c = d.Customer;
        var it = d.Items;
        var sc = Count(it.Select(x => x.Solution));
        var corr = Of(sc, "KB") + Of(sc, "Análise");
        var isen = d.Exemption;
        var esc = string.Join(" e ", d.Scope.Select(p => Prio[p]));
        if (esc.Length == 0)
            esc = "do lote importado";
        var p = d.Evolution.Points;
        long? hours = p.Count > 1 ? (long)Math.Round((p[^1].T - p[0].T) / 3.6e6, MidpointRounding.ToEven) : null;
        var when = "";
        if (hours is > 0)
            when = hours > 48 ? $" em {Math.Round(hours.Value / 24.0, MidpointRounding.ToEven):0} dias de calendário" : $" em {hours} hora{(hours > 1 ? "s" : "")} de calendário";
        var cy = d.Cycles;
        if (cy.Count > 1)
            when += " e " + N(cy.Count) + " ciclos (" + string.Join(", ", cy.Select((x, i) =>
                $"{x.Findings} {(i == 0 ? "na execução" : "na reexecução")} do ATC de {E(Left(x.Extraction?.ExecutedAt ?? "", 5))}")) + ")";
        var back = cy.Skip(1).Sum(x => x.AlreadyTreatedBefore.Count);
        var backText = back > 0 ? $" Na reexecução, {N(back)} findings já tratados no ciclo anterior voltaram a aparecer e foram tratados de novo." : "";
        var cm = d.ManualComparison.Total;
        var oc = Count(ob.Select(o => o.Category));
        var oat = ob.Count(o => o.Requests.Count > 0);
        var pend = Of(sc, "Pendente");
        var rest = pend > 0 ? $" Restam <strong>{pend} pendentes</strong>." : "";
        var liq = d.Costs.Total.NetSeconds;
        var manualNote = cm.ManualSeconds != 0 ? $" (manual estimado: {Hm(cm.ManualSeconds)})" : "";
        string lede;
        List<(string Cls, string A, string B)> kpis;
        if (v == ReportHtmlRenderer.ViewFindings)
        {
            lede = $"Os <strong>{N(it.Count)} findings de {esc}</strong> do ATC foram tratados{when}: <strong>{N(corr)} corrigidos</strong> no código, "
                   + $"<strong>{N(d.Application.ActivatedSources)} fontes ativados</strong> no {E(c.Sid)} e <strong>{N(Of(sc, "Isenção"))} encaminhados para isenção</strong>"
                   + (isen is not null ? $", dos quais {N(isen.Requested)} já estão com pedido aberto no SAP." : ".") + backText + rest;
            kpis =
            [
                ("acc", N(it.Count), $"findings {E(string.Join(" + ", d.Scope))} tratados, {pend} pendentes"),
                ("", N(corr), $"corrigidos ({Of(sc, "KB")} pela KB, {Of(sc, "Análise")} por análise)"),
                ("ok", N(d.Application.ActivatedSources), $"fontes ativados no {E(c.Sid)}"),
                ("", N(isen?.Requested ?? 0), "pedidos de isenção abertos"),
                ("", Hm(liq), "tempo líquido total" + manualNote),
            ];
        }
        else
        {
            lede = $"Os <strong>{N(ob.Count)} objetos</strong> com findings de {esc} foram tratados{when}: "
                   + $"<strong>{N(Of(oc, "Corrigido") + Of(oc, "Corrigido + isenção"))} receberam correção</strong>, "
                   + $"<strong>{N(oat)} tiveram fonte ativado</strong> no {E(c.Sid)} e <strong>{N(Of(oc, "Isenção"))} ficaram só com isenção</strong>."
                   + (Of(oc, "Pendente") > 0 ? $" Restam <strong>{Of(oc, "Pendente")} objetos pendentes</strong>." : "");
            kpis =
            [
                ("acc", N(ob.Count), $"objetos tratados, {Of(oc, "Pendente")} pendentes"),
                ("", N(Of(oc, "Corrigido") + Of(oc, "Corrigido + isenção")), "objetos com correção no código"),
                ("ok", N(oat), $"objetos com fonte ativado no {E(c.Sid)}"),
                ("", N(Of(oc, "Isenção")), "objetos só com isenção"),
                ("", Hm(liq), "tempo líquido total" + manualNote),
            ];
        }
        var (p0, p1) = p.Count > 0 ? (p[0].Label.Split(' ')[0], p[^1].Label.Split(' ')[0]) : ("", "");
        var year = d.GeneratedAt[..4];
        var meta = new (string K, string? V)[]
        {
            ("Sistema", $"{c.Sid}/{c.Client}"), ("Release", c.Release), ("Papel", c.Role), ("Série ATC", d.Extraction?.Series),
            ("Período", p.Count > 0 ? $"{p0} a {p1}.{year}" : null), ("Consultor", c.Author),
        };
        var switchLinks = v == ReportHtmlRenderer.ViewFindings
            ? $"<span>Por findings</span><a href=\"relatorio_final_{E(c.Name)}_objetos.html\">Por objetos</a>"
            : $"<a href=\"relatorio_final_{E(c.Name)}_findings.html\">Por findings</a><span>Por objetos</span>";
        return $"""
            <header class="hero"><div class="wrap">
              <div class="eyebrow">MIGNOW · Adaptação de código custom S/4HANA · Relatório final do ciclo</div>
              <div class="visao" aria-label="Versão do relatório">{switchLinks}</div>
              <h1>Remediação ATC <span>{E(c.Name)}</span></h1>
              <p class="lede">{lede}</p>
              <div class="meta">{string.Concat(meta.Where(m => !string.IsNullOrEmpty(m.V)).Select(m => $"<span>{m.K} <b>{E(m.V)}</b></span>"))}</div>
              <div class="kpis" role="list">{string.Concat(kpis.Select(k => $"<div class=\"kpi {k.Cls}\" role=\"listitem\"><span class=\"n\">{k.A}</span><span class=\"l\">{k.B}</span></div>"))}</div>
            </div></header>
            """;
    }

    private static string Left(string s, int n) => s.Length <= n ? s : s[..n];

    private static string Start(ReportData d, string v, IReadOnlyList<ReportObject> ob)
    {
        var x = d.Extraction;
        if (x is null)
            return "";
        var it = d.Items;
        var ct = v == ReportHtmlRenderer.ViewFindings ? Count(it.Select(i => i.Type)) : Count(ob.Select(o => o.Type));
        var top = ct.OrderByDescending(kv => kv.Value).Take(5).ToList();
        var other = ct.Values.Sum() - top.Sum(kv => kv.Value);
        var types = string.Join(", ", top.Select(kv => $"{kv.Value} {Name(kv.Key)}")) + (other > 0 ? $" e {other} de outros tipos" : "");
        var unit = v == ReportHtmlRenderer.ViewFindings ? "findings" : "objetos";
        var cards = new StringBuilder();
        foreach (var (p, cls) in new[] { ("p1", "p-bad"), ("p2", "p-warn"), ("p3", "p-mut") })
        {
            var inScope = d.Scope.Contains(p.ToUpperInvariant());
            var sub = inScope ? (v == ReportHtmlRenderer.ViewFindings ? $"tratados neste ciclo · {d.TotalObjects} objetos" : $"tratados neste ciclo · {it.Count} findings") : "fora deste ciclo";
            long? val = v == ReportHtmlRenderer.ViewFindings || !inScope ? (p == "p1" ? x.P1 : p == "p2" ? x.P2 : x.P3) : d.TotalObjects;
            cards.Append($"<div class=\"panel\">{Pill("Prioridade " + p[1], cls)}<span class=\"big\">{N(val)}</span><span class=\"muted\">{sub}{(v == ViewObjects && !inScope ? " (findings)" : "")}</span></div>");
        }
        var cy = d.Cycles;
        var cycleHtml = "";
        if (cy.Count > 1)
        {
            var ls = string.Concat(cy.Select(c =>
                $"<tr><td>Ciclo {c.Cycle}</td><td class=\"mono\">{E(c.Extraction?.ExecutedAt ?? "–")}</td>"
                + $"<td class=\"num\">{N(c.Extraction?.Total)}</td><td class=\"num\">{N(c.Extraction?.P1)}</td><td class=\"num\">{N(c.Extraction?.P2)}</td><td class=\"num\">{N(c.Extraction?.P3)}</td>"
                + $"<td class=\"num\">{N(c.Findings)} ({E(c.Scope.Count > 0 ? string.Join(" + ", c.Scope) : "lote")})</td>"
                + $"<td class=\"num\">{(c.Cycle > 1 ? N(c.AlreadyTreatedBefore.Count) : "–")}</td></tr>"));
            var ex0 = cy[0].Extraction;
            var ex1 = cy[^1].Extraction;
            var drop = new List<string>();
            foreach (var p in d.Scope)
            {
                int? a = ex0 is null ? null : (p == "P1" ? ex0.P1 : p == "P2" ? ex0.P2 : ex0.P3);
                int? b = ex1 is null ? null : (p == "P1" ? ex1.P1 : p == "P2" ? ex1.P2 : ex1.P3);
                if (a is > 0 && b is not null)
                {
                    var pctv = Math.Round((b.Value - a.Value) / (double)a.Value * 100, MidpointRounding.ToEven);
                    drop.Add($"{p}: {N(a)} → {N(b)} ({(pctv >= 0 ? "+" : "-")}{Math.Abs(pctv):0}%)");
                }
            }
            cycleHtml = "<p>O ATC foi executado de novo depois da remediação. "
                        + (drop.Count > 0 ? "Findings sem baseline entre a primeira e a última execução: " + E(string.Join("; ", drop)) + "." : "")
                        + " Cada reexecução abriu um ciclo com os findings que continuaram no escopo.</p>"
                        + $"<div class=\"tbl\"><table><thead><tr><th>Ciclo</th><th>Execução do ATC</th><th class=\"num\">Total</th><th class=\"num\">P1</th><th class=\"num\">P2</th><th class=\"num\">P3</th><th class=\"num\">Tratados no ciclo</th><th class=\"num\">Já tratados antes</th></tr></thead><tbody>{ls}</tbody></table></div>"
                        + "<p class=\"muted\">P1, P2 e P3 sem baseline. \"Já tratados antes\": mesmo objeto, check, referência e mensagem de um finding do ciclo anterior.</p>";
        }
        var scopeText = d.Scope.Count > 0 ? "os de " + string.Join(" e ", d.Scope.Select(p => Prio[p])) : "o lote importado";
        return $"""
            <section aria-labelledby="s-ini"><h2 id="s-ini"><small>Ponto de partida</small>O que o ATC encontrou</h2>
            <p>A {(cy.Count > 1 ? "primeira " : "")}execução extraída da série <code>{E(x.Series)}</code> ({E(x.ExecutedAt)}) registrou <b>{N(x.Total)} findings</b>, {N(x.InBaseline)} deles em baseline. Este ciclo tratou {scopeText}.</p>
            <div class="grid3">{cards}</div>
            <p class="muted">Tipos de objeto nos {unit} {(cy.Count > 1 ? "dos ciclos" : "do ciclo")}: {E(types)}.</p>{cycleHtml}</section>
            """;
    }

    private static string Burndown(ReportData d, string v)
    {
        var pts = d.Evolution.Points;
        if (pts.Count == 0)
            return "";
        int K(ReportData.EvolutionPoint p) => v == ViewFindings ? p.Pending : p.PendingObjects ?? 0;
        int KT(ReportData.EvolutionPoint p) => v == ViewFindings ? p.Total : p.TotalObjects;
        var unit = v == ViewFindings ? "findings" : "objetos";
        var multi = pts.Select(p => p.Cycle).Distinct().Count() > 1;
        var tot = KT(pts[0]) != 0 ? KT(pts[0]) : (v == ViewFindings ? d.Items.Count : d.TotalObjects);
        var rec = pts.Any(p => p.Reconstructed == true);
        var note = "Cada ponto é o fechamento de um lote de remediação."
                   + (rec && v == ViewFindings ? " Pontos reconstruídos dos relatórios de lote." : "")
                   + (rec && v == ViewObjects ? " Objetos pendentes por ponto reconstruídos pela ordem de gravação dos itens, na quantidade de findings registrada em cada lote." : "");
        if (multi)
            note += $" Cada reexecução do ATC abre um novo ciclo (linha vertical): o pendente sobe para os {unit} do novo ciclo.";
        if (pts.Any(p => p.SessionPoint == true))
            note += " O ponto “sessão” é a situação gravada na sessão depois da última conversa registrada, na data da última alteração dos itens.";
        var lines = new StringBuilder();
        for (var i = 0; i < pts.Count; i++)
        {
            var p = pts[i];
            var isNew = i > 0 && p.Cycle != pts[i - 1].Cycle;
            var prev = i > 0 ? K(pts[i - 1]) : K(p);
            var res = isNew ? $"novo ciclo: {K(p)}" : i > 0 ? (prev - K(p)).ToString(CultureInfo.InvariantCulture) : "–";
            var tc = KT(p) != 0 ? KT(p) : tot;
            lines.Append($"<tr>{(multi ? "<td>" + p.Cycle + "</td>" : "")}<td class=\"mono\">{E(p.Label)}</td><td class=\"num\">{K(p)}</td><td class=\"num\">{res}</td><td class=\"num\">{Math.Round((tc - K(p)) / (double)tc * 100, MidpointRounding.ToEven):0}%</td></tr>");
        }
        var finals = pts.Where((p, i) => i == pts.Count - 1 || pts[i + 1].Cycle != p.Cycle).ToList();
        var title = multi ? "Burndown por ciclo: " + string.Join(" e ", finals.Select(p => $"{N(KT(p))} → {K(p)}")) + $" {unit} pendentes"
            : $"Burndown: de {N(tot)} {unit} pendentes a {K(pts[^1])}";
        return $"""
            <section aria-labelledby="s-evo"><h2 id="s-evo"><small>Evolução</small>{title}</h2>
            <p>{note} A linha tracejada é o ritmo constante entre o início e o fim {(multi ? "de cada ciclo" : "do ciclo")}; abaixo dela, o trabalho andou à frente desse ritmo.</p>
            <div class="chart" id="chart">
              <div class="legend" aria-hidden="true"><span><i class="sw" style="--c:var(--accent)"></i>{Upper1(unit)} pendentes</span><span><i class="sw dash" style="--c:var(--muted)"></i>Ritmo ideal</span></div>
              <svg id="bd" viewBox="0 0 960 360" role="img" aria-label="Burndown de {unit}: de {tot} pendentes a {K(pts[^1])}"></svg>
              <div class="tip" id="tip" hidden></div>
            </div>
            <details><summary>Ver os números de cada lote</summary><div class="tbl"><table><thead><tr>{(multi ? "<th>Ciclo</th>" : "")}<th>Fechamento</th><th class="num">Pendentes</th><th class="num">Resolvidos no lote</th><th class="num">Concluído</th></tr></thead><tbody>{lines}</tbody></table></div></details>
            </section>
            """;
    }

    private static string Result(ReportData d, string v, IReadOnlyList<ReportObject> ob)
    {
        var it = d.Items;
        List<(string K, string Color)> cats;
        Dictionary<string, int> c;
        int tot;
        var rows = new Dictionary<string, Dictionary<string, int>>();
        var order = new List<string>();
        string first;
        (string, string)[] expl;
        void Add(string row, string cat)
        {
            if (!rows.TryGetValue(row, out var m))
            {
                rows[row] = m = [];
                order.Add(row);
            }
            m[cat] = m.GetValueOrDefault(cat) + 1;
        }
        if (v == ViewFindings)
        {
            cats = [("KB", "--s-kb"), ("Análise", "--s-ia"), ("Isenção", "--s-is"), ("Cancelado", "--s-pend"), ("Pendente", "--s-pend")];
            c = Count(it.Select(x => x.Solution));
            tot = it.Count;
            foreach (var x in it.OrderBy(x => x.Check, StringComparer.Ordinal))
                Add(x.Check, x.Solution);
            first = "Check ATC";
            expl =
            [
                ("Corrigido pela KB", "Solução já aplicada em outro projeto e reaproveitada da base de conhecimento MIGNOW, conferida no código do cliente."),
                ("Corrigido por análise", "Sem solução pronta na base: correção desenhada a partir do código, do dicionário e das notas SAP, e validada pelo consultor."),
                ("Isenção", "Código correto que o check continua marcando, objeto sem uso ou fora do escopo técnico. Segue para o fluxo de isenção do ATC."),
            ];
        }
        else
        {
            cats = [("Corrigido", "--s-ia"), ("Corrigido + isenção", "--s-kb"), ("Isenção", "--s-is"), ("Cancelado", "--s-pend"), ("Pendente", "--s-pend")];
            c = Count(ob.Select(o => o.Category));
            tot = ob.Count;
            foreach (var o in ob)
                Add($"{Upper1(Name(o.Type))} ({o.Type})", o.Category);
            first = "Tipo de objeto";
            expl =
            [
                ("Corrigido", "Todos os findings do objeto foram corrigidos no código (pela KB ou por análise)."),
                ("Corrigido + isenção", "Parte dos findings corrigida e parte encaminhada para isenção."),
                ("Isenção", "Todos os findings do objeto seguem para isenção: código correto, sem uso ou fora do escopo técnico."),
            ];
        }
        var cols = cats.Where(k => Of(c, k.K) > 0).Select(k => k.K).ToList();
        var sorted = order.Select((r, i) => (Row: r, I: i)).OrderByDescending(x => rows[x.Row].Values.Sum()).ThenBy(x => x.I).Select(x => x.Row).ToList();
        var bar = string.Concat(cats.Where(k => Of(c, k.K) > 0).Select(k => $"<i style=\"width:{Pct(Of(c, k.K), tot)};background:var({k.Color})\" title=\"{E(k.K)}: {Of(c, k.K)}\"></i>"));
        var legend = string.Concat(cats.Where(k => Of(c, k.K) > 0).Select(k => $"<span><i class=\"sq\" style=\"--c:var({k.Color})\"></i>{E(k.K)} ({Of(c, k.K)})</span>"));
        var body = string.Concat(sorted.Select(r => $"<tr><td>{E(r)}</td>" + string.Concat(cols.Select(k => $"<td class=\"num\">{rows[r].GetValueOrDefault(k)}</td>")) + $"<td class=\"num\">{rows[r].Values.Sum()}</td></tr>"));
        var foot = "<tr><td>Total</td>" + string.Concat(cols.Select(k => $"<td class=\"num\">{Of(c, k)}</td>")) + $"<td class=\"num\">{tot}</td></tr>";
        var aria = E(string.Join(", ", cats.Where(k => Of(c, k.K) > 0).Select(k => $"{k.K} {Of(c, k.K)}")));
        return $"""
            <section aria-labelledby="s-res"><h2 id="s-res"><small>Resultado</small>{(v == ViewFindings ? "Como cada finding foi resolvido" : "Como cada objeto foi resolvido")}</h2>
            <div class="stack" role="img" aria-label="{aria}">{bar}</div>
            <div class="legend">{legend}</div>
            <div class="tbl"><table><thead><tr><th>{first}</th>{string.Concat(cols.Select(k => $"<th class=\"num\">{E(k)}</th>"))}<th class="num">Total</th></tr></thead><tbody>{body}</tbody><tfoot>{foot}</tfoot></table></div>
            <div class="grid3">{string.Concat(expl.Select(x => $"<div class=\"panel\"><h3>{E(x.Item1)}</h3><p class=\"muted\">{E(x.Item2)}</p></div>"))}</div></section>
            """;
    }

    private static string Comparison(ReportData d)
    {
        var x = d.ManualComparison;
        if (x.Total.ObjectsTreated == 0)
            return "";
        var pm = x.Premises;
        var t = x.Total;
        var C = x.Cycles;
        var multi = C.Count > 1;
        var title = t.AiSeconds != 0 ? $"A IA fez em {Hm(t.AiSeconds)} o que levaria {Days(t.ManualDays)} de um ABAP" : $"Estimativa manual: {Days(t.ManualDays)} de um ABAP";
        string Width(long s) => (Math.Max(s / (double)Math.Max(t.ManualSeconds, t.AiSeconds) * 100, 0.6)).ToString("0.0", CultureInfo.InvariantCulture) + "%";
        var aiDays = t.AiSeconds / 3600.0 / pm.HoursPerDay;
        var bars = new (string R, long S, string Color, string V)[]
        {
            ("Manual (estimado)", t.ManualSeconds, "var(--muted)", $"{Hm(t.ManualSeconds)} · {Days(t.ManualDays)}"),
            ("Com IA (medido)", t.AiSeconds, "var(--accent)", Hm(t.AiSeconds) + (aiDays >= 0.1 ? $" · {Days(aiDays)}" : "")),
        };
        var chart = string.Concat(bars.Where(b => b.S != 0).Select(b =>
            $"<div class=\"cmp-l\">{E(b.R)}</div><div class=\"cmp-t\"><i style=\"width:{Width(b.S)};background:{b.Color}\" title=\"{E(b.R)}: {E(b.V)}\"></i></div><div class=\"cmp-v\">{E(b.V)}</div>"));
        var lines = string.Concat(C.Select(c =>
            $"<tr>{(multi ? "<td>Ciclo " + c.Cycle + "</td>" : "")}<td class=\"num\">{N(c.ObjectsTreated)}</td><td class=\"num\">{Hm(c.ManualSeconds)}</td><td class=\"num\">{Hm(c.AiSeconds)}</td><td class=\"num\">{Hm(c.AiRemediationSeconds)}</td>"
            + $"<td class=\"num\">{Hm(c.ManualSeconds - c.AiSeconds)}</td><td class=\"num\">{Factor(c.ManualSeconds, c.AiSeconds)}</td></tr>"));
        var foot = multi
            ? $"<tfoot><tr><td>Total</td><td class=\"num\">{N(t.ObjectsTreated)}</td><td class=\"num\">{Hm(t.ManualSeconds)}</td><td class=\"num\">{Hm(t.AiSeconds)}</td><td class=\"num\">{Hm(t.AiRemediationSeconds)}</td><td class=\"num\">{Hm(t.GainSeconds)}</td><td class=\"num\">{Factor(t.ManualSeconds, t.AiSeconds)}</td></tr></tfoot>"
            : "";
        var pend = t.ObjectsPending > 0 ? $" Os {t.ObjectsPending} objetos ainda pendentes ficam fora da conta manual, mas o tempo já gasto com eles está no tempo da IA." : "";
        var red = t.ReductionPct is null ? "–" : $"{t.ReductionPct.Value:0}%";
        var aria = E(string.Join("; ", bars.Where(b => b.S != 0).Select(b => $"{b.R}: {b.V}")));
        return $"""
            <section aria-labelledby="s-cmp"><h2 id="s-cmp"><small>Manual × IA</small>{title}</h2>
            <p>Os <b>{N(t.ObjectsTreated)} objetos tratados</b> levariam <b>{Hm(t.ManualSeconds)}</b> de trabalho manual ({Days(t.ManualDays)} de {pm.HoursPerDay} h), pela premissa de {pm.ObjectsPerDay} objetos por dia de um desenvolvedor ABAP. Com a IA, o ciclo inteiro (extração, remediação e isenção) consumiu <b>{Hm(t.AiSeconds)}</b> de tempo líquido medido.</p>
            <div class="cmp" role="img" aria-label="{aria}">{chart}</div>
            <div class="grid3"><div class="panel"><span class="big">{Hm(t.GainSeconds)}</span><span class="muted">economizadas em relação ao manual ({red} a menos)</span></div>
            <div class="panel"><span class="big">{Factor(t.ManualSeconds, t.AiSeconds)}</span><span class="muted">mais rápido que o manual, considerando todas as etapas</span></div>
            <div class="panel"><span class="big">{Factor(t.ManualSeconds, t.AiRemediationSeconds)}</span><span class="muted">mais rápido considerando só a remediação ({Hm(t.AiRemediationSeconds)})</span></div></div>
            <div class="tbl"><table><thead><tr>{(multi ? "<th>Ciclo</th>" : "")}<th class="num">Objetos tratados</th><th class="num">Manual estimado</th><th class="num">IA, todas as etapas</th><th class="num">IA, só remediação</th><th class="num">Tempo economizado</th><th class="num">Mais rápido</th></tr></thead><tbody>{lines}</tbody>{foot}</table></div>
            <p class="note"><b>Como foi calculado.</b> Manual = objetos tratados ÷ {pm.ObjectsPerDay} objetos por dia × {pm.HoursPerDay} h ({E(pm.Source)}): é estimativa, não medição. Objeto tratado = todos os findings do objeto no ciclo com decisão (corrigido, isenção ou cancelado), inclusive os que só seguiram para isenção, porque o ABAP também teria de analisá-los.{(multi ? " Um objeto que volta no ciclo seguinte conta de novo, como um retrabalho manual." : "")}{pend} IA = tempo líquido (sem pausas acima de 10 minutos) de todas as etapas da seção Custos e tempos, com as confirmações do consultor em cada etapa.</p></section>
            """;
    }

    private static string Application(ReportData d, string v, IReadOnlyList<ReportObject> ob)
    {
        var ap = d.Application;
        if (ap.ByRequest.Count == 0 && ap.Manual.Count == 0)
            return "";
        var rs = string.Concat(ap.ByRequest.Select(kv => $"<tr><td class=\"mono\">{E(kv.Key)}{(kv.Key == ap.SessionRequest ? " <span class=muted>(request da sessão)</span>" : "")}</td><td class=\"num\">{kv.Value}</td></tr>"));
        var man = new List<(string Reason, SortedSet<string> Objects)>();
        foreach (var m in ap.Manual)
        {
            var e = man.FirstOrDefault(x => x.Reason == m.Reason);
            if (e.Objects is null)
            {
                e = (m.Reason, new SortedSet<string>(StringComparer.Ordinal));
                man.Add(e);
            }
            e.Objects.Add(m.Object);
        }
        var mtxt = string.Concat(man.Select(k => $"<dt>{E(k.Reason)}</dt><dd>{ap.Manual.Count(m => m.Reason == k.Reason)} itens em {k.Objects.Count} objeto(s): {E(string.Join(", ", k.Objects))}</dd>"));
        var oat = ob.Count(o => o.Requests.Count > 0);
        var noRecord = d.Items.Count(x => x.Solution is "KB" or "Análise" && x.System.Length == 0);
        var highlights = string.Concat(d.Complements.Highlights.Select(x => $"<p class=\"muted\">Destaque: {E(x.Texto)}</p>"));
        var extra = v == ViewObjects ? $" São {oat} objetos com pelo menos um fonte ativado."
            : noRecord > 0 ? $" Cada ativação cobre todos os findings do mesmo fonte: {noRecord} findings corrigidos foram aplicados junto com outro item, indicado na análise de cada um." : "";
        return $"""
            <section aria-labelledby="s-sis"><h2 id="s-sis"><small>No sistema</small>Correções aplicadas no {E(d.Customer.Sid)}</h2>
            <p>As correções foram gravadas e ativadas pelo ADT, com backup do fonte antes de cada gravação e marcadores <code>S/4 MIGRATION MIG-NOW</code> no código alterado.{extra}</p>
            <div class="grid2"><div class="tbl"><table><thead><tr><th>Request</th><th class="num">Fontes ativados</th></tr></thead><tbody>{rs}</tbody><tfoot><tr><td>Total</td><td class="num">{ap.ByRequest.Values.Sum()}</td></tr></tfoot></table></div>
            <div class="panel"><h3>Correções que dependem do desenvolvedor ({ap.Manual.Count} itens)</h3><dl class="kv">{(mtxt.Length > 0 ? mtxt : "<dt>–</dt><dd>Nenhuma</dd>")}</dl></div></div>{highlights}</section>
            """;
    }

    private static string Exemption(ReportData d, string v, IReadOnlyList<ReportObject> ob)
    {
        var x = d.Exemption;
        if (x is null)
            return "";
        var causes = string.Join(", ", x.NoScopeCauses.Select(kv => $"{kv.Value} {kv.Key}"));
        var oisp = ob.Count(o => o.ExemptionRequested);
        var noScope = x.NoScope > 0 ? $"<dt>Sem escopo</dt><dd>{Pill(x.NoScope, "p-warn")} o SAP não permite isentar só o finding ({E(causes)})</dd>" : "";
        var blocked = x.Blocked > 0 || x.Errors > 0 ? $"<dt>Bloqueados / erros</dt><dd>{x.Blocked} / {x.Errors}</dd>" : "";
        var noScopeNote = x.NoScope > 0 ? $" Os {x.NoScope} sem escopo seguem outro caminho, indicado na análise de cada um." : "";
        return $"""
            <section aria-labelledby="s-isen"><h2 id="s-isen"><small>Isenção</small>Pedidos abertos no ATC</h2>
            <div class="grid2"><div class="panel"><h3>Resultado do envio{(x.Date is not null ? " (" + E(Left(x.Date, 10)) + ")" : "")}</h3><dl class="kv">
            <dt>Itens Isenção</dt><dd>{x.Findings} findings, todos ligados ao finding exato do ATC</dd>
            <dt>Pedidos aceitos</dt><dd>{Pill(x.Requested, "p-ok")} motivo {E(x.Reasons.Count > 0 ? string.Join(", ", x.Reasons) : "–")}{(v == ViewObjects ? $" · {oisp} objetos" : "")}</dd>
            {noScope}
            {blocked}
            <dt>Aprovador</dt><dd>{E(string.IsNullOrEmpty(x.Approver) ? "–" : x.Approver)}</dd></dl></div>
            <div class="panel"><h3>O que isso significa</h3><p class="muted">Os {x.Requested} findings deixam de aparecer no ATC quando o aprovador aceitar os pedidos.{noScopeNote}</p></div></div></section>
            """;
    }

    private static string Knowledge(ReportData d)
    {
        var k = d.Kb;
        var ev = string.Join(", ", d.Versions.Where(kv => kv.Value.Current is not null).Select(kv =>
            kv.Value.Initial is not null && kv.Value.Initial != kv.Value.Current ? $"{kv.Key} {kv.Value.Initial} → {kv.Value.Current}" : $"{kv.Key} {kv.Value.Current}"));
        var cards = k.ElementsWithCards is > 0
            ? $"<div class=\"panel\"><span class=\"big\">{k.ElementsWithCards}</span><span class=\"muted\">elementos da KB com cartões de solução criados ou atualizados</span></div>"
            : $"<div class=\"panel\"><span class=\"big\">{k.NewCards}</span><span class=\"muted\">cartões de solução novos na KB</span></div>";
        return $"""
            <section aria-labelledby="s-kb"><h2 id="s-kb"><small>Conhecimento</small>O que o projeto deixou na base MIGNOW</h2>
            <div class="grid3"><div class="panel"><span class="big">{k.NewFacts}</span><span class="muted">fatos técnicos novos registrados (comportamento de APIs, pré-requisitos, armadilhas), reaproveitáveis em outros clientes</span></div>
            {cards}<div class="panel"><span class="big">{k.BatchReports}</span><span class="muted">relatórios de lote com decisões, provas e pendências de cada item</span></div></div>
            {(ev.Length > 0 ? $"<p class=\"muted\">Skills MIGNOW usadas no ciclo: {E(ev)}.</p>" : "")}</section>
            """;
    }

    private static string Next(ReportData d)
    {
        var p = new List<(string Title, string Text)>();
        var x = d.Exemption;
        var sc = Count(d.Items.Select(i => i.Solution));
        if (x is not null && x.Requested > 0)
            p.Add(("Aprovar as isenções", $"{(string.IsNullOrEmpty(x.Approver) ? "o aprovador" : x.Approver)} avalia os {x.Requested} pedidos na transação <code>ATC</code>."));
        foreach (var c in d.Complements.Pending)
            p.Add((c.Titulo, E(c.Texto)));
        var man = d.Application.Manual;
        if (man.Count > 0)
            p.Add(("Aplicações manuais", E($"{man.Count} itens em {man.Select(m => m.Object).Distinct().Count()} objetos: " + string.Join(", ", man.Select(m => m.Object).Distinct().Order(StringComparer.Ordinal)) + ".")));
        if (x is not null && (x.NoScope > 0 || x.ToCheck.Count > 0))
        {
            var t = new List<string>();
            if (x.NoScope > 0)
                t.Add($"{x.NoScope} findings sem escopo de isenção voltam para a remediação");
            if (x.ToCheck.Count > 0)
                t.Add("conferir na transação <code>ATC</code> o pedido de " + E(string.Join(", ", x.ToCheck)));
            p.Add(("Isenção", string.Join("; ", t) + "."));
        }
        if (Of(sc, "Pendente") > 0)
            p.Add(("Itens pendentes", $"{Of(sc, "Pendente")} findings ainda sem decisão."));
        var ex = d.Extraction;
        var outOfScope = new List<string>();
        foreach (var q in new[] { "P1", "P2", "P3" })
        {
            var val = ex is null ? null : (q == "P1" ? ex.P1 : q == "P2" ? ex.P2 : ex.P3);
            if (!d.Scope.Contains(q) && val is > 0)
                outOfScope.Add($"{Prio[q]} ({N(val)})");
        }
        if (outOfScope.Count > 0)
            p.Add(("Novo ciclo", string.Join(", ", outOfScope) + ", seguido de uma nova execução do ATC para medir o resultado."));
        return $"""
            <section aria-labelledby="s-pend"><h2 id="s-pend"><small>Próximos passos</small>O que falta para encerrar</h2>
            <ol class="steps">{string.Concat(p.Select(t => $"<li><b>{E(t.Title)}:</b> {t.Text}</li>"))}</ol></section>
            """;
    }

    private static string List(ReportData d, string v)
    {
        (string F, string Label)[] chips;
        string head, title;
        if (v == ViewFindings)
        {
            chips = [("", "Todos"), ("KB", "KB"), ("Análise", "Análise"), ("Isenção", "Isenção"), ("ATIVADO", "Ativados")];
            head = $"<th class=\"num\">{(d.Cycles.Count > 1 ? "Ciclo.Nº" : "Nº")}</th><th>Objeto</th><th>Check / mensagem</th><th>Ref.</th><th>Solução</th><th>No sistema</th><th>Análise registrada</th>";
            title = $"Os {N(d.Items.Count)} findings";
        }
        else
        {
            chips = [("", "Todos"), ("Corrigido", "Corrigidos"), ("Corrigido + isenção", "Corrigido + isenção"), ("Isenção", "Só isenção"), ("ATIVADO", "Com ativação")];
            head = "<th>Objeto</th><th class=\"num\">Findings</th><th class=\"num\">KB</th><th class=\"num\">Análise</th><th class=\"num\">Isenção</th><th class=\"num\">Pend.</th><th>Situação</th><th>No sistema</th><th>Checks</th>";
            title = $"Os {N(d.TotalObjects)} objetos";
        }
        var buttons = string.Concat(chips.Select((c, i) => $"<button type=\"button\" class=\"chip\" id=\"f-{i}\" data-f=\"{E(c.F)}\" aria-pressed=\"{(c.F.Length == 0 ? "true" : "false")}\">{E(c.Label)}</button>"));
        return $"""
            <section aria-labelledby="s-lista"><h2 id="s-lista"><small>Detalhe</small>{title}</h2>
            <div class="toolbar">{buttons}<input type="search" id="busca" placeholder="Buscar objeto, referência ou texto" aria-label="Buscar na lista"><span class="count" id="cont"></span></div>
            <div class="tbl"><table id="lista"><thead><tr>{head}</tr></thead><tbody></tbody></table></div></section>
            """;
    }

    private static string Process(ReportData d)
    {
        var x = d.Process;
        var ex = x.Runs;
        var sr = x.Missing;
        var t = x.Totals;
        const string head = "<small>Execução do processo</small>Como o trabalho foi executado";
        if (ex.Count == 0)
            return $"<section class=\"proc\" aria-labelledby=\"s-proc\"><h2 id=\"s-proc\">{head}</h2>"
                   + "<p class=\"muted\">Não registrado (execuções anteriores ao registro do processo, sem o campo <code>execucao</code> nos "
                   + "<code>historico.json</code>): agentes, modelos, skills e paralelismo não são estimados.</p></section>";
        string Models(IEnumerable<string> l) { var s = string.Join(", ", l.Select(Short)); return s.Length == 0 ? "?" : s; }
        var lines = new StringBuilder();
        foreach (var r in ex)
        {
            var sk = r.Skills.Count > 0 ? string.Join(", ", r.Skills.Select(kv => $"{E(kv.Key)} ({kv.Value})")) : "–";
            var par = r.Parallelism.Count > 0
                ? $"{r.Parallelism.Count} período(s), até {r.Parallelism.Max(p => p.MaxSimultaneous ?? 0)} simultâneos, ganho {Sec(r.ParallelismGainSeconds)}" : "sem paralelismo";
            lines.Append($"<tr><td>{E(r.Stage)}</td><td class=\"mono\">{E(r.End)}</td><td class=\"mono\">{E(Models(r.MainModel))}</td><td>{sk}</td><td class=\"num\">{r.AgentCount}</td><td class=\"num\">{Sec(r.AgentsActiveSeconds)}</td><td class=\"num\">{N(r.AgentsTokens)}</td><td>{par}</td></tr>");
        }
        foreach (var s in sr)
            lines.Append($"<tr><td>{E(s.Stage)}</td><td class=\"mono\">{E(s.End)}</td><td colspan=\"6\" class=\"muted\">não registrado (anterior ao registro do processo{(s.SkillVersion is not null ? ", skill " + E(s.SkillVersion) : "")})</td></tr>");
        var det = new StringBuilder();
        foreach (var r in ex.Where(r => r.Agents.Count > 0))
        {
            var ls = string.Concat(r.Agents.Select(a => $"<tr><td class=\"mono\">{E(a.Type ?? "?")}</td><td>{E(a.Description ?? "")}</td><td class=\"mono\">{E(Models(a.Models))}</td><td class=\"num\">{Sec(a.ActiveSeconds)}</td><td class=\"num\">{N(a.Tokens)}</td></tr>"));
            var pl = string.Concat(r.Parallelism.Select(p => $"<tr><td class=\"mono\">{E(p.Start)} – {E(p.End.Length >= 5 ? p.End[^5..] : p.End)}</td><td class=\"num\">{p.Agents}</td><td class=\"num\">{p.MaxSimultaneous}</td><td class=\"num\">{Sec(p.ActiveSum ?? 0)}</td><td class=\"num\">{Sec(p.ClockSeconds ?? 0)}</td><td class=\"num\">{Sec(p.GainSeconds ?? 0)}</td></tr>"));
            var pt = pl.Length > 0
                ? $"<div class=\"tbl\"><table><thead><tr><th>Período com agentes simultâneos</th><th class=\"num\">Agentes</th><th class=\"num\">Máx. simultâneos</th><th class=\"num\">Soma do ativo</th><th class=\"num\">Relógio</th><th class=\"num\">Ganho</th></tr></thead><tbody>{pl}</tbody></table></div>" : "";
            det.Append($"<details><summary>{E(r.Stage)} · {E(r.End)}: {r.AgentCount} agentes</summary><div class=\"tbl\"><table><thead><tr><th>Tipo</th><th>Descrição</th><th>Modelo</th><th class=\"num\">Tempo ativo</th><th class=\"num\">Tokens</th></tr></thead><tbody>{ls}</tbody></table></div>{pt}</details>");
        }
        string Li<T>(IReadOnlyDictionary<string, T> dict) =>
            dict.Count > 0 ? string.Concat(dict.Select(kv => $"<dt>{E(Short(kv.Key))}</dt><dd>{N(Convert.ToInt64(kv.Value, CultureInfo.InvariantCulture))}</dd>")) : "<dt>–</dt><dd>Nenhum</dd>";
        var warn = sr.Count > 0 ? $"<p class=\"note\"><b>Sem registro.</b> {sr.Count} execução(ões) anterior(es) ao registro do processo não entram nos totais abaixo e nada foi estimado.</p>" : "";
        return $"""
            <section class="proc" aria-labelledby="s-proc"><h2 id="s-proc">{head}</h2>
            <p>Agentes chamados, modelos, skills e paralelismo de cada execução, lidos dos transcripts do Claude Code ({E(x.Source)}).</p>
            <div class="grid3"><div class="panel"><h3>Agentes por modelo ({t.Agents} no total)</h3><dl class="kv">{Li(t.AgentsByModel)}</dl></div>
            <div class="panel"><h3>Tokens por modelo (agentes e sessão principal)</h3><dl class="kv">{Li(t.TokensByModel)}</dl></div>
            <div class="panel"><span class="big">{Hm(t.ParallelismGainSeconds)}</span><span class="muted">de ganho de tempo com agentes em paralelo (soma do tempo ativo dos agentes sobrepostos − relógio do período)</span></div></div>
            <div class="panel"><h3>Agentes por tipo</h3><dl class="kv">{Li(t.AgentsByType)}</dl></div>
            <div class="tbl"><table><thead><tr><th>Etapa</th><th>Fim</th><th>Modelo principal</th><th>Skills usadas</th><th class="num">Agentes</th><th class="num">Ativo dos agentes</th><th class="num">Tokens dos agentes</th><th>Paralelismo</th></tr></thead><tbody>{lines}</tbody></table></div>
            {warn}{det}
            <p class="note"><b>Como foi medido.</b> Aqui cada "agente" é um uso de IA (Mechanical, Review ou Analysis) com o modelo que respondeu, somando as chamadas registradas nas execuções; o paralelismo é o grau configurado na execução. Tempo ativo e ganho do paralelismo não são medidos pelo aplicativo. Tokens somam o consumo registrado em cada chamada. Horários no fuso local.</p></section>
            """;
    }

    private static string Costs(ReportData d, string v)
    {
        var c = d.Costs;
        var t = c.Total;
        if (c.Phases.Count == 0)
            return "";
        var cr = Pct(t.CacheRead, t.Total).Replace(".", ",", StringComparison.Ordinal);
        var unit = v == ViewFindings ? ("finding", c.NetPerFindingSeconds, d.Items.Count) : ("objeto", c.NetPerObjectSeconds, d.TotalObjects);
        var rem = c.RemediationNetSeconds;
        var ind = ((unit.Item2 ?? 0) / 60.0).ToString("0.0", CultureInfo.InvariantCulture).Replace(".", ",", StringComparison.Ordinal) + " min";
        var lines = string.Concat(c.Phases.Select(f =>
            $"<tr><td>{E(f.Stage)}</td><td class=\"mono\">{E(f.Period)}</td><td class=\"num\">{(f.Sessions != 0 ? f.Sessions.ToString(CultureInfo.InvariantCulture) : "–")}</td><td class=\"num\">{Hm(f.NetSeconds)}</td><td class=\"num\">{N(f.Total)}</td><td class=\"num\">{N(f.Output)}</td><td class=\"num\">{N(f.CacheRead)}</td><td class=\"num\">{N(f.InputAndCacheWrite)}</td></tr>"));
        var foot = $"<tr><td>Total</td><td class=\"mono\">{E(t.Period ?? "")}</td><td class=\"num\">{t.Sessions}</td><td class=\"num\">{Hm(t.NetSeconds)}</td><td class=\"num\">{N(t.Total)}</td><td class=\"num\">{N(t.Output)}</td><td class=\"num\">{N(t.CacheRead)}</td><td class=\"num\">{N(t.InputAndCacheWrite)}</td></tr>";
        var clock = c.ClockInsteadOfNet.Count > 0 ? "Sem tempo líquido medido, entra o tempo de relógio (maior ou igual): " + E(string.Join(", ", c.ClockInsteadOfNet)) + ". " : "";
        return $"""
            <section class="cost" aria-labelledby="s-cost"><h2 id="s-cost"><small>Custos e tempos</small>Quanto o ciclo consumiu</h2>
            <div class="grid3"><div class="panel"><span class="big">{Hm(t.NetSeconds)}</span><span class="muted">tempo líquido de trabalho em todas as etapas</span></div>
            <div class="panel"><span class="big">{Mi(t.Total)}</span><span class="muted">tokens processados no total ({cr} leitura de cache)</span></div>
            <div class="panel"><span class="big">{ind}</span><span class="muted">tempo líquido de remediação por {unit.Item1} ({(rem != 0 ? Hm(rem) : "–")} ÷ {N(unit.Item3)})</span></div></div>
            <div class="tbl"><table><thead><tr><th>Etapa</th><th>Período</th><th class="num">Sessões</th><th class="num">Tempo líquido</th><th class="num">Tokens totais</th><th class="num">Saída</th><th class="num">Leitura de cache</th><th class="num">Entrada + escrita de cache</th></tr></thead><tbody>{lines}</tbody><tfoot>{foot}</tfoot></table></div>
            <p class="note"><b>Como foi medido.</b> Tempo líquido é a soma do tempo das execuções do mesmo alvo (sessão, lote ou extração), desconsiderando pausas acima de 10 minutos entre elas. Tokens somam o consumo registrado em cada chamada de IA (entrada e saída; o aplicativo não registra cache). Uma sessão dividida em etapas conta uma vez. {clock}{(c.ReportIncluded ? "A geração deste relatório está incluída." : "A geração deste relatório não está incluída.")} O tempo de calendário do período foi de cerca de {(t.CalendarHours?.ToString(CultureInfo.InvariantCulture) ?? "–")} horas. Horários no fuso local.</p></section>
            """;
    }

    private static List<Dictionary<string, object?>> Rows(ReportData d, string v, IReadOnlyList<ReportObject> ob)
    {
        if (v == ViewFindings)
        {
            var multi = d.Cycles.Count > 1;
            return [.. d.Items.Select(x => new Dictionary<string, object?>
            {
                ["n"] = multi ? $"{x.Cycle}.{x.N}" : x.N, ["obj"] = x.Obj, ["tipo"] = x.Type, ["check"] = x.Check, ["msg"] = x.Message, ["ref"] = x.Ref,
                ["sol"] = x.Solution, ["sis"] = x.System, ["req"] = x.Request, ["obs"] = x.Obs, ["f"] = new[] { x.Solution, x.System },
                ["busca"] = $"{x.Obj} {x.Type} {x.Ref} {x.Check} {x.Message} {x.Obs}".ToLowerInvariant(),
            })];
        }
        return [.. ob.OrderByDescending(o => o.Count).ThenBy(o => o.Obj, StringComparer.Ordinal).Select(o => new Dictionary<string, object?>
        {
            ["obj"] = o.Obj, ["tipo"] = o.Type, ["n"] = o.Count, ["kb"] = o.Kb, ["ia"] = o.Ai, ["is"] = o.Exemption, ["pend"] = o.Pending, ["cat"] = o.Category,
            ["req"] = o.Requests, ["isp"] = o.ExemptionRequested, ["checks"] = o.Checks,
            ["f"] = o.Requests.Count > 0 ? new[] { o.Category, "ATIVADO" } : new[] { o.Category },
            ["busca"] = $"{o.Obj} {o.Type} {o.Checks}".ToLowerInvariant(),
        })];
    }
}
