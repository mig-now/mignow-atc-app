import { Activity, Download, ExternalLink, Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import type { ExtractionMonitorRow, RunResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
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
import { formatDateTime, formatNumber } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import {
  type ExtractionDispatch,
  useCustomerOptions,
  useExtractionMonitor,
  useStartExtractions,
} from "../hooks/useExtraction";

const DEFAULTS = { customerId: "" };

type Summary = { situation?: string; totalCount?: number; newCount?: number };

/** Passo em andamento (ou o último que andou) com o progresso, para a coluna "Andamento". */
function currentStep(run: RunResponse) {
  const steps = [...run.steps].sort((a, b) => a.order - b.order);
  return steps.find((s) => s.status === "Processing") ?? steps.find((s) => s.status === "Error") ?? null;
}

function Progress({ run }: { run: RunResponse }) {
  const { t, i18n } = useTranslation("extraction");
  const step = currentStep(run);
  if (!step) return <span className="text-muted-foreground">—</span>;
  const label = i18n.exists(`runs:steps.${step.code}`) ? t(`runs:steps.${step.code}`) : step.code;
  const pct = step.progressTotal > 0 ? Math.round((step.progressDone / step.progressTotal) * 100) : null;
  return (
    <div className="min-w-40 space-y-1">
      <p className="text-sm">{label}</p>
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
          <span className="text-sm tabular-nums text-muted-foreground">
            {step.progressDone}/{step.progressTotal}
          </span>
        </div>
      )}
    </div>
  );
}

function RunCell({ row }: { row: ExtractionMonitorRow }) {
  const { t, i18n } = useTranslation("extraction");
  const run = row.lastRun;
  if (!run) return <span className="text-muted-foreground">{t("monitor.neverRun")}</span>;
  const summary = (run.status === "Completed" ? run.result : null) as Summary | null;
  return (
    <div className="space-y-1">
      <RunStatusBadge status={run.status} />
      <p className="text-xs text-muted-foreground">
        {formatDateTime(run.finishedAt ?? run.startedAt ?? run.queuedAt, i18n.language)}
      </p>
      {summary?.situation && <p className="text-sm">{t(`runner.situation.${summary.situation}`)}</p>}
      {run.status === "Failed" && run.failureCode && (
        <p className="text-xs text-destructive">
          {/* O back manda o texto já localizado em `message` (ex.: Sap.Upstream); sem ele, tradução pelo código. */}
          {run.failureArgs?.message ?? <CodeMessage code={run.failureCode} args={run.failureArgs ?? undefined} />}
        </p>
      )}
    </div>
  );
}

function LastExtractionCell({ row }: { row: ExtractionMonitorRow }) {
  const { t, i18n } = useTranslation("extraction");
  const e = row.lastExtraction;
  if (!e) return <span className="text-muted-foreground">—</span>;
  return (
    <div className="space-y-0.5 text-sm">
      <p className="font-mono">{e.displayId}</p>
      <p className="text-xs text-muted-foreground">{formatDateTime(e.extractedAt, i18n.language)}</p>
      <p>
        {t("monitor.counts", {
          total: formatNumber(e.totalCount, i18n.language),
          fresh: formatNumber(e.newCount, i18n.language),
        })}
      </p>
    </div>
  );
}

/**
 * `/extracao` (tela principal da extração): dispara a extração de vários ambientes (de clientes diferentes) de uma vez e acompanha todos na
 * mesma tela. Cada ambiente vira um run próprio; o motor roda vários em paralelo (um por ambiente, até o limite do worker)
 * e o resto fica na fila. Ambiente ocupado (409) aparece na linha, sem impedir os outros.
 */
