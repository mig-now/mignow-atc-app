import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { DdicCreateParameter, ItemResponse, RunResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox, Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert, attempt, EmptyState } from "@/features/shared";
import { ConfirmApplyDialog } from "./ConfirmApplyDialog";
import { STATUS, useActivationDecision, useApplyContext, useApplyItems, useApplyRun, useSimulateRun } from "./hooks";
import { ItemApplyResult } from "./ItemApplyResult";
import { DDIC_TYPES, RESULT_TONE } from "./labels";
import { RunPanel } from "./RunPanel";

type Ddic = Record<string, { package: string; description: string }>;

/** Passo 3: simular, confirmar explicitamente, aplicar no SAP e decidir os erros de ativação. */
export function ApplySection({ sessionId, onChanged }: { sessionId: string; onChanged: () => void }) {
  const { t } = useTranslation("remediation");
  const ctx = useApplyContext(sessionId);
  const items = useApplyItems(sessionId);
  const simulate = useSimulateRun(sessionId);
  const apply = useApplyRun(sessionId);
  const activation = useActivationDecision(sessionId);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [allowTable, setAllowTable] = useState(false);
  const [ddic, setDdic] = useState<Ddic>({});
  const [simulated, setSimulated] = useState<Set<string>>(new Set());
  const [simRunId, setSimRunId] = useState<string | null>(null);
  const [applyRunId, setApplyRunId] = useState<string | null>(null);
  const [rollbackRunId, setRollbackRunId] = useState<string | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);

  const eligible = items.items.filter((i) => (i.status === STATUS.kb || i.status === STATUS.ai) && i.hasProposal);
  const chosen = eligible.filter((i) => selected.has(i.id));
  const request = ctx.session?.request?.trim() ?? "";
  const production = ctx.isProduction;
  const allSimulated = chosen.length > 0 && chosen.every((i) => simulated.has(i.id));

  const body = () => ({
    itemIds: chosen.map((i) => i.id),
    allowTableStructure: allowTable,
    ddicCreate: chosen
      .filter((i) => DDIC_TYPES.has(i.objectType) && ddic[i.id]?.package?.trim() && ddic[i.id]?.description?.trim())
      .map(
        (i): DdicCreateParameter => ({
          itemId: i.id,
          package: ddic[i.id]?.package.trim() ?? "",
          description: ddic[i.id]?.description.trim() ?? "",
        }),
      ),
  });

  const runSimulate = () =>
    attempt(async () => {
      const run = await simulate.mutateAsync(body());
      setSimRunId(run.id);
    });
  const runApply = () =>
    attempt(async () => {
      const run = await apply.mutateAsync(body());
      setConfirmOpen(false);
      setApplyRunId(run.id);
    });
  const decide = (itemId: string, decision: "keepInactive" | "rollback") =>
    attempt(async () => {
      const r = await activation.mutateAsync({ itemId, decision });
      if (r.run) setRollbackRunId(r.run.id);
    });
  const simDone = (run: RunResponse) => {
    if (run.status === "Completed") setSimulated(new Set(chosen.map((i) => i.id)));
    onChanged();
  };

  const toggle = (i: ItemResponse) =>
    setSelected((s) => {
      const next = new Set(s);
      if (next.has(i.id)) next.delete(i.id);
      else next.add(i.id);
      return next;
    });
  const showResults = eligible.filter((i) => (selected.has(i.id) && simulated.has(i.id)) || !!i.applicationResult);

  if (ctx.isLoading || items.isLoading) return <Loading />;

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("apply.apply.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">{t("apply.apply.intro")}</p>
          <ApiErrorAlert error={ctx.error ?? items.error} />
          {production && (
            <Alert tone="danger" title={t("apply.confirm.productionTitle")}>
              {t("apply.apply.production")}
            </Alert>
          )}
          {!request && (
            <Alert tone="warning" title={t("apply.apply.noRequestTitle")}>
              {t("apply.apply.noRequest")}
            </Alert>
          )}
          <dl className="flex flex-wrap gap-x-6 gap-y-1 text-sm">
            <div>
              <dt className="inline text-muted-foreground">{t("apply.apply.system")}: </dt>
              <dd className="inline font-medium">{ctx.system?.displayName ?? "—"}</dd>
            </div>
            <div>
              <dt className="inline text-muted-foreground">{t("apply.apply.request")}: </dt>
              <dd className="inline font-medium">{request || "—"}</dd>
            </div>
          </dl>

          {eligible.length === 0 ? (
            <EmptyState text={t("apply.apply.empty")} />
          ) : (
            <ul className="max-h-72 divide-y divide-border overflow-auto rounded-md border border-border">
              {eligible.map((i) => (
                <li key={i.id} className="space-y-2 px-3 py-1.5 text-sm">
                  <div className="flex items-center gap-3">
                    <Checkbox id={`apply-sel-${i.id}`} checked={selected.has(i.id)} onChange={() => toggle(i)} />
                    <label htmlFor={`apply-sel-${i.id}`} className="min-w-0 flex-1 truncate">
                      #{i.n} {i.objectName}
                    </label>
                    <Badge tone={i.status === STATUS.kb ? "success" : "primary"}>{i.status}</Badge>
                    {i.applicationResult && (
                      <Badge tone={RESULT_TONE[i.applicationResult] ?? "neutral"}>
                        {t(`apply.result.${i.applicationResult}`, { defaultValue: i.applicationResult })}
                      </Badge>
                    )}
                  </div>
                  {selected.has(i.id) && DDIC_TYPES.has(i.objectType) && (
                    <div className="grid gap-2 pl-7 sm:grid-cols-2">
                      <div className="space-y-1">
                        <Label htmlFor={`apply-ddic-pkg-${i.id}`}>{t("apply.apply.ddicPackage", { n: i.n })}</Label>
                        <Input
                          id={`apply-ddic-pkg-${i.id}`}
                          value={ddic[i.id]?.package ?? ""}
                          onChange={(e) =>
                            setDdic((d) => ({ ...d, [i.id]: { description: "", ...d[i.id], package: e.target.value } }))
                          }
                        />
                      </div>
                      <div className="space-y-1">
                        <Label htmlFor={`apply-ddic-desc-${i.id}`}>
                          {t("apply.apply.ddicDescription", { n: i.n })}
                        </Label>
                        <Input
                          id={`apply-ddic-desc-${i.id}`}
                          value={ddic[i.id]?.description ?? ""}
                          onChange={(e) =>
                            setDdic((d) => ({ ...d, [i.id]: { package: "", ...d[i.id], description: e.target.value } }))
                          }
                        />
                      </div>
                    </div>
                  )}
                </li>
              ))}
            </ul>
          )}

          <div className="flex items-start gap-2">
            <Checkbox id="apply-allow-table" checked={allowTable} onChange={(e) => setAllowTable(e.target.checked)} />
            <div>
              <Label htmlFor="apply-allow-table">{t("apply.apply.allowTable")}</Label>
              <p className="text-xs text-muted-foreground">{t("apply.apply.allowTableHint")}</p>
            </div>
          </div>

          <ApiErrorAlert error={simulate.error ?? apply.error ?? activation.error} />
          <div className="flex flex-wrap items-center gap-3">
            <Button variant="outline" disabled={chosen.length === 0 || simulate.isPending} onClick={runSimulate}>
              {t("apply.apply.simulate", { count: chosen.length })}
            </Button>
            <Button
              variant="destructive"
              disabled={!allSimulated || production || !request}
              onClick={() => setConfirmOpen(true)}
            >
              {t("apply.apply.apply", { count: chosen.length })}
            </Button>
            {chosen.length > 0 && !allSimulated && (
              <span className="text-xs text-muted-foreground">{t("apply.apply.simulateFirst")}</span>
            )}
          </div>
        </CardContent>
      </Card>

      {simRunId && <RunPanel runId={simRunId} onDone={simDone} onRetried={(r) => setSimRunId(r.id)} />}

      {showResults.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>{t("apply.apply.resultsTitle")}</CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-3">
              {showResults.map((i) => (
                <ItemApplyResult
                  key={i.id}
                  sessionId={sessionId}
                  item={i}
                  busy={activation.isPending}
                  onDecide={(d) => decide(i.id, d)}
                />
              ))}
            </ul>
          </CardContent>
        </Card>
      )}

      {applyRunId && <RunPanel runId={applyRunId} onDone={onChanged} onRetried={(r) => setApplyRunId(r.id)} />}
      {rollbackRunId && <RunPanel runId={rollbackRunId} onDone={onChanged} onRetried={(r) => setRollbackRunId(r.id)} />}

      {confirmOpen && (
        <ConfirmApplyDialog
          systemName={ctx.system?.displayName ?? ""}
          request={request}
          items={chosen}
          production={production}
          pending={apply.isPending}
          error={apply.error}
          onConfirm={runApply}
          onClose={() => setConfirmOpen(false)}
        />
      )}
    </div>
  );
}
