import { Download, History, X } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RunMonitorPanel, useRunWatcher } from "@/features/runs";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { Markdown } from "@/features/shared/Markdown";
import { useExportCsv } from "../hooks";

/** Resumo gravado pelo último passo do play (`result.summary`). */
export type AutoSummary = {
  items: number;
  kb: number;
  ai: number;
  exemption: number;
  cancelled: number;
  pending: number;
  awaitingApproval?: number;
  activated: number;
  applyErrors: number;
  standard: number;
};

/**
 * Acompanhamento do play na sessão: passos, progresso e log ao vivo; ao concluir, o resumo (itens por situação, ativados no SAP), o
 * relatório do lote e as saídas: exportar o atc_status.csv e abrir o histórico.
 */
export function AutoRemediationPanel({
  sessionId,
  runId,
  onRunChange,
  onHistory,
  onClose,
}: {
  sessionId: string;
  runId: string;
  onRunChange: (runId: string) => void;
  onHistory: () => void;
  onClose: () => void;
}) {
  const { t } = useTranslation("remediation");
  const { run } = useRunWatcher(runId);
  const exportCsv = useExportCsv(sessionId);
  const result = run?.result as { summary?: AutoSummary; reportMarkdown?: string } | null | undefined;
  const summary = run?.status === "Completed" ? result?.summary : undefined;
  const finished = run && ["Completed", "Failed", "Cancelled"].includes(run.status);

  return (
    <Card className="mb-4">
      <CardHeader>
        <CardTitle>{t("auto.panelTitle")}</CardTitle>
        <Button variant="ghost" size="icon" aria-label={t("auto.close")} onClick={onClose}>
          <X />
        </Button>
      </CardHeader>
      <CardContent className="space-y-4">
        {summary && (
          <div className="space-y-2" aria-live="polite">
            <p className="text-sm font-medium">
              {t("auto.done", { items: summary.items, activated: summary.activated })}
            </p>
            <div className="flex flex-wrap gap-1">
              <Badge tone="success">{t("auto.summary.activated", { count: summary.activated })}</Badge>
              <Badge tone="info">{t("auto.summary.kb", { count: summary.kb })}</Badge>
              <Badge tone="info">{t("auto.summary.ai", { count: summary.ai })}</Badge>
              <Badge tone="neutral">{t("auto.summary.exemption", { count: summary.exemption })}</Badge>
              <Badge tone="neutral">{t("auto.summary.cancelled", { count: summary.cancelled })}</Badge>
              <Badge tone="warning">
                {t("auto.summary.pending", { count: summary.pending - (summary.awaitingApproval ?? 0) })}
              </Badge>
              {(summary.awaitingApproval ?? 0) > 0 && (
                <Badge tone="primary">{t("auto.summary.approval", { count: summary.awaitingApproval })}</Badge>
              )}
              {summary.applyErrors > 0 && (
                <Badge tone="danger">{t("auto.summary.applyErrors", { count: summary.applyErrors })}</Badge>
              )}
              {summary.standard > 0 && (
                <Badge tone="warning">{t("auto.summary.standard", { count: summary.standard })}</Badge>
              )}
            </div>
          </div>
        )}
        {finished && (
          <div className="flex flex-wrap gap-2">
            <Button disabled={exportCsv.isPending} onClick={() => exportCsv.mutate()}>
              <Download />
              {t("auto.export")}
            </Button>
            <Button variant="outline" onClick={onHistory}>
              <History />
              {t("auto.history")}
            </Button>
          </div>
        )}
        <ApiErrorAlert error={exportCsv.error} />
        <RunMonitorPanel runId={runId} onRetried={(r) => onRunChange(r.id)} />
        {finished && result?.reportMarkdown && (
          <details className="rounded-md border p-3">
            <summary className="cursor-pointer text-sm font-medium">{t("auto.report")}</summary>
            <div className="mt-2">
              <Markdown text={result.reportMarkdown} />
            </div>
          </details>
        )}
      </CardContent>
    </Card>
  );
}
