using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static Mignow.Atc.Application.Modules.Report.ReportFormat;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// Porte dos relatórios de análise da skill atc-resultado: <c>analise_isencao.py</c> (para quem aprova), <c>analise_pendencias.py</c>
/// (cliente, funcional, consultor) e <c>analise_mignow.py</c> (o que o ATC não pegou). Cada método recebe os dados já lidos do
/// banco (inclusive o trecho de código) e devolve a página HTML completa na identidade MIGNOW (CSS do modelo).
/// </summary>
public static class AnalysisRenderer
{
    public static readonly IReadOnlyList<string> NoCodeTypes = ["TABL", "VIEW", "DTEL", "DOMA", "TTYP", "SHLP", "STRU", "MSAG", "TABU", "DEVC", "SHMA"];

    // ---------------------------------------------------------------- isenção

    public sealed record ExemptionRow(int Seq, int N, string Obj, string Type, string? Include, int? Line, string? Ref, string Check, string Msg, string? Reason,
        string? Result, string? Justification, string Obs, string Priority, string? RobT, string? Note, string Conf, IReadOnlyList<CodeLine>? Snippet, string? Origin);

    public sealed record ExemptionInput(string Customer, string? Sid, string? Client, string? Approver, string? Series, string? ExecutedAt8, string Batch,
        string Phase, IReadOnlyList<ExemptionRow> Rows, DateTimeOffset Now);

    private static readonly Dictionary<string, (string Title, string Text)> ExemptionRules = new()
    {
        ["R9"] = ("Upgrade: objeto fora do escopo", "Projeto de upgrade (S/4 → S/4 2025): findings de BSEG, KNKK, S066 e S067 não são remediados no upgrade (decisão do consultor, triagem R9 da atc-remediacao)."),
        ["R8"] = ("Escrita em tabela standard", "MODIFY/UPDATE/INSERT direto em tabela standard: fica com a análise do cliente (Out Of Scope – triagem R8)."),
        ["R1"] = ("Dicionário: só chave/verificação", "Objeto sem código ABAP que usa o referenciado só como chave/verificação (check table, tabela de valores, método de seleção) e o referenciado está ativo no DD02L (conferência R1 no DDIC)."),
        ["IA"] = ("Código já correto (análise)", "O agente leu o código e o DDIC do sistema: o uso apontado continua válido no S/4HANA (ex.: BAPI sem campo de material, campo existente, tipo exigido pela interface). Prova na justificativa."),
    };

    public static string ExemptionRuleOf(string obs) =>
        obs.Contains("Triagem R9", StringComparison.Ordinal) ? "R9"
        : obs.Contains("Out Of Scope", StringComparison.Ordinal) ? "R8"
        : obs.Contains("Conferência R1", StringComparison.Ordinal) || obs.Contains("Triagem R1", StringComparison.Ordinal) ? "R1" : "IA";

    private static string CodeBlock(IReadOnlyList<CodeLine> lines, Func<int, bool> hit) =>
        string.Concat(lines.Select(l => $"<span class=\"ln{(hit(l.Line) ? " hit" : "")}\"><i>{l.Line}</i>{(l.Text.Length == 0 ? " " : E(l.Text))}</span>"));

    private const string CommonCss = """

        .grp > summary { color: var(--fg); display: flex; flex-wrap: wrap; gap: 6px 10px; align-items: center; }
        .grp .cnt { margin-left: auto; color: var(--muted); font-size: 12.5px; }
        .grp-b { padding: 12px 14px 14px; display: grid; gap: 8px; }
        .fd { background: var(--surface-2); }
        .fd > summary { display: flex; flex-wrap: wrap; gap: 6px 10px; align-items: center; color: var(--fg); }
        .fd-b { padding: 12px 14px; display: grid; gap: 12px; }
        details.sub { background: var(--bg); }
        details.sub > summary { font-size: 13px; }
        table.papeis caption { text-align: left; font-size: 13px; color: var(--muted); padding: 0 0 6px; }
        table.papeis th { white-space: nowrap; width: 1%; vertical-align: top; }
        .code { border: 1px solid var(--line); border-radius: 8px; overflow: hidden; background: var(--bg); }
        .code-h { font-family: var(--mono); font-size: 12px; color: var(--muted); padding: 6px 12px; border-bottom: 1px solid var(--line); }
        .code pre { margin: 0; padding: 8px 0; overflow-x: auto; font-family: var(--mono); font-size: 12.5px; line-height: 1.55; }
        .code .ln { display: block; padding: 0 12px; white-space: pre; }
        .code .ln i { display: inline-block; width: 5ch; margin-right: 12px; text-align: right; color: var(--muted); font-style: normal; user-select: none; }
        .code .ln.hit { background: var(--warn-soft); box-shadow: inset 3px 0 0 var(--warn); }
        .code .ln.mk { color: var(--accent); }
        .hidden { display: none !important; }
        """;

    private const string Fonts = "<link rel=\"stylesheet\" href=\"https://fonts.googleapis.com/css2?family=Michroma&family=Jost:wght@300;400;500;600&family=JetBrains+Mono:wght@400;500&display=swap\">";

    private static string Page(string title, string css, string body, string footer, string script) =>
        "<!doctype html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n"
        + $"<title>{E(title)}</title>\n{Fonts}\n<style>{css}{CommonCss}</style></head><body>\n{body}\n"
        + $"<footer><div class=\"wrap\"><span class=\"brand\">MIGNOW</span><span>{footer}</span></div></footer>\n<script>\n{script}\n</script></body></html>";

