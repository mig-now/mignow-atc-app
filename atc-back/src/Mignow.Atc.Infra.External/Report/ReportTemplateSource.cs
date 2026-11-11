namespace Mignow.Atc.Infra.External.Report;

/// <summary>
/// Cópia do modelo HTML da skill atc-resultado (<c>.claude/skills/atc-resultado/modelo/relatorio.html</c>, sha256 33bc5645502bb19d...),
/// embutida como constante para não depender do caminho da skill em tempo de execução. Marcadores: <c>{{TITULO}}</c>,
/// <c>&lt;!--CORPO--&gt;</c> e <c>/*DADOS*/null</c>. Mudou o modelo da skill: regere este arquivo.
/// </summary>
internal static class ReportTemplateSource
{
    public const string Html = """""
<title>{{TITULO}}</title>
<meta name="claude:viewport-fit-sides" content="cover">
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Michroma&family=Jost:wght@300;400;500;600&family=JetBrains+Mono:wght@400;500&display=swap">
<style>
/* Layout: relatório de missão MIGNOW (skill atc-resultado) - abertura escura com os números do ciclo, seções em coluna de leitura, burndown, lista e custos/tempos no fechamento */
:root {
  --bg: #1e1f20; --surface: #26272a; --surface-2: #2e2f33; --line: #3a3b40;
  --fg: #ffffff; --muted: #a9abb3;
  --accent: #a145ff; --accent-soft: rgba(161, 69, 255, .16);
  --s-kb: #a145ff; --s-ia: #1396dc; --s-is: #c4821c; --s-pend: #4a4b52;
  --ok: #60d360; --ok-soft: rgba(96, 211, 96, .14);
  --warn: #f2b84b; --warn-soft: rgba(242, 184, 75, .14);
  --bad: #ed5d64; --bad-soft: rgba(237, 93, 100, .14);
  --info: #01a9fb; --info-soft: rgba(1, 169, 251, .14);
  --display: "Michroma", "Eurostile", "Arial Black", sans-serif;
  --body: "Jost", "Geometria", "Futura", "Century Gothic", Arial, sans-serif;
  --mono: "JetBrains Mono", Consolas, "Courier New", monospace;
  color-scheme: dark;
}
@media (prefers-color-scheme: light) { :root:not([data-theme="dark"]) {
  --bg: #f2f2f2; --surface: #ffffff; --surface-2: #f7f6fa; --line: #dedde4;
  --fg: #1e1f20; --muted: #5d5e66;
  --accent: #8a2be2; --accent-soft: rgba(138, 43, 226, .10);
  --s-kb: #8a2be2; --s-ia: #0083c4; --s-is: #b0741a; --s-pend: #d3d2da;
  --ok: #2e9b3a; --ok-soft: rgba(46, 155, 58, .12);
  --warn: #9a6512; --warn-soft: rgba(183, 121, 31, .12);
  --bad: #cf3d45; --bad-soft: rgba(207, 61, 69, .10);
  --info: #0083c4; --info-soft: rgba(0, 131, 196, .10);
  color-scheme: light;
} }
:root[data-theme="light"] {
  --bg: #f2f2f2; --surface: #ffffff; --surface-2: #f7f6fa; --line: #dedde4;
  --fg: #1e1f20; --muted: #5d5e66;
  --accent: #8a2be2; --accent-soft: rgba(138, 43, 226, .10);
  --s-kb: #8a2be2; --s-ia: #0083c4; --s-is: #b0741a; --s-pend: #d3d2da;
  --ok: #2e9b3a; --ok-soft: rgba(46, 155, 58, .12);
  --warn: #9a6512; --warn-soft: rgba(183, 121, 31, .12);
  --bad: #cf3d45; --bad-soft: rgba(207, 61, 69, .10);
  --info: #0083c4; --info-soft: rgba(0, 131, 196, .10);
  color-scheme: light;
}
* { box-sizing: border-box; }
body { background: var(--bg); color: var(--fg); font-family: var(--body); font-size: 16px; line-height: 1.6; }
.wrap { max-width: 1080px; margin: 0 auto; padding-left: max(16px, env(safe-area-inset-left, 0px)); padding-right: max(16px, env(safe-area-inset-right, 0px)); }
header.hero { border-bottom: 1px solid var(--line); background: radial-gradient(900px 340px at 88% -40%, var(--accent-soft), transparent 70%), var(--bg); }
header.hero .wrap { padding-block: 48px 36px; display: grid; gap: 20px; }
.eyebrow { font-family: var(--mono); font-size: 12px; letter-spacing: .14em; text-transform: uppercase; color: var(--accent); }
.visao { display: inline-flex; gap: 6px; flex-wrap: wrap; }
.visao a, .visao span { font-family: var(--mono); font-size: 12px; letter-spacing: .06em; padding: 3px 11px; border-radius: 999px; border: 1px solid var(--line); color: var(--muted); text-decoration: none; }
.visao span { border-color: var(--accent); background: var(--accent-soft); color: var(--fg); }
h1 { font-family: var(--display); font-weight: 400; font-size: clamp(26px, 4.6vw, 44px); line-height: 1.15; margin: 0; text-wrap: balance; }
h1 span { color: var(--accent); }
.lede { max-width: 70ch; color: var(--muted); font-size: 18px; font-weight: 300; margin: 0; }
.lede strong { color: var(--fg); font-weight: 500; }
.meta { display: flex; flex-wrap: wrap; gap: 8px 22px; font-family: var(--mono); font-size: 13px; color: var(--muted); }
.meta b { color: var(--fg); font-weight: 500; }
.kpis { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 1px; background: var(--line); border: 1px solid var(--line); border-radius: 10px; overflow: hidden; }
.kpi { background: var(--surface); padding: 18px 18px; display: grid; gap: 4px; align-content: start; }
.kpi .n { font-family: var(--display); font-size: clamp(24px, 3.4vw, 34px); line-height: 1.1; font-variant-numeric: tabular-nums; }
.kpi .l { font-size: 14px; color: var(--muted); line-height: 1.4; }
.kpi.ok .n { color: var(--ok); } .kpi.acc .n { color: var(--accent); }
main .wrap { padding-block: 16px 64px; display: grid; gap: 56px; }
section { display: grid; gap: 16px; min-width: 0; }
h2 { font-family: var(--display); font-weight: 400; font-size: clamp(18px, 2.4vw, 22px); margin: 0; text-wrap: balance; }
h2 small { display: block; font-family: var(--mono); font-size: 12px; letter-spacing: .14em; text-transform: uppercase; color: var(--accent); margin-bottom: 6px; }
h3 { margin: 0; font-size: 16px; font-weight: 600; }
p { margin: 0; max-width: 74ch; }
.muted { color: var(--muted); }
code, .mono { font-family: var(--mono); font-size: .86em; }
code { background: var(--surface-2); border: 1px solid var(--line); border-radius: 4px; padding: 1px 5px; overflow-wrap: anywhere; }
b { font-weight: 600; }
.tbl { overflow-x: auto; border: 1px solid var(--line); border-radius: 10px; background: var(--surface); }
table { width: 100%; border-collapse: collapse; font-size: 14.5px; }
th, td { text-align: left; padding: 10px 14px; border-bottom: 1px solid var(--line); vertical-align: top; }
th { font-family: var(--mono); font-size: 11.5px; letter-spacing: .1em; text-transform: uppercase; color: var(--muted); font-weight: 500; background: var(--surface-2); white-space: nowrap; }
tr:last-child td { border-bottom: 0; }
.num { text-align: right; font-variant-numeric: tabular-nums; white-space: nowrap; }
tfoot td { font-weight: 600; background: var(--surface-2); }
.pill { display: inline-flex; align-items: center; gap: 6px; font-family: var(--mono); font-size: 11.5px; letter-spacing: .03em; padding: 2px 9px; border-radius: 999px; white-space: nowrap; }
.pill::before { content: ""; width: 6px; height: 6px; border-radius: 50%; background: currentColor; }
.p-ok { color: var(--ok); background: var(--ok-soft); }
.p-warn { color: var(--warn); background: var(--warn-soft); }
.p-bad { color: var(--bad); background: var(--bad-soft); }
.p-info { color: var(--info); background: var(--info-soft); }
.p-acc { color: var(--accent); background: var(--accent-soft); }
.p-mut { color: var(--muted); background: var(--surface-2); }
.grid2 { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.grid3 { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 16px; }
.panel { background: var(--surface); border: 1px solid var(--line); border-radius: 10px; padding: 18px 20px; display: grid; gap: 10px; align-content: start; min-width: 0; }
.panel .big { font-family: var(--display); font-size: 28px; line-height: 1.1; font-variant-numeric: tabular-nums; }
dl.kv { display: grid; grid-template-columns: max-content minmax(0, 1fr); gap: 8px 18px; margin: 0; font-size: 14.5px; }
dl.kv dt { color: var(--muted); }
dl.kv dd { margin: 0; min-width: 0; overflow-wrap: anywhere; }
.chart { background: var(--surface); border: 1px solid var(--line); border-radius: 10px; padding: 16px 16px 8px; position: relative; min-width: 0; }
.chart svg { display: block; width: 100%; height: auto; overflow: visible; }
.chart .ax { fill: var(--muted); font-family: var(--mono); font-size: 11px; }
.chart .lbl { fill: var(--fg); font-family: var(--mono); font-size: 12px; paint-order: stroke; stroke: var(--surface); stroke-width: 5px; stroke-linejoin: round; }
.chart .grid { stroke: var(--line); stroke-width: 1; }
.legend { display: flex; flex-wrap: wrap; gap: 6px 20px; font-size: 14px; color: var(--muted); padding: 4px 4px 8px; }
.legend .sw { display: inline-block; width: 22px; height: 0; border-top: 2px solid var(--c); margin-right: 7px; vertical-align: 4px; }
.legend .sw.dash { border-top-style: dashed; }
.legend .sq { display: inline-block; width: 12px; height: 12px; border-radius: 3px; margin-right: 7px; background: var(--c); vertical-align: -1px; }
.tip { position: absolute; pointer-events: none; background: var(--bg); border: 1px solid var(--line); border-radius: 8px; padding: 10px 12px; font-size: 13px; min-width: 200px; box-shadow: 0 6px 24px rgba(0,0,0,.25); display: grid; gap: 3px; }
.tip[hidden] { display: none; }
.tip b { font-family: var(--mono); font-weight: 500; font-size: 12px; color: var(--muted); }
.tip div { display: flex; justify-content: space-between; gap: 16px; font-variant-numeric: tabular-nums; }
details { border: 1px solid var(--line); border-radius: 10px; background: var(--surface); }
details > summary { cursor: pointer; padding: 10px 14px; font-size: 14px; color: var(--muted); }
details[open] > summary { border-bottom: 1px solid var(--line); }
details .tbl { border: 0; border-radius: 0 0 10px 10px; }
.stack { display: flex; height: 14px; border-radius: 7px; overflow: hidden; gap: 2px; background: var(--surface); }
.stack i { display: block; height: 100%; }
ol.steps, ul.list { margin: 0; padding-left: 22px; display: grid; gap: 10px; max-width: 78ch; }
ol.steps li::marker { font-family: var(--mono); color: var(--accent); }
ul.list li::marker { color: var(--accent); }
.toolbar { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; }
.chip { font: inherit; font-size: 13.5px; color: var(--fg); background: var(--surface); border: 1px solid var(--line); border-radius: 999px; padding: 5px 13px; cursor: pointer; }
.chip[aria-pressed="true"] { border-color: var(--accent); background: var(--accent-soft); }
.chip:focus-visible, input:focus-visible, summary:focus-visible, .visao a:focus-visible { outline: 2px solid var(--accent); outline-offset: 2px; }
input[type="search"] { font: inherit; font-size: 14px; color: var(--fg); background: var(--surface); border: 1px solid var(--line); border-radius: 8px; padding: 6px 12px; min-width: 0; flex: 1 1 220px; max-width: 340px; }
.count { font-family: var(--mono); font-size: 13px; color: var(--muted); margin-left: auto; }
#lista td.obs { color: var(--muted); font-size: 13.5px; min-width: 300px; max-width: 480px; }
#lista td.obj { font-family: var(--mono); font-size: 13px; white-space: nowrap; }
.cmp { display: grid; grid-template-columns: max-content minmax(0, 1fr) max-content; gap: 12px 16px; align-items: center; background: var(--surface); border: 1px solid var(--line); border-radius: 10px; padding: 20px; }
.cmp-l { font-size: 14px; color: var(--muted); }
.cmp-t { height: 22px; min-width: 0; }
.cmp-t i { display: block; height: 100%; border-radius: 0 4px 4px 0; }
.cmp-v { font-family: var(--mono); font-size: 13px; font-variant-numeric: tabular-nums; white-space: nowrap; }
@media (max-width: 700px) { .cmp { grid-template-columns: minmax(0, 1fr); gap: 4px; } .cmp-v { margin-bottom: 10px; } }
.cost { border-top: 1px solid var(--line); padding-top: 40px; }
.note { font-size: 13.5px; color: var(--muted); max-width: 90ch; }
footer { border-top: 1px solid var(--line); }
footer .wrap { padding-block: 22px 32px; font-size: 13px; color: var(--muted); display: flex; flex-wrap: wrap; gap: 6px 22px; justify-content: space-between; }
.brand { font-family: var(--display); letter-spacing: .08em; color: var(--fg); }
@media (max-width: 860px) { .kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); } .kpis .kpi:first-child { grid-column: 1 / -1; } .grid3 { grid-template-columns: minmax(0, 1fr); } }
@media (max-width: 700px) { .grid2 { grid-template-columns: minmax(0, 1fr); } header.hero .wrap { padding-block: 32px 28px; } }
@media (prefers-reduced-motion: no-preference) { .chip { transition: background .15s, border-color .15s; } }
</style>

<!--CORPO-->

<script>
const D = /*DADOS*/null;
const NS = 'http://www.w3.org/2000/svg';
const esc = s => String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));

// burndown: pendentes ao longo do tempo (escala de tempo real) contra a linha ideal
(function () {
  const P = D.burndown, svg = document.getElementById('bd'), tip = document.getElementById('tip'), box = document.getElementById('chart');
  if (!svg || !P.length) return;
  // vários ciclos (reexecução do ATC): escala pelo maior ciclo; ideal e "concluído" por ciclo
  const W = 960, H = 360, L = 48, R = 20, T = 36, B = 34, tot = Math.max(...P.map(p => p.tot || p.pend), P[0].pend) || D.total;
  const totDe = p => p.tot || tot, multi = P.some(p => p.ciclo !== P[0].ciclo);
  const t0 = P[0].t, t1 = P[P.length - 1].t > t0 ? P[P.length - 1].t : t0 + 1;
  const x = t => L + (t - t0) / (t1 - t0) * (W - L - R);
  const y = v => T + (1 - v / tot) * (H - T - B);
  const el = (n, a) => { const e = document.createElementNS(NS, n); for (const k in a) e.setAttribute(k, a[k]); return e; };
  const txt = (s, a) => { const e = el('text', a); e.textContent = s; svg.appendChild(e); return e; };
  const passo = tot > 300 ? 100 : tot > 120 ? 50 : tot > 40 ? 20 : 10;
  for (let v = 0; v < tot - passo * 0.4; v += passo) { svg.appendChild(el('line', { x1: L, x2: W - R, y1: y(v), y2: y(v), class: 'grid' })); txt(v, { x: L - 8, y: y(v) + 4, 'text-anchor': 'end', class: 'ax' }); }
  svg.appendChild(el('line', { x1: L, x2: W - R, y1: y(tot), y2: y(tot), class: 'grid' })); txt(tot, { x: L - 8, y: y(tot) + 4, 'text-anchor': 'end', class: 'ax' });
  // marcas de tempo: a cada 6 h (ou 1 dia em ciclos longos), alinhadas à hora local
  const dur = (t1 - t0) / 3600000, step = (dur > 96 ? 24 : dur > 36 ? 12 : 6) * 3600000;
  let tk = new Date(t0); tk.setMinutes(0, 0, 0); tk.setHours(Math.ceil(tk.getHours() / (step / 3600000)) * (step / 3600000));
  for (let t = tk.getTime(); t <= t1; t += step) {
    if (t < t0) continue;
    const dd = new Date(t), r = `${String(dd.getDate()).padStart(2, '0')}.${String(dd.getMonth() + 1).padStart(2, '0')} ${String(dd.getHours()).padStart(2, '0')}h`;
    txt(r, { x: x(t), y: H - 10, 'text-anchor': 'middle', class: 'ax' });
  }
  // área e linha real (degraus: o pendente só muda no fechamento de cada lote)
  let d = `M${x(P[0].t)},${y(P[0].pend)}`;
  for (let i = 1; i < P.length; i++) d += `H${x(P[i].t)}V${y(P[i].pend)}`;
  svg.appendChild(el('path', { d: d + `V${y(0)}H${x(P[0].t)}Z`, fill: 'var(--accent-soft)' }));
  [...new Set(P.map(p => p.ciclo))].forEach((c, i) => {
    const pc = P.filter(p => p.ciclo === c), a = pc[0], z = pc[pc.length - 1];
    if (z.t > a.t) svg.appendChild(el('line', { x1: x(a.t), y1: y(totDe(a)), x2: x(z.t), y2: y(0), stroke: 'var(--muted)', 'stroke-width': 2, 'stroke-dasharray': '6 5' }));
    if (multi && i) {
      svg.appendChild(el('line', { x1: x(a.t), x2: x(a.t), y1: T, y2: H - B, stroke: 'var(--muted)', 'stroke-width': 1 }));
      txt(`Ciclo ${c} · reexecução do ATC`, { x: x(a.t) - 6, y: T + 2, 'text-anchor': 'end', class: 'ax' });
    }
  });
  svg.appendChild(el('path', { d, fill: 'none', stroke: 'var(--accent)', 'stroke-width': 2.5, 'stroke-linejoin': 'round' }));
  P.forEach(p => svg.appendChild(el('circle', { cx: x(p.t), cy: y(p.pend), r: 4.5, fill: 'var(--accent)', stroke: 'var(--surface)', 'stroke-width': 2 })));
  // rótulos de início e fim acima da linha, fora dos degraus
  txt(`${tot} ${D.unidade}`, { x: x(t0), y: y(tot) - 12, class: 'lbl' });
  const u = P[P.length - 1], uAnt = P.length > 1 ? P[P.length - 2].pend : u.pend;
  txt(`${u.pend} pendentes`, { x: x(u.t), y: Math.min(y(u.pend), y(uAnt)) - 24, 'text-anchor': 'end', class: 'lbl' });
  const cross = el('line', { y1: T, y2: H - B, stroke: 'var(--muted)', 'stroke-width': 1, 'stroke-dasharray': '3 3', visibility: 'hidden' }); svg.appendChild(cross);
  const hit = el('rect', { x: L, y: T, width: W - L - R, height: H - T - B, fill: 'transparent' }); svg.appendChild(hit);
  function mostrar(ev) {
    const r = svg.getBoundingClientRect(), px = (ev.clientX - r.left) / r.width * W;
    let k = 0; P.forEach((p, i) => { if (Math.abs(x(p.t) - px) < Math.abs(x(P[k].t) - px)) k = i; });
    const p = P[k], ant = k ? P[k - 1].pend : p.pend, novo = k && P[k - 1].ciclo !== p.ciclo;
    cross.setAttribute('x1', x(p.t)); cross.setAttribute('x2', x(p.t)); cross.setAttribute('visibility', 'visible');
    tip.innerHTML = `<b>${esc(p.rot)}${multi ? ' · ciclo ' + p.ciclo : ''}</b><div><span>Pendentes</span><span>${p.pend}</span></div><div><span>${novo ? 'Novo ciclo' : 'Resolvidos no lote'}</span><span>${novo ? '+' + p.pend : ant - p.pend}</span></div><div><span>Concluído${multi ? ' no ciclo' : ''}</span><span>${Math.round((totDe(p) - p.pend) / totDe(p) * 100)}%</span></div>`;
    tip.hidden = false;
    const bx = box.getBoundingClientRect(), cx = x(p.t) / W * r.width + (r.left - bx.left), w = tip.offsetWidth;
    tip.style.left = Math.max(8, Math.min(bx.width - w - 8, cx + 14 > bx.width - w - 8 ? cx - w - 14 : cx + 14)) + 'px';
    tip.style.top = '52px';
  }
  hit.addEventListener('pointermove', mostrar);
  hit.addEventListener('pointerleave', () => { tip.hidden = true; cross.setAttribute('visibility', 'hidden'); });
})();

// lista detalhada com filtro e busca
(function () {
  const corpo = document.querySelector('#lista tbody'); if (!corpo) return;
  const pill = (t, c) => `<span class="pill ${c}">${esc(t)}</span>`;
  const solP = { 'KB': 'p-acc', 'Análise': 'p-info', 'Isenção': 'p-warn', 'Pendente': 'p-bad', 'Cancelado': 'p-mut',
    'Corrigido': 'p-info', 'Corrigido + isenção': 'p-acc', 'Isenção pedida': 'p-ok' };
  const sis = s => s === 'ATIVADO' ? pill('Ativado', 'p-ok') : s === 'PEDIDO' ? pill('Isenção pedida', 'p-ok')
    : s === 'SEM_ESCOPO' ? pill('Sem escopo', 'p-warn') : s ? pill(s, 'p-mut') : '<span class="muted">–</span>';
  const linha = D.visao === 'objetos'
    ? r => `<tr><td class="obj">${esc(r.obj)}<br><span class="muted">${esc(r.tipo)}</span></td><td class="num">${r.n}</td><td class="num">${r.kb}</td><td class="num">${r.ia}</td><td class="num">${r.is}</td><td class="num">${r.pend}</td><td>${pill(r.cat, solP[r.cat] || 'p-mut')}</td><td>${r.req.length ? pill('Ativado', 'p-ok') + '<br><span class="muted mono">' + esc(r.req.join(', ')) + '</span>' : (r.isp ? pill('Isenção pedida', 'p-ok') : '<span class="muted">–</span>')}</td><td class="obs">${esc(r.checks)}</td></tr>`
    : r => `<tr><td class="num">${r.n}</td><td class="obj">${esc(r.obj)}<br><span class="muted">${esc(r.tipo)}</span></td><td>${esc(r.check)}<br><span class="muted">${esc(r.msg)}</span></td><td class="mono">${esc(r.ref) || '<span class="muted">–</span>'}</td><td>${pill(r.sol, solP[r.sol] || 'p-mut')}</td><td>${sis(r.sis)}${r.req ? '<br><span class="muted mono">' + esc(r.req) + '</span>' : ''}</td><td class="obs">${esc(r.obs)}</td></tr>`;
  let filtro = '';
  function desenhar() {
    const q = document.getElementById('busca').value.trim().toLowerCase();
    const ls = D.linhas.filter(r => (!filtro || r.f.includes(filtro)) && (!q || r.busca.includes(q)));
    corpo.innerHTML = ls.map(linha).join('');
    document.getElementById('cont').textContent = `${ls.length} de ${D.linhas.length}`;
  }
  document.querySelectorAll('.chip').forEach(b => b.addEventListener('click', () => {
    filtro = b.dataset.f; document.querySelectorAll('.chip').forEach(x => x.setAttribute('aria-pressed', String(x === b))); desenhar();
  }));
  document.getElementById('busca').addEventListener('input', desenhar);
  desenhar();
})();
</script>
""""";
}
