import { Download } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { isApiError } from "@/api/client";
import type { AtcLatestResponse, RunResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { RunMonitorPanel } from "@/features/runs";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { useBlockingRun, useRefreshWhenRunEnds, useStartExtraction } from "../hooks/useExtraction";

type Summary = { situation?: string; totalCount?: number; newCount?: number };

/** O 409 `Run.EnvironmentBusy` mostra o run que bloqueia o ambiente, com link para o monitor. */
function BusyAlert({ sapSystemId, error }: { sapSystemId: string; error: unknown }) {
  const { t } = useTranslation("extraction");
  const blocking = useBlockingRun(sapSystemId, error);
  if (!isApiError(error) || error.status !== 409) return null;
  return (
    <Alert tone="warning" title={error.title}>
      {blocking ? (
        <p className="mt-1 text-sm">
          <Link className="font-medium underline" to={`/execucoes/${blocking.id}`}>
            {t("busy.open", { kind: t(`runs:kind.${blocking.kind}`), by: blocking.startedBy })}
          </Link>
        </p>
      ) : null}
    </Alert>
  );
}

/** Botão Extrair (com "forçar" e confirmação quando a execução já foi extraída) + monitor do run inline. */
export function ExtractionRunner({ sapSystemId, latest }: { sapSystemId: string; latest: AtcLatestResponse }) {
  const { t } = useTranslation("extraction");
  const start = useStartExtraction();
  const [runId, setRunId] = useState<string | undefined>();
  const [confirm, setConfirm] = useState(false);
  const run = useRefreshWhenRunEnds(runId);
  const force = latest.alreadyExtracted;

  const launch = () => {
    setConfirm(false);
    start.mutate({ sapSystemId, force }, { onSuccess: (r: RunResponse) => setRunId(r.id) });
  };
  const summary = (run?.status === "Completed" ? run.result : null) as Summary | null;
  const active = run && (run.status === "Queued" || run.status === "Running" || run.status === "Cancelling");

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("runner.title")}</CardTitle>
        <Button disabled={start.isPending || !!active} onClick={() => (force ? setConfirm(true) : launch())}>
          <Download />
          {force ? t("runner.forceExtract") : t("runner.extract")}
        </Button>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground">{force ? t("runner.hintForce") : t("runner.hint")}</p>
        <ApiErrorAlert error={isApiError(start.error) && start.error.status === 409 ? null : start.error} />
        <BusyAlert sapSystemId={sapSystemId} error={start.error} />
        {summary && (
          <Alert tone="info" title={t(`runner.situation.${summary.situation ?? "Extracted"}`)}>
            <p className="text-sm">
              {t("runner.summary", { total: summary.totalCount ?? 0, fresh: summary.newCount ?? 0 })}
            </p>
          </Alert>
        )}
        {runId && <RunMonitorPanel runId={runId} onRetried={(r) => setRunId(r.id)} />}
      </CardContent>
      <Dialog open={confirm} onOpenChange={setConfirm}>
        <DialogContent title={t("runner.confirmTitle")} description={t("runner.confirmText", { id: latest.displayId })}>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirm(false)}>
              {t("common:actions.cancel")}
            </Button>
            <Button onClick={launch}>{t("runner.confirmAction")}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  );
}