    public static string RenderExemption(ExemptionInput input, string css)
    {
        var regs = input.Rows.Select(r => (R: r, Reg: ExemptionRuleOf(r.Obs))).ToList();
        var groups = new List<(string Key, List<(ExemptionRow R, string Reg)> Items)>();
        foreach (var x in regs.OrderBy(x => x.Reg, StringComparer.Ordinal).ThenBy(x => x.R.Ref ?? "", StringComparer.Ordinal)
                     .ThenBy(x => x.R.Check, StringComparer.Ordinal).ThenBy(x => x.R.Seq))
        {
            var key = $"{x.Reg}|{(string.IsNullOrEmpty(x.R.Ref) ? "–" : x.R.Ref)}|{x.R.Check}";
            var g = groups.FirstOrDefault(g => g.Key == key);
            if (g.Items is null)
            {
                g = (key, []);
                groups.Add(g);
            }
            g.Items.Add(x);
        }
        var byReason = regs.GroupBy(x => x.R.Reason ?? "–").OrderByDescending(g => g.Count()).ToList();
        var byResult = regs.GroupBy(x => x.R.Result ?? "–").OrderByDescending(g => g.Count()).ToList();
        var objs = regs.Select(x => (x.R.Type, x.R.Obj)).Distinct().Count();
        var accepted = byResult.Where(g => g.Key is "PEDIDO" or "JA_PEDIDO" or "SIMULADO").Sum(g => g.Count());
        var withCode = regs.Count(x => x.R.Snippet is { Count: > 0 });
        var phase = input.Phase;

        string PillRes(string t) => $"<span class=\"pill {(t is "PEDIDO" or "JA_PEDIDO" ? "p-ok" : t == "SIMULADO" ? "p-info" : t == "SEM_ESCOPO" ? "p-warn" : "p-bad")}\">{E(t)}</span>";

        string Card((ExemptionRow R, string Reg) x)
        {
            var f = x.R;
            string cod;
            if (f.Snippet is { Count: > 0 })
                cod = $"<div class=\"code\"><div class=\"code-h\">{E(f.Include)} · linha {E(f.Line)} · {E(f.Origin)}</div><pre>{CodeBlock(f.Snippet, k => k == f.Line)}</pre></div>";
            else if (NoCodeTypes.Contains(f.Type.ToUpperInvariant()))
                cod = "<p class=\"note\">Objeto de dicionário (sem código ABAP): a prova está na justificativa (DDIC lido no sistema).</p>";
            else
                cod = "<p class=\"note\">Trecho de código não disponível no cache local.</p>";
            var search = string.Join(" ", new[] { f.Obj, f.Include, f.Ref, f.Check, f.Msg, x.R.Obs, f.Reason }.Select(v => v ?? "None")).ToLowerInvariant();
            return $"""
                <details class="fd" data-mot="{E(f.Reason ?? "")}" data-reg="{x.Reg}" data-q="{E(search)}">
                <summary><span class="mono">#{f.Seq}</span> <b class="mono">{E(f.Obj)}</b> <span class="muted mono">{E(f.Include ?? "")}{(f.Line is not null ? ":" + f.Line : "")}</span>
                <span class="pill p-acc">{E(f.Reason ?? "–")}</span> {PillRes(f.Result ?? "–")}{(f.Conf.Contains("sem isen", StringComparison.OrdinalIgnoreCase) ? " <span class=\"pill p-warn\">conferir na ATC</span>" : "")}</summary>
                <div class="fd-b"><dl class="kv">
                <dt>Item</dt><dd>{f.N} · {E(f.Type)} {E(f.Obj)} · prioridade {E(f.Priority)}</dd>
                <dt>Check</dt><dd>{E(f.Check)} – {E(f.Msg)}</dd>
                <dt>Referenciado</dt><dd><code>{E(string.IsNullOrEmpty(f.Ref) ? "–" : f.Ref)}</code> {E(f.RobT ?? "")}{(string.IsNullOrEmpty(f.Note) ? "" : " · nota " + E(f.Note))}</dd>
                <dt>Base da decisão</dt><dd>{E(ExemptionRules[x.Reg].Title)}</dd>
                <dt>Justificativa enviada</dt><dd>{E(string.IsNullOrEmpty(f.Justification) ? f.Obs : f.Justification)}</dd>
                {(f.Conf.Length > 0 ? $"<dt>Conferência no SAP</dt><dd>{E(f.Conf)}</dd>" : "")}
                </dl>{cod}</div></details>
                """;
        }

        var blocks = string.Concat(groups.Select(g =>
        {
            var parts = g.Key.Split('|', 3);
            var mot = g.Items.GroupBy(x => x.R.Reason ?? "–").OrderByDescending(m => m.Count()).First().Key;
            var nobj = g.Items.Select(x => x.R.Obj).Distinct().Count();
            return $"""
                <details class="grp" data-reg="{parts[0]}"><summary><span class="pill p-acc">{E(mot)}</span> <b>{E(ExemptionRules[parts[0]].Title)}</b> · <code>{E(parts[1])}</code> · {E(parts[2])}
                <span class="cnt mono">{g.Items.Count} findings · {nobj} objetos</span></summary>
                <div class="grp-b"><p class="note">{E(ExemptionRules[parts[0]].Text)}</p>{string.Concat(g.Items.Select(Card))}</div></details>
                """;
        }));
        var summary = string.Concat(groups.Select(g =>
        {
            var parts = g.Key.Split('|', 3);
            var mot = g.Items.GroupBy(x => x.R.Reason ?? "–").OrderByDescending(m => m.Count()).First().Key;
            return $"<tr><td><span class=\"pill p-acc\">{E(mot)}</span></td><td>{E(ExemptionRules[parts[0]].Title)}</td><td><code>{E(parts[1])}</code></td><td>{E(parts[2])}</td><td class=\"num\">{g.Items.Count}</td><td class=\"num\">{g.Items.Select(x => x.R.Obj).Distinct().Count()}</td></tr>";
        }));
        var how = string.Concat(ExemptionRules.Where(kv => regs.Any(x => x.Reg == kv.Key)).Select(kv => $"<div class=\"panel\"><h3>{E(kv.Value.Title)}</h3><p class=\"muted\">{E(kv.Value.Text)}</p></div>"));
        var confTxt = regs.Any(x => x.R.Conf.Length > 0)
            ? " Conferência no SAP: " + string.Join(", ", regs.Where(x => x.R.Conf.Length > 0).GroupBy(x => x.R.Conf).OrderByDescending(g => g.Count()).Select(g => $"{g.Count()} × {E(g.Key)}")) + "." : "";
        var kpiReason = string.Concat(byReason.Take(2).Select(g => $"<div class=\"kpi\"><span class=\"n\">{g.Count()}</span><span class=\"l\">motivo {E(g.Key)}</span></div>"));
        var chips = string.Concat(byReason.Select(g => $"<button class=\"chip\" data-f=\"mot\" data-v=\"{E(g.Key)}\" aria-pressed=\"false\">{E(g.Key)}</button>"));
        var body = $"""
            <header class="hero"><div class="wrap">
            <div class="eyebrow">Análise para aprovação · ATC</div>
            <h1>Isenções do <span>{E(input.Customer)}</span></h1>
            <p class="lede">Cada pedido de isenção com a <strong>base da decisão</strong>, a justificativa enviada ao SAP e o <strong>trecho do código</strong> do finding, agrupados por motivo para aprovar ou recusar em bloco.</p>
            <div class="meta"><span>Sistema <b>{E(input.Sid)}/{E(input.Client)}</b></span><span>Aprovador <b>{E(string.IsNullOrEmpty(input.Approver) ? "–" : input.Approver)}</b></span>
            <span>Execução ATC <b>{E(input.Series)} {E(input.ExecutedAt8)}</b></span><span>Lote <b>{E(input.Batch)}</b> ({E(phase)})</span><span>Gerado <b>{input.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}</b></span></div>
            <div class="kpis">
            <div class="kpi acc"><span class="n">{regs.Count}</span><span class="l">findings no pedido</span></div>
            <div class="kpi"><span class="n">{objs}</span><span class="l">objetos</span></div>
            {kpiReason}
            <div class="kpi ok"><span class="n">{accepted}</span><span class="l">{(phase == "envio" ? "pedidos aceitos pelo SAP" : "simulados sem erro")}</span></div>
            </div></div></header>
            <main><div class="wrap">
            <section><h2><small>Como ler</small>Do grupo para o finding</h2>
            <p>Os findings estão agrupados por <b>base da decisão</b>, objeto referenciado e check. Abra um grupo para ver a regra que o sustenta e cada finding; abra o finding para ver a justificativa e o código (a linha do finding fica destacada). {withCode} de {regs.Count} findings têm o trecho de código; objetos de dicionário mostram a prova do DDIC.</p>
            <div class="grid2">{how}</div></section>
            <section><h2><small>Resumo</small>Grupos de decisão</h2>
            <div class="tbl"><table><thead><tr><th>Motivo</th><th>Base</th><th>Referenciado</th><th>Check</th><th class="num">Findings</th><th class="num">Objetos</th></tr></thead><tbody>{summary}</tbody>
            <tfoot><tr><td colspan="4">Total</td><td class="num">{regs.Count}</td><td class="num">{objs}</td></tr></tfoot></table></div>
            <p class="note">Resultado do {E(phase)}: {string.Join(", ", byResult.Select(g => $"{E(g.Key)} {g.Count()}"))}.{confTxt}</p></section>
            <section><h2><small>Detalhe</small>Findings por grupo</h2>
            <div class="toolbar"><button class="chip" data-f="mot" data-v="" aria-pressed="true">Todos</button>{chips}
            <input type="search" id="q" placeholder="Objeto, include, referenciado, texto…" aria-label="Filtrar findings">
            <button class="chip" id="abrir" aria-pressed="false">Abrir grupos</button><span class="count" id="cnt"></span></div>
            <div id="grupos" style="display:grid;gap:10px">{blocks}</div></section>
            </div></main>
            """;
        var script = """
            (() => {
              let mot = "";
              const q = document.getElementById("q"), cnt = document.getElementById("cnt");
              function apply() {
                const t = q.value.trim().toLowerCase(); let n = 0;
                document.querySelectorAll(".grp").forEach(g => {
                  let v = 0;
                  g.querySelectorAll(".fd").forEach(f => {
                    const ok = (!mot || f.dataset.mot === mot) && (!t || f.dataset.q.includes(t));
                    f.classList.toggle("hidden", !ok); if (ok) v++;
                  });
                  g.classList.toggle("hidden", v === 0); n += v;
                  if (t && v) g.open = true;
                });
                cnt.textContent = n + " findings";
              }
              document.querySelectorAll('.chip[data-f="mot"]').forEach(b => b.addEventListener("click", () => {
                mot = b.dataset.v; document.querySelectorAll('.chip[data-f="mot"]').forEach(x => x.setAttribute("aria-pressed", x === b)); apply();
              }));
              q.addEventListener("input", apply);
              const ab = document.getElementById("abrir");
              ab.addEventListener("click", () => {
                const on = ab.getAttribute("aria-pressed") !== "true"; ab.setAttribute("aria-pressed", on);
                ab.textContent = on ? "Fechar grupos" : "Abrir grupos";
                document.querySelectorAll(".grp:not(.hidden)").forEach(g => g.open = on);
              });
              apply();
            })();
            """;
        return Page($"Isenções ATC {input.Customer}", css, body,
            $"Gerado pela skill atc-resultado a partir do lote {E(input.Batch)} da atc-isencao e da sessão da atc-remediacao. Nada aqui foi lido do SAP na geração.", script);
    }

