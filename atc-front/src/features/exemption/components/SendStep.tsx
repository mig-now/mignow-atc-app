import { Send } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { isApiError } from "@/api/client";
import type { ExemptionBatchResponse, RunResponse, SapSystemResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { RunMonitorPanel } from "@/features/runs";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { isRunActive } from "@/features/shared/status";
import { useBatchAction, useBlockingRun, useRefreshWhenRunEnds } from "../hooks/useExemption";
import { RequestsTable } from "./RequestsTable";

type TableState = { status: string; page: number; onStatus: (s: string) => void; onPage: (p: number) => void };

/**
 * Etapa 4: ENVIA ao SAP (grava pedidos de isenção). Sempre com diálogo de confirmação explícito (contagem + aviso);
 * envio de todos os SIMULADO ou só dos `seq` marcados. Bloqueado em ambiente de produção.
 */
export function SendStep({
  batch,
  system,
  runs,
  table,
}: {
  batch: ExemptionBatchResponse;
  system: SapSystemResponse | undefined;
  runs: RunResponse[];
  table: TableState;
}) {
  const { t } = useTranslation("exemption");
  const send = useBatchAction(batch.id, "send");
  const [selected, setSelected] = useState<number[]>([]);
  const [pending, setPending] = useState<{ seq?: number[]; count: number } | null>(null);
  const [started, setStarted] = useState<string | undefined>();
  const latest = runs.find((r) => r.kind === "ExemptionSend");
  const runId = started ?? latest?.id;
  useRefreshWhenRunEnds(runId);
  const blocking = useBlockingRun(batch.sapSystemId, send.error);
  const busy = isApiError(send.error) && send.error.status === 409;
  const sendable = batch.totals.SIMULADO ?? 0;
  const production = system?.isProduction ?? false;
  const active = runs.some((r) => isRunActive(r.status));
  const locked = production || active || send.isPending;

  const confirm = () => {
    if (!pending) return;
    const body = pending.seq ? { seq: pending.seq } : undefined;
    setPending(null);
    send.mutate(body, { onSuccess: (r) => setStarted(r.id) });
  };

  return (
    <div className="space-y-4">
      <Alert tone="warning" title={t("send.warningTitle")}>
        <p className="mt-1 text-sm">{t("send.warning")}</p>
      </Alert>
      {production && (
        <Alert tone="danger" title={t("send.productionTitle")}>
          <p className="mt-1 text-sm">{t("send.production")}</p>
        </Alert>
      )}
      <div className="flex flex-wrap items-center gap-2">
        <Button disabled={locked || sendable === 0} onClick={() => setPending({ count: sendable })}>
          <Send />
          {t("send.sendAll", { count: sendable })}
        </Button>
        <Button
          variant="outline"
          disabled={locked || selected.length === 0}
          onClick={() => setPending({ seq: selected, count: selected.length })}
        >
          {t("send.sendSelected", { count: selected.length })}
        </Button>
        {sendable === 0 && <span className="text-sm text-muted-foreground">{t("send.nothing")}</span>}
      </div>
      <ApiErrorAlert error={busy ? null : send.error} />
      {busy && isApiError(send.error) && (
        <Alert tone="warning" title={send.error.title}>
          {blocking ? (
            <p className="mt-1 text-sm">
              <Link className="font-medium underline" to={`/execucoes/${blocking.id}`}>
                {t("busy.open", { kind: t(`runs:kind.${blocking.kind}`), by: blocking.startedBy })}
              </Link>
            </p>
          ) : null}
        </Alert>
      )}
      <RequestsTable batchId={batch.id} {...table} selection={{ selected, onChange: setSelected }} />
      {runId && <RunMonitorPanel runId={runId} onRetried={(r) => setStarted(r.id)} />}

      <Dialog open={!!pending} onOpenChange={(o) => !o && setPending(null)}>
        <DialogContent
          title={t("send.confirmTitle", { count: pending?.count ?? 0 })}
          description={t("send.confirmText", {
            count: pending?.count ?? 0,
            approver: batch.approver ?? "—",
            environment: system?.displayName ?? batch.sapSystemId,
          })}
        >
          <Alert tone="danger" title={t("send.confirmWarningTitle")}>
            <p className="mt-1 text-sm">{t("send.confirmWarning")}</p>
          </Alert>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPending(null)}>
              {t("common:actions.cancel")}
            </Button>
            <Button variant="destructive" onClick={confirm}>
              {t("send.confirmAction", { count: pending?.count ?? 0 })}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
