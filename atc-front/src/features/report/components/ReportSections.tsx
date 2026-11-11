import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import type { ReportData } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { EmptyState } from "@/features/shared/empty-state";
import { formatNumber } from "@/lib/format";
import { formatSeconds } from "../lib/duration";
import { Legend, LineChart, type Series, StackedBars } from "./charts";

const FINDING_SERIES: (Series & { i18n: string })[] = [
  { key: "KB", i18n: "KB", label: "", fill: "fill-success", stroke: "" },
  { key: "Análise", i18n: "Analysis", label: "", fill: "fill-info", stroke: "" },
  { key: "Isenção", i18n: "Exemption", label: "", fill: "fill-warning", stroke: "" },
  { key: "Cancelado", i18n: "Cancelled", label: "", fill: "fill-muted-foreground", stroke: "" },
  { key: "Pendente", i18n: "Pending", label: "", fill: "fill-destructive", stroke: "" },
];
const SWATCH: Record<string, string> = {
  KB: "bg-success",
  Análise: "bg-info",
  Isenção: "bg-warning",
  Cancelado: "bg-muted-foreground",
  Pendente: "bg-destructive",
};

function Stat({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="text-lg font-semibold tabular-nums">{value}</dd>
      {hint ? <dd className="text-xs text-muted-foreground">{hint}</dd> : null}
    </div>
  );
}

function Source({ text }: { text: string | string[] }) {
  const { t } = useTranslation("report");
  const value = Array.isArray(text) ? text.join("; ") : text;
  return <p className="text-xs text-muted-foreground">{t("source", { value })}</p>;
}

/** Cartões de abertura: cliente, extração, ciclos, objetos e avisos de validação. */
export function SummarySection({ data }: { data: ReportData }) {
  const { t, i18n } = useTranslation("report");
  const lang = i18n.language;
  const n = (v: number | null | undefined) => (v == null ? "—" : formatNumber(v, lang));
  const findings = data.ciclos.reduce((s, c) => s + c.findings, 0);
  const last = data.evolucao.pontos.at(-1);
  const treated = last ? last.total - last.Pendente : null;
  return (
    <div className="space-y-4">
      {data.validacao.ok && data.validacao.avisos.length === 0 ? (
        <Alert tone="info">{t("validation.ok")}</Alert>
      ) : (
        <Alert tone="warning" title={t("validation.warnings")}>
          <ul className="list-disc pl-5">
            {data.validacao.avisos.map((a) => (
              <li key={a}>{a}</li>
            ))}
          </ul>
        </Alert>
      )}
      <Card>
        <CardHeader>
          <CardTitle>{data.cliente.nome}</CardTitle>
          <span className="text-xs text-muted-foreground">
            {[
              data.cliente.sid && `${data.cliente.sid}/${data.cliente.mandante ?? ""}`,
              data.cliente.release,
              data.cliente.papel,
            ]
              .filter(Boolean)
              .join(" · ")}
          </span>
        </CardHeader>
        <CardContent className="space-y-3">
          <dl className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <Stat label={t("summary.cycles")} value={n(data.ciclos.length)} />
            <Stat label={t("summary.findings")} value={n(findings)} />
            <Stat label={t("summary.objects")} value={n(data.objetos_total)} />
            <Stat
              label={t("summary.treated")}
              value={treated == null ? "—" : n(treated)}
              hint={
                last && last.total > 0 && treated != null ? `${Math.round((treated / last.total) * 100)}%` : undefined
              }
            />
          </dl>
          <p className="text-sm text-muted-foreground">
            {t("summary.scope", { scope: data.escopo.join(", ") || "—" })}
            {data.cliente.request ? ` · ${t("summary.request", { request: data.cliente.request })}` : ""}
          </p>
          {data.extracao ? (
            <dl className="grid grid-cols-2 gap-4 border-t border-border pt-3 md:grid-cols-5">
              <Stat
                label={t("extraction.series")}
                value={data.extracao.serie ?? "—"}
                hint={data.extracao.display_id ?? undefined}
              />
              <Stat label={t("extraction.executed")} value={data.extracao.executada ?? "—"} />
              <Stat label={t("extraction.total")} value={n(data.extracao.total)} />
              <Stat label={t("extraction.baseline")} value={n(data.extracao.em_baseline)} />
              <Stat
                label={t("extraction.new")}
                value={n(data.extracao.com_baseline)}
                hint={`P1 ${n(data.extracao.p1)} · P2 ${n(data.extracao.p2)} · P3 ${n(data.extracao.p3)}`}
              />
            </dl>
          ) : null}
        </CardContent>
      </Card>
    </div>
  );
}