    // ---------------------------------------------------------------- pendências

    public sealed record PendingRow(string Group, string Title, int N, string Obj, string Type, string? Package, string? Priority, string? Ref, string? RobT,
        string? Note, string Check, string Msg, string Status, string Reason, string? Include, int? Line, IReadOnlyList<CodeLine>? Snippet, string? Origin,
        IReadOnlyList<string> Blocks, int BaseN);

    public sealed record PendingInput(string Customer, string? Sid, string? Client, string? ProjectType, string? Request, IReadOnlyList<PendingRow> Rows, DateTimeOffset Now);

    public static readonly IReadOnlyDictionary<string, (string Responsible, string Action)> PendingActions = new Dictionary<string, (string, string)>
    {
        ["SD"] = ("Consultor MIGNOW", "Item sem decisão: analisar o código e o dicionário e decidir entre correção, isenção e cancelamento (ou encaminhar ao funcional/cliente)."),
        ["MP"] = ("Cliente / fornecedor do produto", "Objeto protegido pelo produto de terceiro: encaminhar a correção pronta (bloco abaixo) ao fornecedor ou liberar a alteração no sistema."),
        ["MS"] = ("Consultor / desenvolvedor", "Aplicar a correção no nó do Smartform (transação SMARTFORMS) conforme o bloco; o código gerado /1BCDWB/ não é editável."),
        ["MF"] = ("Consultor / desenvolvedor", "Alterar a interface do módulo de função na SE37 conforme o bloco (o include $01 é gerado) e conferir os chamadores."),
        ["MM"] = ("Consultor / desenvolvedor", "Aplicar na SE38 com o assistente de modificação (chave de acesso), conforme o bloco."),
    };

