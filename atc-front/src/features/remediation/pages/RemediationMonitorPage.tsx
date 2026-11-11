import { Download, ExternalLink, Plus } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import type { RemediationMonitorRow, RunResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Checkbox, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { CodeMessage } from "@/features/shared/CodeMessage";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { isRunActive, RunStatusBadge } from "@/features/shared/status";
import { formatDateTime } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import type { AutoSummary } from "../components/AutoRemediationPanel";
import { CountsBar } from "../components/status";
import { useCustomerOptions, useExportCsv, useRemediationMonitor } from "../hooks";

const DEFAULTS = { customerId: "", archived: "false" };

/** Passo em andamento (ou o que falhou) com o progresso. */
function Progress({ run }: { run: RunResponse }) {
  const { t, i18n } = useTranslation("remediation");
  const steps = [...run.steps].sort((a, b) => a.order - b.order);
  const step = steps.find((s) => s.status === "Processing") ?? steps.find((s) => s.status === "Error");
  if (!step) return <span className="text-muted-foreground">—</span>;
  const label = i18n.exists(`runs:steps.${step.code}`) ? t(`runs:steps.${step.code}`) : step.code;
  const done = steps.filter((s) => s.status === "Completed" || s.status === "Skipped").length;
  const pct = step.progressTotal > 0 ? Math.round((step.progressDone / step.progressTotal) * 100) : null;
  return (
    <div className="min-w-44 space-y-1">
      <p className="text-sm">
        {t("monitor.stepOf", { current: done + 1, total: steps.length })} · {label}
      </p>
      {pct !== null && (
        <div className="flex items-center gap-2">
          <div
            className="h-1.5 flex-1 overflow-hidden rounded bg-muted"
            role="progressbar"
            aria-valuenow={pct}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label={t("monitor.progressOf", { name: run.sapSystemName })}
          >
            <div className="h-full bg-primary" style={{ width: `${pct}%` }} />
          </div>
          <span className="text-xs tabular-nums text-muted-foreground">
            {step.progressDone}/{step.progressTotal}
          </span>
        </div>
      )}
    </div>
  );
}

function LastRunCell({ run }: { run: RunResponse | undefined }) {
  const { t, i18n } = useTranslation("remediation");
  if (!run) return <span className="text-muted-foreground">{t("monitor.neverRun")}</span>;
  const summary =
    run.kind === "AutoRemediation" && run.status === "Completed"
      ? (run.result as { summary?: AutoSummary } | null)?.summary
      : undefined;
  return (
    <div className="space-y-1">
      <div className="flex flex-wrap items-center gap-1">
        <RunStatusBadge status={run.status} />
        <span className="text-sm">{t(`runs:kind.${run.kind}`)}</span>
      </div>
      <p className="text-xs text-muted-foreground">
        {formatDateTime(run.finishedAt ?? run.startedAt ?? run.queuedAt, i18n.language)}
      </p>
      {summary && (
        <p className="text-sm">
          {t("monitor.autoSummary", {
            activated: summary.activated,
            pending: summary.pending - (summary.awaitingApproval ?? 0),
          })}
          {(summary.awaitingApproval ?? 0) > 0 && (
            <span> · {t("monitor.autoApproval", { count: summary.awaitingApproval })}</span>
          )}
          {summary.applyErrors > 0 && (
            <span className="text-destructive"> · {t("monitor.autoErrors", { count: summary.applyErrors })}</span>
          )}
        </p>
      )}
      {run.status === "Failed" && run.failureCode && (
        <p className="text-xs text-destructive">
          {run.failureArgs?.message ?? <CodeMessage code={run.failureCode} args={run.failureArgs ?? undefined} />}
        </p>
      )}
    </div>
  );
}

/** Exportar o atc_status.csv direto do monitor (o hook baixa com o token). */
function ExportButton({ row }: { row: RemediationMonitorRow }) {
  const { t } = useTranslation("remediation");
  const exportCsv = useExportCsv(row.session.id);
  const active = !!row.lastRun && isRunActive(row.lastRun.status);
  return (
    <Button
      variant="ghost"
      size="icon"
      disabled={active || exportCsv.isPending}
      aria-label={t("monitor.export", { name: row.session.sapSystemName })}
      title={exportCsv.error ? exportCsv.error.title : undefined}
      onClick={() => exportCsv.mutate()}
    >
      <Download />
    </Button>
  );
}

/**
 * `/remediacao` (tela principal da remediação): sessões de todos os clientes com a situação dos itens e a execução em andamento, ao vivo.
 * "Nova remediação" cria a sessão; o play é dado na aba Itens da sessão. Ao terminar, exporta o atc_status.csv daqui ou da sessão.
 */
export function RemediationMonitorPage() {
  const { t, i18n } = useTranslation("remediation");
  const [state, set] = useSearchState(DEFAULTS);
  const customers = useCustomerOptions();
  const monitor = useRemediationMonitor(state.customerId, state.archived === "true");
  const rows = monitor.data ?? [];
  const counts = {
    running: rows.filter((r) => r.lastRun?.status === "Running" || r.lastRun?.status === "Cancelling").length,
    queued: rows.filter((r) => r.lastRun?.status === "Queued").length,
    completed: rows.filter((r) => r.lastRun?.status === "Completed").length,
    failed: rows.filter((r) => r.lastRun?.status === "Failed").length,
  };

  return (
    <>
      <PageHeader
        title={t("title")}
        description={t("monitor.description")}
        actions={
          <Button asChild>
            <Link to="/remediacao/nova">
              <Plus />
              {t("monitor.new")}
            </Link>
          </Button>
        }
      />
      <div className="space-y-4">
        <div className="flex flex-wrap items-end justify-between gap-4">
          <div className="flex flex-wrap items-end gap-4">
            <div className="w-full sm:w-72">
              <Field id="rem-customer" label={t("header.customer")}>
                <NativeSelect
                  id="rem-customer"
                  value={state.customerId}
                  onChange={(e) => set({ customerId: e.target.value })}
                >
                  <option value="">{t("monitor.allCustomers")}</option>
                  {customers.data?.items.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </NativeSelect>
              </Field>
            </div>
            <label className="flex items-center gap-2 pb-2 text-sm">
              <Checkbox
                checked={state.archived === "true"}
                onChange={(e) => set({ archived: e.target.checked ? "true" : "false" })}
              />
              {t("monitor.includeArchived")}
            </label>
          </div>
          <div className="flex flex-wrap items-center gap-2" aria-live="polite">
            <Badge tone="info">{t("monitor.count.running", { count: counts.running })}</Badge>
            <Badge tone="neutral">{t("monitor.count.queued", { count: counts.queued })}</Badge>
            <Badge tone="success">{t("monitor.count.completed", { count: counts.completed })}</Badge>
            <Badge tone="danger">{t("monitor.count.failed", { count: counts.failed })}</Badge>
          </div>
        </div>

        <ApiErrorAlert error={monitor.error} />
        <Card>
          {monitor.isLoading ? (
            <Loading />
          ) : rows.length === 0 ? (
            <EmptyState text={t("monitor.empty")} />
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <THead>
                  <Tr>
                    <Th>{t("header.customer")}</Th>
                    <Th>{t("header.system")}</Th>
                    <Th>{t("monitor.col.items")}</Th>
                    <Th>{t("monitor.col.lastRun")}</Th>
                    <Th>{t("monitor.col.progress")}</Th>
                    <Th className="w-24">
                      <span className="sr-only">{t("common:common.actions")}</span>
                    </Th>
                  </Tr>
                </THead>
                <TBody>
                  {rows.map((row) => {
                    const s = row.session;
                    const run = row.lastRun;
                    const active = !!run && isRunActive(run.status);
                    const auto = run?.kind === "AutoRemediation" ? `?auto=${run.id}` : "";
                    return (
                      <Tr key={s.id}>
                        <Td>{s.customerName}</Td>
                        <Td>
                          <Link className="font-medium text-primary hover:underline" to={`/remediacao/${s.id}${auto}`}>
                            {s.sapSystemName}
                          </Link>
                          <p className="text-xs text-muted-foreground">
                            {t("monitor.cycle", { cycle: s.cycle })} · {t(`source.${s.source}`)} ·{" "}
                            {formatDateTime(s.createdAt, i18n.language)}
                          </p>
                          {s.archived && <Badge tone="neutral">{t("header.archived")}</Badge>}
                        </Td>
                        <Td className="min-w-56">
                          <CountsBar counts={s.counts} />
                        </Td>
                        <Td>
                          <LastRunCell run={run} />
                        </Td>
                        <Td>
                          {run && active ? <Progress run={run} /> : <span className="text-muted-foreground">—</span>}
                        </Td>
                        <Td>
                          <div className="flex items-center gap-1">
                            <Button variant="ghost" size="icon" asChild>
                              <Link
                                to={`/remediacao/${s.id}${auto}`}
                                aria-label={t("monitor.open", { name: s.sapSystemName })}
                              >
                                <ExternalLink />
                              </Link>
                            </Button>
                            <ExportButton row={row} />
                          </div>
                        </Td>
                      </Tr>
                    );
                  })}
                </TBody>
              </Table>
            </div>
          )}
        </Card>
        <p className="text-xs text-muted-foreground">
          {t("monitor.refresh", { seconds: counts.running + counts.queued ? 3 : 15 })}{" "}
          {monitor.dataUpdatedAt ? formatDateTime(new Date(monitor.dataUpdatedAt).toISOString(), i18n.language) : ""}
        </p>
      </div>
    </>
  );
}
