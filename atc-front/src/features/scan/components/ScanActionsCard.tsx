import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ScanResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Checkbox, Input } from "@/components/ui/input";
import { RunMonitorPanel } from "@/features/runs/components/RunMonitorPanel";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { useScanActions } from "../hooks/useScans";

/** Simular -> Executar (progresso por lote no monitor do Run) e demais ações que disparam Run. */
export function ScanActionsCard({
  scan,
  runId,
  active,
  onRun,
}: {
  scan: ScanResponse;
  runId: string;
  active: boolean;
  onRun: (runId: string) => void;
}) {
  const { t } = useTranslation("scan");
  const actions = useScanActions(scan.id);
  const [lots, setLots] = useState("");
  const [parallel, setParallel] = useState("");
  const [redo, setRedo] = useState(false);
  const [includeOtherLine, setIncludeOtherLine] = useState(false);
  const [compareOriginal, setCompareOriginal] = useState(true);
  const par = parallel.trim() === "" ? undefined : Math.max(1, Number(parallel) || 1);
  const error = actions.simulate.error ?? actions.execute.error ?? actions.cleanup.error ?? actions.inventory.error;
  const busy =
    active ||
    actions.simulate.isPending ||
    actions.execute.isPending ||
    actions.cleanup.isPending ||
    actions.inventory.isPending;
  const hasInventory = scan.batches.length > 0;
  const go = { onSuccess: (r: { id: string }) => onRun(r.id) };

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("actions.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground">{t("actions.flow")}</p>
        <div className="grid gap-3 md:grid-cols-3">
          <Field id="scan-lots" label={t("actions.lots")} hint={t("actions.lotsHint")}>
            <Input id="scan-lots" value={lots} onChange={(e) => setLots(e.target.value)} placeholder="1-3,5" />
          </Field>
          <Field id="scan-parallel" label={t("actions.parallel")} hint={t("actions.parallelHint")}>
            <Input
              id="scan-parallel"
              type="number"
              min={1}
              value={parallel}
              onChange={(e) => setParallel(e.target.value)}
            />
          </Field>
          <div className="space-y-2 pt-6 text-sm">
            <label className="flex items-center gap-2">
              <Checkbox checked={redo} onChange={(e) => setRedo(e.target.checked)} />
              {t("actions.redo")}
            </label>
            <label className="flex items-center gap-2">
              <Checkbox checked={includeOtherLine} onChange={(e) => setIncludeOtherLine(e.target.checked)} />
              {t("actions.includeOtherLine")}
            </label>
            <label className="flex items-center gap-2">
              <Checkbox checked={compareOriginal} onChange={(e) => setCompareOriginal(e.target.checked)} />
              {t("actions.compareOriginal")}
            </label>
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            disabled={busy || !hasInventory}
            onClick={() => actions.simulate.mutate({ lots: lots.trim() || undefined, parallel: par }, go)}
          >
            {t("actions.simulate")}
          </Button>
          <Button
            disabled={busy || !hasInventory}
            onClick={() =>
              actions.execute.mutate(
                { lots: lots.trim() || null, redo, parallel: par ?? null, includeOtherLine, compareOriginal },
                go,
              )
            }
          >
            {t("actions.execute")}
          </Button>
          <Button variant="outline" disabled={busy} onClick={() => actions.inventory.mutate(false, go)}>
            {t("actions.inventory")}
          </Button>
        </div>
        {error ? <ApiErrorAlert error={error} /> : null}
        {runId ? (
          <div className="border-t border-border pt-4">
            <RunMonitorPanel runId={runId} onRetried={(r) => onRun(r.id)} />
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}