    public static readonly IReadOnlyDictionary<string, string> PendingTitles = new Dictionary<string, string>
    {
        ["SD"] = "Sem decisão",
        ["MP"] = "Correção pronta – objeto protegido por produto de terceiro",
        ["MS"] = "Correção pronta – Smartform",
        ["MF"] = "Correção pronta – interface de módulo de função (SE37)",
        ["MM"] = "Correção pronta – modificação de objeto SAP",
    };

    public static string ManualGroup(string obs) =>
        obs.Contains("SMARTFORMS", StringComparison.Ordinal) ? "MS"
        : obs.Contains("SE37", StringComparison.Ordinal) ? "MF"
        : obs.Contains("assistente de modificação", StringComparison.Ordinal) || obs.Contains("SE38", StringComparison.Ordinal) ? "MM" : "MP";

    public static string RenderPending(PendingInput input, string css)
    {
        var order = new[] { "SD", "MP", "MS", "MF", "MM" };
        var regs = input.Rows;
        var groups = regs.OrderBy(x => Array.IndexOf(order, x.Group)).ThenBy(x => x.Obj, StringComparer.Ordinal).ThenBy(x => x.N).GroupBy(x => x.Group).ToList();
        var resp = regs.GroupBy(x => PendingActions[x.Group].Responsible).OrderByDescending(g => g.Count()).ToList();
        var objs = regs.Select(x => (x.Type, x.Obj)).Distinct().Count();
        var nPend = regs.Count(x => x.Group == "SD");
        var nMan = regs.Count - nPend;
        var withCode = regs.Count(x => x.Snippet is { Count: > 0 });
        var withBlock = regs.Count(x => x.Blocks.Count > 0);
        string Pill(string g) => $"<span class=\"pill {(g == "SD" ? "p-warn" : "p-info")}\">{E(g)}</span>";

        string Card(PendingRow x)
        {
            string cod;
            if (x.Snippet is { Count: > 0 })
                cod = $"<details class=\"sub\"><summary>Código atual · {E(x.Include)} linha {E(x.Line)} <span class=\"muted\">({E(x.Origin)})</span></summary><div class=\"code\"><pre>{CodeBlock(x.Snippet, k => k == x.Line)}</pre></div></details>";
            else if (NoCodeTypes.Contains(x.Type.ToUpperInvariant()))
                cod = "<p class=\"note\">Objeto de dicionário (sem código ABAP).</p>";
            else if (x.Line is null)
                cod = "<p class=\"note\">O ATC não grava posição para este finding (erro de sintaxe/pré-requisito do teste): o erro real está no motivo.</p>";
            else
                cod = "<p class=\"note\">Trecho de código não disponível no cache local.</p>";
            var blo = string.Concat(x.Blocks.Select((b, k) =>
                $"<details class=\"sub\"><summary>Correção pronta{(x.Blocks.Count > 1 ? $" (parte {k + 1})" : "")}{(x.BaseN != x.N ? $" · mesmo bloco do item {x.BaseN}" : "")}</summary><div class=\"code\"><pre>"
                + string.Concat(b.Split('\n').Select(l => { l = l.TrimEnd('\r'); return $"<span class=\"ln{(l.StartsWith("*>>>", StringComparison.Ordinal) || l.StartsWith("*<<<", StringComparison.Ordinal) ? " mk" : "")}\">{(l.Length == 0 ? " " : E(l))}</span>"; }))
                + "</pre></div></details>"));
            var search = string.Join(" ", new[] { x.Obj, x.Include, x.Ref, x.Check, x.Msg, x.Reason }.Select(v => v ?? "None")).ToLowerInvariant();
            var st = x.Status.Length == 0 ? "Pendente" : x.Status;
            return $"""
                <details class="fd" data-q="{E(search)}"><summary><span class="mono">#{x.N}</span> <b class="mono">{E(x.Obj)}</b>
                <span class="muted mono">{E(x.Include ?? "")}{(x.Line is not null ? ":" + x.Line : "")}</span> <code>{E(string.IsNullOrEmpty(x.Ref) ? "–" : x.Ref)}</code>
                <span class="pill {(st == "Pendente" ? "p-warn" : "p-info")}">{E(st)}</span>{(x.Blocks.Count > 0 ? " <span class=\"pill p-ok\">correção pronta</span>" : "")}</summary>
                <div class="fd-b"><dl class="kv">
                <dt>Objeto</dt><dd>{E(x.Type)} {E(x.Obj)} · pacote {E(string.IsNullOrEmpty(x.Package) ? "–" : x.Package)} · prioridade {E(string.IsNullOrEmpty(x.Priority) ? "1" : x.Priority)}</dd>
                <dt>Check</dt><dd>{E(x.Check)} – {E(x.Msg)}</dd>
                <dt>Referenciado</dt><dd><code>{E(string.IsNullOrEmpty(x.Ref) ? "–" : x.Ref)}</code> {E(x.RobT ?? "")}{(string.IsNullOrEmpty(x.Note) ? "" : " · nota " + E(x.Note))}</dd>
                <dt>Motivo / análise</dt><dd>{E(x.Reason)}</dd>
                <dt>Ação esperada</dt><dd>{E(PendingActions[x.Group].Action)}</dd>
                </dl>{cod}{blo}</div></details>
                """;
        }

        var sections = string.Concat(groups.Select(g => $"""
            <details class="grp" data-r="{E(PendingActions[g.Key].Responsible)}"><summary>{Pill(g.Key)} <b>{E(g.First().Title)}</b> · {E(PendingActions[g.Key].Responsible)}
            <span class="cnt mono">{g.Count()} itens · {g.Select(x => x.Obj).Distinct().Count()} objetos</span></summary>
            <div class="grp-b"><p class="note"><b>Ação esperada:</b> {E(PendingActions[g.Key].Action)}</p>{string.Concat(g.Select(Card))}</div></details>
            """));
        var summary = string.Concat(groups.Select(g =>
            $"<tr><td>{Pill(g.Key)}</td><td>{E(g.First().Title)}</td><td>{E(PendingActions[g.Key].Responsible)}</td><td>{E(PendingActions[g.Key].Action)}</td><td class=\"num\">{g.Count()}</td><td class=\"num\">{g.Count(x => x.Blocks.Count > 0)}</td></tr>"));
        var chips = string.Concat(resp.Select(g => $"<button class=\"chip\" data-r=\"{E(g.Key)}\" aria-pressed=\"false\">{E(g.Key)} ({g.Count()})</button>"));
        var body = $"""
            <header class="hero"><div class="wrap">
            <div class="eyebrow">Análise para decisão · ATC</div>
            <h1>Pendências do <span>{E(input.Customer)}</span></h1>
            <p class="lede">O que ainda depende de <strong>cliente, funcional ou consultor</strong>: cada item com o motivo, a <strong>ação esperada</strong>, o código atual do finding e, quando existe, a correção já pronta.</p>
            <div class="meta"><span>Sistema <b>{E(input.Sid)}/{E(input.Client)}</b></span><span>Projeto <b>{E(string.IsNullOrEmpty(input.ProjectType) ? "–" : input.ProjectType)}</b></span><span>Request <b>{E(string.IsNullOrEmpty(input.Request) ? "–" : input.Request)}</b></span><span>Gerado <b>{input.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}</b></span></div>
            <div class="kpis">
            <div class="kpi acc"><span class="n">{regs.Count}</span><span class="l">itens para decidir ou aplicar</span></div>
            <div class="kpi"><span class="n">{nPend}</span><span class="l">pendentes sem decisão</span></div>
            <div class="kpi"><span class="n">{nMan}</span><span class="l">correções prontas, aplicação manual</span></div>
            <div class="kpi"><span class="n">{objs}</span><span class="l">objetos</span></div>
            <div class="kpi ok"><span class="n">{withBlock}</span><span class="l">com bloco de correção pronto</span></div>
            </div></div></header>
            <main><div class="wrap">
            <section><h2><small>Resumo</small>Quem decide o quê</h2>
            <div class="tbl"><table><thead><tr><th>Grupo</th><th>Assunto</th><th>Responsável</th><th>Ação esperada</th><th class="num">Itens</th><th class="num">Com correção</th></tr></thead><tbody>{summary}</tbody>
            <tfoot><tr><td colspan="4">Total</td><td class="num">{regs.Count}</td><td class="num">{withBlock}</td></tr></tfoot></table></div>
            <p class="note">Grupo SD: itens sem decisão (status pendente na sessão). Grupos M: correções já decididas e preparadas que não puderam ser gravadas via ADT. {withCode} itens têm o trecho do código do finding (backup anterior às correções ou fonte em cache).</p></section>
            <section><h2><small>Detalhe</small>Itens por grupo</h2>
            <div class="toolbar"><button class="chip" data-r="" aria-pressed="true">Todos</button>{chips}
            <input type="search" id="q" placeholder="Objeto, include, referenciado, motivo…" aria-label="Filtrar itens">
            <button class="chip" id="abrir" aria-pressed="false">Abrir grupos</button><span class="count" id="cnt"></span></div>
            <div style="display:grid;gap:10px">{sections}</div></section>
            </div></main>
            """;
        var script = """
            (() => {
              let r = ""; const q = document.getElementById("q"), cnt = document.getElementById("cnt");
              function apply() {
                const t = q.value.trim().toLowerCase(); let n = 0;
                document.querySelectorAll(".grp").forEach(g => {
                  const okg = !r || g.dataset.r === r; let v = 0;
                  g.querySelectorAll(".fd").forEach(f => { const ok = okg && (!t || f.dataset.q.includes(t)); f.classList.toggle("hidden", !ok); if (ok) v++; });
                  g.classList.toggle("hidden", v === 0); n += v; if (t && v) g.open = true;
                });
                cnt.textContent = n + " itens";
              }
              document.querySelectorAll(".chip[data-r]").forEach(b => b.addEventListener("click", () => {
                r = b.dataset.r; document.querySelectorAll(".chip[data-r]").forEach(x => x.setAttribute("aria-pressed", x === b)); apply();
              }));
              q.addEventListener("input", apply);
              const ab = document.getElementById("abrir");
              ab.addEventListener("click", () => { const on = ab.getAttribute("aria-pressed") !== "true"; ab.setAttribute("aria-pressed", on);
                ab.textContent = on ? "Fechar grupos" : "Abrir grupos"; document.querySelectorAll(".grp:not(.hidden)").forEach(g => g.open = on); });
              apply();
            })();
            """;
        return Page($"Pendências ATC {input.Customer}", css, body,
            "Gerado pela skill atc-resultado a partir da sessão da atc-remediacao. Nada aqui foi lido do SAP na geração.", script);
    }

