import { Ban, RotateCw } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { RunResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { AiCallsTable } from "@/features/shared/AiCallsTable";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { CodeMessage } from "@/features/shared/CodeMessage";
import { EventsLog } from "@/features/shared/EventsLog";
import { Pagination } from "@/features/shared/pagination";
import { StepsTimeline } from "@/features/shared/StepsTimeline";
import { RunStatusBadge } from "@/features/shared/status";
import { formatDateTime, formatMoney, formatNumber } from "@/lib/format";
import { useRunWatcher } from "../hooks/useRunWatcher";

/** Duração legível (h/min/s) entre dois instantes; "—" sem início. */
export function formatDuration(from?: string | null, to?: string | null) {
  if (!from || !to) return "—";
  const total = Math.max(0, Math.round((new Date(to).getTime() - new Date(from).getTime()) / 1000));
  const h = Math.floor(total / 3600);
  const m = Math.floor((total % 3600) / 60);
  const s = total % 60;
  return [h ? `${h}h` : "", h || m ? `${m}min` : "", `${s}s`].filter(Boolean).join(" ");
}

function Info({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="truncate text-sm">{children}</dd>
    </div>
  );
}

/**
 * Monitor de um Run: cabeçalho (tipo, ambiente, status, quem, tempos, tokens/custo), passos com progresso, log paginado,
 * chamadas de IA e ações cancelar / tentar de novo. Ao vivo (SignalR) + polling de 3 s enquanto ativo.
 * `onRetried` recebe o run novo (o retry cria outro id): a página navega; um painel inline pode trocar o runId.
 */
export function RunMonitorPanel({ runId, onRetried }: { runId: string; onRetried?: (run: RunResponse) => void }) {
  const { t, i18n } = useTranslation("runs");
  const lang = i18n.language;
  const w = useRunWatcher(runId);
  const [tab, setTab] = useState("log");
  const { run } = w;

  if (w.isLoading) return <Loading />;
  if (w.error || !run) return <ApiErrorAlert error={w.error ?? new Error("not-found")} />;

  const canRetry = run.status === "Failed" || run.status === "Cancelled";
  const end = run.finishedAt ?? (w.active ? new Date().toISOString() : null);
  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="min-w-0">
            <CardTitle>{t(`kind.${run.kind}`)}</CardTitle>
            <p className="mt-1 text-xs text-muted-foreground">
              {run.customerName} · {run.sapSystemName}
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <RunStatusBadge status={run.status} />
            {w.active && run.status !== "Cancelling" && (
              <Button variant="outline" size="sm" disabled={w.cancel.isPending} onClick={() => w.cancel.mutate()}>
                <Ban />
                {t("monitor.cancel")}
              </Button>
            )}
            {canRetry && (
              <Button
                size="sm"
                disabled={w.retry.isPending}
                onClick={() => w.retry.mutate(undefined, { onSuccess: (r) => onRetried?.(r) })}
              >
                <RotateCw />
                {t("monitor.retry")}
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <ApiErrorAlert error={w.cancel.error ?? w.retry.error} />
          {run.failureCode && (
            <Alert tone="danger" title={t("monitor.failure")}>
              <CodeMessage code={run.failureCode} args={run.failureArgs} />
            </Alert>
          )}
          <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <Info label={t("monitor.startedBy")}>{run.startedBy}</Info>
            <Info label={t("monitor.queuedAt")}>{formatDateTime(run.queuedAt, lang)}</Info>
            <Info label={t("monitor.startedAt")}>{formatDateTime(run.startedAt, lang)}</Info>
            <Info label={t("monitor.finishedAt")}>{formatDateTime(run.finishedAt, lang)}</Info>
            <Info label={t("monitor.duration")}>{formatDuration(run.startedAt, end)}</Info>
            <Info label={t("monitor.parallelism")}>{run.parallelism}</Info>
            <Info label={t("monitor.tokens")}>
              {formatNumber(run.aiInputTokens, lang)} / {formatNumber(run.aiOutputTokens, lang)}
              <span className="text-xs text-muted-foreground"> ({t("monitor.aiCalls", { count: run.aiCalls })})</span>
            </Info>
            <Info label={t("monitor.cost")}>
              {formatMoney(run.aiCost, lang)}
              {run.aiCallsWithoutCost > 0 && (
                <span className="text-xs text-muted-foreground">
                  {" "}
                  ({t("monitor.withoutCost", { count: run.aiCallsWithoutCost })})
                </span>
              )}
            </Info>
          </dl>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t("monitor.steps")}</CardTitle>
        </CardHeader>
        <CardContent>
          <StepsTimeline steps={w.steps} />
        </CardContent>
      </Card>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          <TabsTrigger value="log">{t("monitor.log")}</TabsTrigger>
          <TabsTrigger value="ai">{t("monitor.ai")}</TabsTrigger>
        </TabsList>
        <TabsContent value="log">
          <Card>
            <EventsLog
              events={w.events.items}
              loading={w.events.isLoading}
              error={w.events.error}
              paging={{
                page: w.events.page,
                pageSize: w.events.pageSize,
                total: w.events.total,
                onPage: w.events.setPage,
              }}
            />
          </Card>
        </TabsContent>
        <TabsContent value="ai">
          <Card>
            <CardContent>{w.aiCalls.isLoading ? <Loading /> : <AiCallsTable calls={w.aiCalls.items} />}</CardContent>
            {w.aiCalls.total > w.aiCalls.pageSize && (
              <Pagination
                page={w.aiCalls.page}
                pageSize={w.aiCalls.pageSize}
                total={w.aiCalls.total}
                onPage={w.aiCalls.setPage}
              />
            )}
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