/** Burndown por findings (barras empilhadas por situação) e por objetos (linhas). */
export function EvolutionSection({ data }: { data: ReportData }) {
  const { t, i18n } = useTranslation("report");
  const fmt = (v: number) => formatNumber(v, i18n.language);
  const points = data.evolucao.pontos;
  const series = FINDING_SERIES.map((s) => ({ ...s, label: t(`evolution.${s.i18n}`) }));
  if (points.length === 0) return <EmptyState text={t("evolution.empty")} />;
  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("evolution.findingsTitle")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          <StackedBars points={points} series={series} label={t("evolution.findingsTitle")} fmt={fmt} />
          <Legend items={series.map((s) => ({ key: s.key, label: s.label, swatch: SWATCH[s.key] ?? "" }))} />
          <Source text={data.evolucao.fonte} />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>{t("evolution.objectsTitle")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          <LineChart
            points={points}
            label={t("evolution.objectsTitle")}
            fmt={fmt}
            series={[
              { key: "total_objetos", label: t("evolution.totalObjects"), stroke: "stroke-info" },
              { key: "objetos_pendentes", label: t("evolution.pendingObjects"), stroke: "stroke-destructive" },
            ]}
          />
          <Legend
            items={[
              { key: "t", label: t("evolution.totalObjects"), swatch: "bg-info" },
              { key: "p", label: t("evolution.pendingObjects"), swatch: "bg-destructive" },
            ]}
          />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>{t("evolution.table")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>#</Th>
                  <Th>{t("evolution.moment")}</Th>
                  <Th>{t("evolution.cycle")}</Th>
                  {series.map((s) => (
                    <Th key={s.key} className="text-right">
                      {s.label}
                    </Th>
                  ))}
                  <Th className="text-right">{t("evolution.pendingObjects")}</Th>
                </Tr>
              </THead>
              <TBody>
                {points.map((p, i) => (
                  <Tr key={`${p.t}-${p.rot}`}>
                    <Td>{i + 1}</Td>
                    <Td>
                      {p.rot}
                      {p.reconstruido ? <Badge className="ml-2">{t("evolution.rebuilt")}</Badge> : null}
                    </Td>
                    <Td>{p.ciclo}</Td>
                    {series.map((s) => (
                      <Td key={s.key} className="text-right tabular-nums">
                        {fmt(Number((p as unknown as Record<string, number>)[s.key] ?? 0))}
                      </Td>
                    ))}
                    <Td className="text-right tabular-nums">
                      {p.objetos_pendentes == null ? "—" : fmt(p.objetos_pendentes)}
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

/** Ciclos: findings, objetos, por solução e já tratados antes. */
export function CyclesSection({ data }: { data: ReportData }) {
  const { t, i18n } = useTranslation("report");
  const fmt = (v: number) => formatNumber(v, i18n.language);
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("cycles.title")}</CardTitle>
      </CardHeader>
      <CardContent>
        {data.ciclos.length === 0 ? (
          <EmptyState text={t("cycles.empty")} />
        ) : (
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>{t("cycles.cycle")}</Th>
                  <Th>{t("cycles.scope")}</Th>
                  <Th className="text-right">{t("cycles.findings")}</Th>
                  <Th className="text-right">{t("cycles.objects")}</Th>
                  <Th>{t("cycles.bySolution")}</Th>
                  <Th className="text-right">{t("cycles.alreadyTreated")}</Th>
                </Tr>
              </THead>
              <TBody>
                {data.ciclos.map((c) => (
                  <Tr key={c.ciclo}>
                    <Td>{c.ciclo}</Td>
                    <Td>{c.escopo.join(", ")}</Td>
                    <Td className="text-right tabular-nums">{fmt(c.findings)}</Td>
                    <Td className="text-right tabular-nums">{fmt(c.objetos)}</Td>
                    <Td>
                      {Object.entries(c.por_solucao).map(([k, v]) => (
                        <Badge key={k} className="mr-1">
                          {k}: {fmt(v)}
                        </Badge>
                      ))}
                    </Td>
                    <Td className="text-right tabular-nums">{fmt(c.ja_tratados_antes.qtde)}</Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

/** Check x solução: contagem dos itens do relatório (calculada dos itens devolvidos pela API). */
export function CheckSolutionSection({ data }: { data: ReportData }) {
  const { t, i18n } = useTranslation("report");
  const fmt = (v: number) => formatNumber(v, i18n.language);
  const pivot = useMemo(() => {
    const cols = new Map<string, number>();
    const rows = new Map<string, Map<string, number>>();
    for (const it of data.itens) {
      cols.set(it.sol, (cols.get(it.sol) ?? 0) + 1);
      const row = rows.get(it.check) ?? new Map<string, number>();
      row.set(it.sol, (row.get(it.sol) ?? 0) + 1);
      rows.set(it.check, row);
    }
    return {
      cols: [...cols.entries()].sort((a, b) => b[1] - a[1]).map(([k]) => k),
      rows: [...rows.entries()].sort(
        (a, b) => [...b[1].values()].reduce((x, y) => x + y, 0) - [...a[1].values()].reduce((x, y) => x + y, 0),
      ),
    };
  }, [data.itens]);
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("check.title")}</CardTitle>
      </CardHeader>
      <CardContent>
        {pivot.rows.length === 0 ? (
          <EmptyState text={t("check.empty")} />
        ) : (
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>{t("check.check")}</Th>
                  {pivot.cols.map((c) => (
                    <Th key={c} className="text-right">
                      {c || t("check.noSolution")}
                    </Th>
                  ))}
                  <Th className="text-right">{t("check.total")}</Th>
                </Tr>
              </THead>
              <TBody>
                {pivot.rows.map(([check, row]) => (
                  <Tr key={check}>
                    <Td>{check}</Td>
                    {pivot.cols.map((c) => (
                      <Td key={c} className="text-right tabular-nums">
                        {fmt(row.get(c) ?? 0)}
                      </Td>
                    ))}
                    <Td className="text-right font-medium tabular-nums">
                      {fmt([...row.values()].reduce((a, b) => a + b, 0))}
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

/** Manual x IA: premissas, por ciclo e total (ganho, redução, fator). */
export function ManualVsAiSection({ data }: { data: ReportData }) {
  const { t, i18n } = useTranslation("report");
  const lang = i18n.language;
  const c = data.comparacao_manual;
  const fmt = (v: number) => formatNumber(v, lang);
  const dec = (v: number | null | undefined) =>
    v == null ? "—" : new Intl.NumberFormat(lang, { maximumFractionDigits: 1 }).format(v);
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("manual.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <p className="text-sm text-muted-foreground">
          {t("manual.premises", { perDay: c.premissas.objetos_dia, hours: c.premissas.horas_dia })}
        </p>
        <dl className="grid grid-cols-2 gap-4 md:grid-cols-5">
          <Stat
            label={t("manual.manualTime")}
            value={formatSeconds(c.total.manual_s)}
            hint={t("manual.days", { n: dec(c.total.manual_dias) })}
          />
          <Stat label={t("manual.aiTime")} value={formatSeconds(c.total.ia_s)} />
          <Stat label={t("manual.gain")} value={formatSeconds(c.total.ganho_s)} />
          <Stat
            label={t("manual.reduction")}
            value={c.total.reducao_pct == null ? "—" : `${dec(c.total.reducao_pct)}%`}
          />
          <Stat
            label={t("manual.factor")}
            value={c.total.fator == null ? "—" : `${dec(c.total.fator)}x`}
            hint={
              c.total.fator_remediacao == null
                ? undefined
                : t("manual.factorRemediation", { n: dec(c.total.fator_remediacao) })
            }
          />
        </dl>
        <div className="overflow-x-auto rounded-md border border-border">
          <Table>
            <THead>
              <Tr>
                <Th>{t("cycles.cycle")}</Th>
                <Th className="text-right">{t("manual.treatedObjects")}</Th>
                <Th className="text-right">{t("manual.pendingObjects")}</Th>
                <Th className="text-right">{t("manual.manualTime")}</Th>
                <Th className="text-right">{t("manual.aiTime")}</Th>
                <Th className="text-right">{t("manual.aiRemediation")}</Th>
              </Tr>
            </THead>
            <TBody>
              {c.ciclos.map((r) => (
                <Tr key={r.ciclo}>
                  <Td>{r.ciclo}</Td>
                  <Td className="text-right tabular-nums">{fmt(r.objetos_tratados)}</Td>
                  <Td className="text-right tabular-nums">{fmt(r.objetos_pendentes)}</Td>
                  <Td className="text-right tabular-nums">{formatSeconds(r.manual_s)}</Td>
                  <Td className="text-right tabular-nums">{formatSeconds(r.ia_s)}</Td>
                  <Td className="text-right tabular-nums">{formatSeconds(r.ia_remediacao_s)}</Td>
                </Tr>
              ))}
            </TBody>
          </Table>
        </div>
        <Source text={c.fonte} />
      </CardContent>
    </Card>
  );
}

/** Isenção, aplicação no SAP e KB. */
export function ExemptionKbSection({ data }: { data: ReportData }) {
  const { t, i18n } = useTranslation("report");
  const fmt = (v: number) => formatNumber(v, i18n.language);
  const ex = data.isencao;
  const requests = Object.entries(data.aplicacao.por_request);
  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("exemption.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {ex ? (
            <>
              <dl className="grid grid-cols-2 gap-4 md:grid-cols-4">
                <Stat label={t("exemption.findings")} value={fmt(ex.findings)} />
                <Stat label={t("exemption.requests")} value={fmt(ex.pedidos)} />
                <Stat label={t("exemption.noScope")} value={fmt(ex.sem_escopo)} />
                <Stat label={t("exemption.checkedOk")} value={fmt(ex.conferidos_ok)} />
                <Stat label={t("exemption.blocked")} value={fmt(ex.bloqueados)} />
                <Stat label={t("exemption.errors")} value={fmt(ex.erros)} />
                <Stat label={t("exemption.alreadyRequested")} value={fmt(ex.ja_pedidos)} />
                <Stat label={t("exemption.simulated")} value={fmt(ex.simulados_sem_envio)} />
              </dl>
              <p className="text-xs text-muted-foreground">
                {t("exemption.batch", { batch: ex.lote, approver: ex.aprovador ?? "—", date: ex.data ?? "—" })}
                {ex.motivos.length ? ` · ${t("exemption.reasons", { reasons: ex.motivos.join(", ") })}` : ""}
              </p>
              {ex.conferir.length > 0 ? (
                <Alert tone="warning" title={t("exemption.toCheck")}>
                  <ul className="list-disc pl-5">
                    {ex.conferir.map((c) => (
                      <li key={c}>{c}</li>
                    ))}
                  </ul>
                </Alert>
              ) : null}
              <Source text={ex.fonte} />
            </>
          ) : (
            <EmptyState text={t("exemption.empty")} />
          )}
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>{t("application.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <dl className="grid grid-cols-2 gap-4 md:grid-cols-3">
            <Stat label={t("application.activated")} value={fmt(data.aplicacao.fontes_ativadas)} />
            <Stat label={t("application.manual")} value={fmt(data.aplicacao.manuais.length)} />
            <Stat label={t("application.sessionRequest")} value={data.aplicacao.request_sessao ?? "—"} />
          </dl>
          {requests.length > 0 ? (
            <ul className="flex flex-wrap gap-2 text-sm">
              {requests.map(([r, q]) => (
                <li key={r} className="rounded-md border border-border px-2 py-1">
                  <span className="font-mono">{r}</span> · {fmt(q)}
                </li>
              ))}
            </ul>
          ) : null}
          {data.aplicacao.manuais.length > 0 ? (
            <div className="overflow-x-auto rounded-md border border-border">
              <Table>
                <THead>
                  <Tr>
                    <Th>#</Th>
                    <Th>{t("application.object")}</Th>
                    <Th>{t("application.type")}</Th>
                    <Th>{t("application.reason")}</Th>
                  </Tr>
                </THead>
                <TBody>
                  {data.aplicacao.manuais.map((m) => (
                    <Tr key={`${m.n}-${m.objeto}`}>
                      <Td>{m.n}</Td>
                      <Td className="font-mono">{m.objeto}</Td>
                      <Td>{m.tipo}</Td>
                      <Td>{m.motivo}</Td>
                    </Tr>
                  ))}
                </TBody>
              </Table>
            </div>
          ) : null}
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>{t("kb.title")}</CardTitle>
        </CardHeader>
        <CardContent>
          <dl className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <Stat label={t("kb.newFacts")} value={fmt(data.kb.fatos_novos)} />
            <Stat label={t("kb.newCards")} value={fmt(data.kb.cartoes_novos)} />
            <Stat
              label={t("kb.elements")}
              value={data.kb.elementos_com_cartoes == null ? "—" : fmt(data.kb.elementos_com_cartoes)}
            />
            <Stat label={t("kb.batchReports")} value={fmt(data.kb.relatorios_lote)} />
          </dl>
        </CardContent>
      </Card>
    </div>
  );
}

/** Execução do processo (agentes, tokens, paralelismo) e custos/tempos por etapa. */
export function ProcessSection({ data }: { data: ReportData }) {
  const { t, i18n } = useTranslation("report");
  const fmt = (v: number) => formatNumber(v, i18n.language);
  const c = data.custos;
  const p = data.execucao_processo;
  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("costs.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <dl className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <Stat label={t("costs.netTime")} value={formatSeconds(c.total.liquido_s)} />
            <Stat label={t("costs.remediationNet")} value={formatSeconds(c.remediacao_liquido_s)} />
            <Stat label={t("costs.perFinding")} value={formatSeconds(c.liquido_por_finding_s)} />
            <Stat label={t("costs.perObject")} value={formatSeconds(c.liquido_por_objeto_s)} />
            <Stat label={t("costs.sessions")} value={fmt(c.total.sessoes)} />
            <Stat label={t("costs.tokens")} value={fmt(c.total.total)} />
            <Stat label={t("costs.output")} value={fmt(c.total.saida)} />
            <Stat label={t("costs.cacheRead")} value={fmt(c.total.cache_leitura)} />
          </dl>
          {c.relogio_no_lugar_do_liquido.length > 0 ? (
            <Alert tone="warning">
              {t("costs.clockInstead", { phases: c.relogio_no_lugar_do_liquido.join(", ") })}
            </Alert>
          ) : null}
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>{t("costs.phase")}</Th>
                  <Th>{t("cycles.cycle")}</Th>
                  <Th>{t("costs.period")}</Th>
                  <Th className="text-right">{t("costs.sessions")}</Th>
                  <Th className="text-right">{t("costs.netTime")}</Th>
                  <Th className="text-right">{t("costs.tokens")}</Th>
                </Tr>
              </THead>
              <TBody>
                {c.fases.map((f) => (
                  <Tr key={`${f.etapa}-${f.ciclo}-${f.ini}`}>
                    <Td>{f.etapa}</Td>
                    <Td>{f.ciclo}</Td>
                    <Td>{f.periodo}</Td>
                    <Td className="text-right tabular-nums">{fmt(f.sessoes)}</Td>
                    <Td className="text-right tabular-nums">{formatSeconds(f.liquido_s)}</Td>
                    <Td className="text-right tabular-nums">{fmt(f.total)}</Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          </div>
          <Source text={c.fonte} />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>{t("process.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <dl className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <Stat label={t("process.agents")} value={fmt(p.totais.agentes)} />
            <Stat label={t("process.agentTime")} value={formatSeconds(p.totais.ativo_agentes_s)} />
            <Stat label={t("process.parallelGain")} value={formatSeconds(p.totais.ganho_paralelismo_s)} />
            <Stat
              label={t("process.runsRecorded")}
              value={`${fmt(p.totais.execucoes_com_registro)}/${fmt(p.totais.execucoes_com_registro + p.totais.execucoes_sem_registro)}`}
            />
          </dl>
          <div className="grid gap-4 md:grid-cols-3">
            <Breakdown title={t("process.byType")} values={p.totais.agentes_por_tipo} fmt={fmt} />
            <Breakdown title={t("process.byModel")} values={p.totais.agentes_por_modelo} fmt={fmt} />
            <Breakdown title={t("process.tokensByModel")} values={p.totais.tokens_por_modelo} fmt={fmt} />
          </div>
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>{t("costs.phase")}</Th>
                  <Th>{t("process.start")}</Th>
                  <Th>{t("process.end")}</Th>
                  <Th>{t("process.skills")}</Th>
                  <Th className="text-right">{t("process.agents")}</Th>
                  <Th className="text-right">{t("process.agentTime")}</Th>
                  <Th className="text-right">{t("process.parallelGain")}</Th>
                </Tr>
              </THead>
              <TBody>
                {p.execucoes.map((r) => (
                  <Tr key={`${r.etapa}-${r.fim}`}>
                    <Td>{r.etapa}</Td>
                    <Td>{r.inicio ?? "—"}</Td>
                    <Td>{r.fim}</Td>
                    <Td>{Object.keys(r.skills).join(", ") || "—"}</Td>
                    <Td className="text-right tabular-nums">{fmt(r.agentes_qtde)}</Td>
                    <Td className="text-right tabular-nums">{formatSeconds(r.agentes_ativo_s)}</Td>
                    <Td className="text-right tabular-nums">{formatSeconds(r.ganho_paralelismo_s)}</Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          </div>
          {p.sem_registro.length > 0 ? (
            <Alert tone="warning" title={t("process.missing")}>
              <ul className="list-disc pl-5">
                {p.sem_registro.map((m) => (
                  <li key={`${m.etapa}-${m.fim}`}>
                    {m.etapa} · {m.fim}
                  </li>
                ))}
              </ul>
            </Alert>
          ) : null}
          <Versions data={data} />
        </CardContent>
      </Card>
    </div>
  );
}

function Breakdown({
  title,
  values,
  fmt,
}: {
  title: string;
  values: Record<string, number>;
  fmt: (n: number) => string;
}) {
  const entries = Object.entries(values);
  return (
    <div>
      <p className="mb-1 text-xs font-medium text-muted-foreground">{title}</p>
      {entries.length === 0 ? (
        <p className="text-sm text-muted-foreground">—</p>
      ) : (
        <ul className="space-y-0.5 text-sm">
          {entries.map(([k, v]) => (
            <li key={k}>
              {k}: <span className="tabular-nums">{fmt(v)}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

function Versions({ data }: { data: ReportData }) {
  const { t } = useTranslation("report");
  const entries = Object.entries(data.versoes);
  if (entries.length === 0) return null;
  return (
    <div>
      <p className="mb-1 text-xs font-medium text-muted-foreground">{t("process.versions")}</p>
      <ul className="flex flex-wrap gap-2 text-sm">
        {entries.map(([skill, v]) => (
          <li key={skill} className="rounded-md border border-border px-2 py-1">
            {skill}: {v.inicial ?? "—"} → {v.atual ?? "—"}
          </li>
        ))}
      </ul>
    </div>
  );
}