export function ExtractionMonitorPage() {
  const { t, i18n } = useTranslation("extraction");
  const [state, set] = useSearchState(DEFAULTS);
  const customers = useCustomerOptions();
  const monitor = useExtractionMonitor(state.customerId);
  const rows = monitor.data ?? [];
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [force, setForce] = useState(false);
  const start = useStartExtractions();
  const [dispatches, setDispatches] = useState<Record<string, ExtractionDispatch>>({});

  const selectable = rows.filter((r) => !(r.lastRun && isRunActive(r.lastRun.status)));
  const allSelected = selectable.length > 0 && selectable.every((r) => selected.has(r.sapSystemId));
  const toggle = (id: string) =>
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  const toggleAll = () => setSelected(allSelected ? new Set() : new Set(selectable.map((r) => r.sapSystemId)));

  const counts = {
    running: rows.filter((r) => r.lastRun?.status === "Running" || r.lastRun?.status === "Cancelling").length,
    queued: rows.filter((r) => r.lastRun?.status === "Queued").length,
    completed: rows.filter((r) => r.lastRun?.status === "Completed").length,
    failed: rows.filter((r) => r.lastRun?.status === "Failed").length,
  };

  const extract = () =>
    start.mutate(
      { sapSystemIds: [...selected], force, failedText: t("monitor.dispatchFailed") },
      {
        onSuccess: (results) => {
          setDispatches(Object.fromEntries(results.map((r) => [r.sapSystemId, r])));
          setSelected(new Set());
        },
      },
    );

  const okCount = Object.values(dispatches).filter((d) => d.ok).length;
  const failCount = Object.values(dispatches).length - okCount;

  return (
    <>
      <PageHeader
        title={t("monitor.title")}
        description={t("monitor.description")}
        actions={
          <Button asChild>
            <Link to={state.customerId ? `/extracao/nova?customerId=${state.customerId}` : "/extracao/nova"}>
              <Plus />
              {t("monitor.newExtraction")}
            </Link>
          </Button>
        }
      />
      <div className="space-y-4">
        <div className="flex flex-wrap items-end justify-between gap-4">
          <div className="w-full sm:w-72">
            <Field id="mon-customer" label={t("picker.customer")}>
              <NativeSelect
                id="mon-customer"
                value={state.customerId}
                onChange={(e) => {
                  set({ customerId: e.target.value });
                  setSelected(new Set());
                }}
              >
                <option value="">{t("picker.allCustomers")}</option>
                {customers.data?.items.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </NativeSelect>
            </Field>
          </div>
          <div className="flex flex-wrap items-center gap-2" aria-live="polite">
            <Badge tone="info">{t("monitor.count.running", { count: counts.running })}</Badge>
            <Badge tone="neutral">{t("monitor.count.queued", { count: counts.queued })}</Badge>
            <Badge tone="success">{t("monitor.count.completed", { count: counts.completed })}</Badge>
            <Badge tone="danger">{t("monitor.count.failed", { count: counts.failed })}</Badge>
          </div>
        </div>

        <Card className="flex flex-wrap items-center justify-between gap-3 p-4">
          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={force} onChange={(e) => setForce(e.target.checked)} />
            {t("monitor.force")}
          </label>
          <Button disabled={selected.size === 0 || start.isPending} onClick={extract}>
            <Download />
            {t("monitor.extractSelected", { count: selected.size })}
          </Button>
          <p className="w-full text-xs text-muted-foreground">{t("monitor.hint")}</p>
        </Card>

        {Object.keys(dispatches).length > 0 && (
          <Alert
            tone={failCount ? "warning" : "info"}
            title={t("monitor.dispatched", { ok: okCount, failed: failCount })}
          />
        )}

        <ApiErrorAlert error={start.error} />
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
                    <Th className="w-8">
                      <Checkbox aria-label={t("monitor.selectAll")} checked={allSelected} onChange={toggleAll} />
                    </Th>
                    <Th>{t("picker.customer")}</Th>
                    <Th>{t("picker.environment")}</Th>
                    <Th>{t("monitor.col.status")}</Th>
                    <Th>{t("monitor.col.progress")}</Th>
                    <Th>{t("monitor.col.lastExtraction")}</Th>
                    <Th className="w-24">
                      <span className="sr-only">{t("common:common.actions")}</span>
                    </Th>
                  </Tr>
                </THead>
                <TBody>
                  {rows.map((row) => {
                    const active = !!row.lastRun && isRunActive(row.lastRun.status);
                    const dispatch = dispatches[row.sapSystemId];
                    return (
                      <Tr key={row.sapSystemId}>
                        <Td>
                          <Checkbox
                            aria-label={t("monitor.select", { name: `${row.customerName} · ${row.sapSystemName}` })}
                            checked={selected.has(row.sapSystemId)}
                            disabled={active}
                            onChange={() => toggle(row.sapSystemId)}
                          />
                        </Td>
                        <Td>{row.customerName}</Td>
                        <Td>
                          <span className="font-medium">{row.sapSystemName}</span>{" "}
                          {row.isProduction && <Badge tone="warning">{t("monitor.production")}</Badge>}
                          {row.isSimulator && <Badge tone="neutral">{t("monitor.simulator")}</Badge>}
                          {dispatch && !dispatch.ok && (
                            <p className="mt-1 text-xs text-destructive">{dispatch.message}</p>
                          )}
                        </Td>
                        <Td>
                          <RunCell row={row} />
                        </Td>
                        <Td>
                          {row.lastRun && active ? (
                            <Progress run={row.lastRun} />
                          ) : (
                            <span className="text-muted-foreground">—</span>
                          )}
                        </Td>
                        <Td>
                          <LastExtractionCell row={row} />
                        </Td>
                        <Td>
                          <div className="flex items-center gap-1">
                            {row.lastRun && (
                              <Button variant="ghost" size="icon" asChild>
                                <Link
                                  to={`/execucoes/${row.lastRun.id}`}
                                  aria-label={t("monitor.openRun", { name: row.sapSystemName })}
                                >
                                  <Activity />
                                </Link>
                              </Button>
                            )}
                            <Button variant="ghost" size="icon" asChild>
                              <Link
                                to={`/extracao/nova?customerId=${row.customerId}&systemId=${row.sapSystemId}`}
                                aria-label={t("monitor.openEnvironment", { name: row.sapSystemName })}
                              >
                                <ExternalLink />
                              </Link>
                            </Button>
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
          {t("monitor.refresh", { seconds: Math.round((counts.running + counts.queued ? 3000 : 15000) / 1000) })}{" "}
          {monitor.dataUpdatedAt ? formatDateTime(new Date(monitor.dataUpdatedAt).toISOString(), i18n.language) : ""}
        </p>
      </div>
    </>
  );
}