    // ---------------------------------------------------------------- MIGNOW

    public sealed record MignowRow(int Seq, string Rule, string ObjType, string ObjName, string? Package, string? Responsible, string AdtSource, int Start, int End,
        string Element, string Snippet, string Cross, string? Category, string? Severity, string? Description, string? RefinedPcre,
        IReadOnlyList<CodeLine>? Context);

    public sealed record MignowCoverage(string Element, int Pending, int AtcCaught, int Mignow, double CoveragePct);

    public sealed record MignowSummary(string? System, string? BaseSeries, string? BaseExecutedAt8, int BaseFindings, int ScopeObjects, int ReadSources, int ReadLines,
        int OccTotal, int OccInformative, int OccRemediated, int OccPending, int CrossAtcCaught, int CrossAtcOtherLine, IReadOnlyList<MignowCoverage> Coverage);

    public sealed record MignowInput(string Customer, string? ProjectType, string ScanName, MignowSummary Summary, IReadOnlyList<MignowRow> Rows, DateTimeOffset Now,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> RoleTexts);

    private static readonly Dictionary<string, (string Label, string Cls, string Text)> Cross = new()
    {
        ["MIGNOW_FONTE_COM_FINDING"] = ("ATC não pegou · fonte com outros findings", "p-warn", "O fonte tem findings do ATC, mas nenhum deste elemento."),
        ["MIGNOW_FONTE_SEM_FINDING"] = ("ATC não pegou · fonte sem findings", "p-bad", "O fonte não tem nenhum finding do ATC (o objeto pode estar fora do escopo da execução do ATC – não verificado)."),
        ["ATC_OUTRA_LINHA"] = ("ATC achou em outra linha", "p-info", "Mesmo fonte e elemento no ATC, outra linha."),
    };

