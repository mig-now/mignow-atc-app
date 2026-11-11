import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { isApiError } from "@/api/client";
import type { ExemptionBatchResponse, RunResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { RunMonitorPanel } from "@/features/runs";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { isRunActive } from "@/features/shared/status";
import { type BatchAction, useBatchAction, useBlockingRun, useRefreshWhenRunEnds } from "../hooks/useExemption";

const KIND = {
  discover: "ExemptionDiscover",
  simulate: "ExemptionSimulate",
  send: "ExemptionSend",
  check: "ExemptionCheck",
} as const;

/**
 * Botão da etapa + monitor do run (inline). Sem run disparado nesta tela, mostra o último run desse tipo do lote.
 * O 409 de ambiente ocupado mostra o run que bloqueia, com link.
 */
export function RunStepPanel({
  batch,
  runs,
  action,
  label,
  seq,
  disabled,
  extra,
}: {
  batch: ExemptionBatchResponse;
  runs: RunResponse[];
  action: BatchAction;
  label: string;
  seq?: number[];
  disabled?: boolean;
  extra?: React.ReactNode;
}) {
  const { t } = useTranslation("exemption");
  const run = useBatchAction(batch.id, action);
  const [started, setStarted] = useState<string | undefined>();
  const latest = runs.find((r) => r.kind === KIND[action]);
  const runId = started ?? latest?.id;
  useRefreshWhenRunEnds(runId);
  const blocking = useBlockingRun(batch.sapSystemId, run.error);
  const busy = isApiError(run.error) && run.error.status === 409;
  const active = !!runs.find((r) => isRunActive(r.status));

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end gap-3">
        {extra}
        <Button
          disabled={disabled || run.isPending || active}
          onClick={() => run.mutate(seq ? { seq } : undefined, { onSuccess: (r) => setStarted(r.id) })}
        >
          {label}
        </Button>
      </div>
      <ApiErrorAlert error={busy ? null : run.error} />
      {busy && isApiError(run.error) && (
        <Alert tone="warning" title={run.error.title}>
          {blocking ? (
            <p className="mt-1 text-sm">
              <Link className="font-medium underline" to={`/execucoes/${blocking.id}`}>
                {t("busy.open", { kind: t(`runs:kind.${blocking.kind}`), by: blocking.startedBy })}
              </Link>
            </p>
          ) : null}
        </Alert>
      )}
      {runId && <RunMonitorPanel runId={runId} onRetried={(r) => setStarted(r.id)} />}
    </div>
  );
}