    public static (string Main, IReadOnlyDictionary<string, string> Texts) RolesOf(string? category, MignowInput input, string rule)
    {
        var cat = category is not null && MignowRoleTexts.ByCategory.ContainsKey(category) ? category : "TABELA";
        var fixedRole = MignowRoleTexts.ByCategory[cat];
        var texts = input.RoleTexts.TryGetValue(cat, out var t) ? t : fixedRole.Texts;
        _ = rule;
        return (fixedRole.Main, texts);
    }

    public static string RenderMignow(MignowInput input, string css)
    {
        var res = input.Summary;
        var regs = input.Rows;
        var groups = regs.OrderBy(x => x.Category is not null && MignowRoleTexts.CategoryOrder.Contains(x.Category) ? MignowRoleTexts.CategoryOrder.IndexOf(x.Category) : 99)
            .ThenBy(x => x.Rule, StringComparer.Ordinal).ThenBy(x => x.ObjName, StringComparer.Ordinal).ThenBy(x => x.Start).GroupBy(x => x.Rule).ToList();
        var objs = regs.Select(x => (x.ObjType, x.ObjName)).Distinct().Count();
        var byCross = regs.GroupBy(x => x.Cross).OrderByDescending(g => g.Count()).ToList();
        string MainRole(MignowRow x) => RolesOf(x.Category, input, x.Rule).Main;
        var byRole = regs.GroupBy(MainRole).OrderByDescending(g => g.Count()).ToList();
        var withCode = regs.Count(x => x.Context is { Count: > 0 });
        var mismatch = regs.Count(x => Confere(x) == false);
        int semFinding = regs.Count(x => x.Cross == "MIGNOW_FONTE_SEM_FINDING");

        string Card(MignowRow x)
        {
            var cz = Cross.TryGetValue(x.Cross, out var c) ? c : (x.Cross, "p-info", "");
            string cod;
            if (x.Context is { Count: > 0 })
            {
                cod = $"<details class=\"sub\"><summary>Código atual · {E(x.AdtSource.Replace("/sap/bc/adt/", ""))} linhas {x.Start}–{x.End} <span class=\"muted\">(versão ativa lida via ADT)</span></summary><div class=\"code\"><pre>{CodeBlock(x.Context, k => k >= x.Start && k <= x.End)}</pre></div></details>";
                if (Confere(x) == false)
                    cod += "<p class=\"note\">A regra não bate nas linhas destacadas da versão lida (o fonte pode ter mudado depois da varredura).</p>";
            }
            else
                cod = "<p class=\"note\">Fonte não lido: a varredura não guardou o fonte desta ocorrência no cache.</p>";
            var search = string.Join(" ", new[] { x.ObjName, x.AdtSource, x.Element, x.Rule, x.Description, x.Package, x.Snippet, cz.Item1 }.Select(v => v ?? "None")).ToLowerInvariant();
            return $"""
                <details class="fd" data-c="{E(x.Cross)}" data-q="{E(search)}"><summary><span class="mono">#{x.Seq}</span> <b class="mono">{E(x.ObjName)}</b>
                <span class="muted mono">{E(x.AdtSource[(x.AdtSource.LastIndexOf('/') + 1)..])}:{x.Start}</span> <code>{E(x.Element)}</code>
                <span class="pill {cz.Item2}">{E(cz.Item1)}</span></summary>
                <div class="fd-b"><dl class="kv">
                <dt>Objeto</dt><dd>{E(x.ObjType)} {E(x.ObjName)} · pacote {E(string.IsNullOrEmpty(x.Package) ? "–" : x.Package)} · responsável {E(string.IsNullOrEmpty(x.Responsible) ? "–" : x.Responsible)}</dd>
                <dt>Regra</dt><dd>{E(x.Rule)} ({E(x.Severity ?? "–")}) – {E(x.Description ?? "")}</dd>
                <dt>Sentença encontrada</dt><dd><code>{E(x.Snippet)}</code></dd>
                <dt>Cruzamento com o ATC</dt><dd>{E(cz.Item3)}</dd>
                </dl>{cod}</div></details>
                """;
        }

        var sections = string.Concat(groups.Select(g =>
        {
            var first = g.First();
            var (main, texts) = RolesOf(first.Category, input, g.Key);
            var elems = string.Join(", ", g.Select(x => x.Element).Distinct().Order(StringComparer.Ordinal));
            var tab = string.Concat(texts.Select(kv => $"<tr><th>{E(kv.Key)}</th><td>{E(kv.Value)}</td></tr>"));
            return $"""
                <details class="grp" data-p="{E(main)}"><summary><span class="pill p-acc">{E(g.Key)}</span> <b>{E(elems)}</b> · {E(first.Category ?? "")}
                <span class="cnt mono">{g.Count()} ocorrências · {g.Select(x => x.ObjName).Distinct().Count()} objetos</span></summary>
                <div class="grp-b"><p class="note"><b>Regra:</b> {E(first.Description ?? "")}</p>
                <div class="tbl"><table class="papeis"><caption>O que cada papel analisa <span class="pill p-info">recomendação da IA</span></caption><tbody>{tab}</tbody></table></div>
                {string.Concat(g.Select(Card))}</div></details>
                """;
        }));
        var summary = string.Concat(groups.Select(g =>
            $"<tr><td><span class=\"pill p-acc\">{E(g.Key)}</span></td><td>{E(string.Join(", ", g.Select(x => x.Element).Distinct().Order(StringComparer.Ordinal)))}</td><td>{E(g.First().Category ?? "")}</td><td>{E(MainRole(g.First()))}</td>"
            + $"<td class=\"num\">{g.Count()}</td><td class=\"num\">{g.Select(x => x.ObjName).Distinct().Count()}</td><td class=\"num\">{g.Count(x => x.Cross == "MIGNOW_FONTE_SEM_FINDING")}</td></tr>"));
        var cov = string.Concat(res.Coverage.OrderByDescending(c => c.Mignow).ThenByDescending(c => c.Pending)
            .Select(c => $"<tr><td><code>{E(c.Element)}</code></td><td class=\"num\">{c.Pending}</td><td class=\"num\">{c.AtcCaught}</td><td class=\"num\">{c.Mignow}</td><td class=\"num\">{c.CoveragePct.ToString("0", CultureInfo.InvariantCulture)}%</td></tr>"));
        var chipsP = string.Concat(byRole.Select(g => $"<button class=\"chip\" data-f=\"p\" data-v=\"{E(g.Key)}\" aria-pressed=\"false\">{E(g.Key)} ({g.Count()})</button>"));
        var chipsC = string.Concat(byCross.Select(g => $"<button class=\"chip\" data-f=\"c\" data-v=\"{E(g.Key)}\" aria-pressed=\"false\">{E(Cross.TryGetValue(g.Key, out var c) ? c.Label : g.Key)} ({g.Count()})</button>"));
        var mismatchTxt = mismatch > 0 ? $"; em {mismatch} a regra não bate nas linhas da versão lida" : "";
        var body = $"""
            <header class="hero"><div class="wrap">
            <div class="eyebrow">Análise para decisão · Extração MIGNOW</div>
            <h1>O que o ATC não pegou no <span>{E(input.Customer)}</span></h1>
            <p class="lede">Ocorrências da varredura MIGNOW (regex refinadas sobre os fontes Z/Y) que <strong>não têm finding do ATC</strong>: cada uma com a regra, a sentença encontrada, o <strong>código atual</strong> e o que o <strong>consultor ABAP, o funcional e o cliente</strong> devem analisar.</p>
            <div class="meta"><span>Sistema <b>{E(res.System)}</b></span><span>Projeto <b>{E(string.IsNullOrEmpty(input.ProjectType) ? "–" : input.ProjectType)}</b></span>
            <span>Base ATC <b>{E(res.BaseSeries)} {E(res.BaseExecutedAt8)}</b> ({res.BaseFindings} findings)</span>
            <span>Varredura <b>{E(input.ScanName)}</b></span><span>Gerado <b>{input.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}</b></span></div>
            <div class="kpis">
            <div class="kpi acc"><span class="n">{regs.Count}</span><span class="l">ocorrências que o ATC não pegou</span></div>
            <div class="kpi"><span class="n">{objs}</span><span class="l">objetos</span></div>
            <div class="kpi"><span class="n">{semFinding}</span><span class="l">em fontes sem nenhum finding do ATC</span></div>
            <div class="kpi"><span class="n">{groups.Count}</span><span class="l">regras com ocorrência</span></div>
            <div class="kpi ok"><span class="n">{res.ScopeObjects}</span><span class="l">objetos Z/Y varridos</span></div>
            </div></div></header>
            <main><div class="wrap">
            <section><h2><small>Como ler</small>Da regra para a ocorrência</h2>
            <p>A varredura leu {res.ReadSources} fontes ({res.ReadLines} linhas) e achou {res.OccTotal} ocorrências: {res.OccInformative} informativas (sem o contexto exigido pela regra), {res.OccRemediated} já remediadas (bloco MIG-NOW) e {res.OccPending} pendentes. Das pendentes, o ATC já pegou {res.CrossAtcCaught} na mesma linha e {res.CrossAtcOtherLine} em outra linha; as {regs.Count} restantes são as deste relatório.</p>
            <p>Abra uma regra para ver o que cada papel analisa e as ocorrências; abra a ocorrência para ver o código (linhas da sentença destacadas). {withCode} de {regs.Count} ocorrências têm o código da versão ativa lida via ADT{mismatchTxt}. O texto "o que cada papel analisa" é <b>recomendação da IA</b> a partir da descrição da regra, não decisão registrada.</p></section>
            <section><h2><small>Resumo</small>Quem analisa o quê</h2>
            <div class="tbl"><table><thead><tr><th>Regra</th><th>Elemento</th><th>Categoria</th><th>Papel principal</th><th class="num">Ocorrências</th><th class="num">Objetos</th><th class="num">Fonte sem finding</th></tr></thead><tbody>{summary}</tbody>
            <tfoot><tr><td colspan="4">Total</td><td class="num">{regs.Count}</td><td class="num">{objs}</td><td class="num">{semFinding}</td></tr></tfoot></table></div></section>
            <section><h2><small>Cobertura</small>ATC × varredura por elemento</h2>
            <div class="tbl"><table><thead><tr><th>Elemento</th><th class="num">Pendentes</th><th class="num">ATC pegou</th><th class="num">ATC não pegou</th><th class="num">Cobertura do ATC</th></tr></thead><tbody>{cov}</tbody></table></div>
            <p class="note">Pendentes = ocorrências com o contexto da regra e fora de bloco MIG-NOW. "ATC pegou" = finding no mesmo fonte, mesmo elemento e linha dentro da sentença.</p></section>
            <section><h2><small>Detalhe</small>Ocorrências por regra</h2>
            <div class="toolbar"><button class="chip" data-f="p" data-v="" aria-pressed="true">Todos</button>{chipsP}
            <button class="chip" data-f="c" data-v="" aria-pressed="true">Qualquer cruzamento</button>{chipsC}
            <input type="search" id="q" placeholder="Objeto, fonte, elemento, pacote, sentença…" aria-label="Filtrar ocorrências">
            <button class="chip" id="abrir" aria-pressed="false">Abrir grupos</button><span class="count" id="cnt"></span></div>
            <div style="display:grid;gap:10px">{sections}</div></section>
            </div></main>
            """;
        var script = """
            (() => {
              const f = { p: "", c: "" }; const q = document.getElementById("q"), cnt = document.getElementById("cnt");
              function apply() {
                const t = q.value.trim().toLowerCase(); let n = 0;
                document.querySelectorAll(".grp").forEach(g => {
                  const okg = !f.p || g.dataset.p === f.p; let v = 0;
                  g.querySelectorAll(".fd").forEach(x => { const ok = okg && (!f.c || x.dataset.c === f.c) && (!t || x.dataset.q.includes(t)); x.classList.toggle("hidden", !ok); if (ok) v++; });
                  g.classList.toggle("hidden", v === 0); n += v; if (t && v) g.open = true;
                });
                cnt.textContent = n + " ocorrências";
              }
              document.querySelectorAll(".chip[data-f]").forEach(b => b.addEventListener("click", () => {
                f[b.dataset.f] = b.dataset.v;
                document.querySelectorAll(`.chip[data-f="${b.dataset.f}"]`).forEach(x => x.setAttribute("aria-pressed", x === b)); apply();
              }));
              q.addEventListener("input", apply);
              const ab = document.getElementById("abrir");
              ab.addEventListener("click", () => { const on = ab.getAttribute("aria-pressed") !== "true"; ab.setAttribute("aria-pressed", on);
                ab.textContent = on ? "Fechar grupos" : "Abrir grupos"; document.querySelectorAll(".grp:not(.hidden)").forEach(g => g.open = on); });
              apply();
            })();
            """;
        return Page($"Extração MIGNOW {input.Customer}", css, body,
            $"Gerado pela skill atc-resultado a partir da varredura {E(input.ScanName)} da atc-extracao-mignow (código lido via ADT, só leitura). Nada aqui foi lido do SAP na geração.", script);
    }

    /// <summary>A regex refinada bate nas linhas destacadas? Nulo = sem como conferir (sem código ou sem regex).</summary>
    public static bool? Confere(MignowRow x)
    {
        if (x.Context is not { Count: > 0 })
            return null;
        var target = string.Join(" ", x.Context.Where(l => l.Line >= x.Start && l.Line <= x.End).Select(l => l.Text)).ToUpperInvariant();
        if (string.IsNullOrEmpty(x.RefinedPcre))
            return x.Element.ToUpperInvariant().Length > 0 && target.Contains(x.Element.ToUpperInvariant(), StringComparison.Ordinal);
        try
        {
            return Regex.IsMatch(target, x.RefinedPcre, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
        }
        catch (Exception e) when (e is ArgumentException or RegexMatchTimeoutException)
        {
            return target.Contains(x.Element.ToUpperInvariant(), StringComparison.Ordinal);
        }
    }

    public static IReadOnlyList<CodeLine>? Window(IReadOnlyList<string>? lines, int from, int to, int context)
    {
        if (lines is null || lines.Count == 0 || from <= 0)
            return null;
        var a = Math.Max(1, from - context);
        var b = Math.Min(lines.Count, to + context);
        return [.. Enumerable.Range(a, Math.Max(0, b - a + 1)).Select(k => new CodeLine(k, lines[k - 1]))];
    }

    private static int IndexOf(this IReadOnlyList<string> l, string v) { for (var i = 0; i < l.Count; i++) if (l[i] == v) return i; return -1; }
}
