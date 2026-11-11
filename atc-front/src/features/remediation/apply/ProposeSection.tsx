import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { RemediationGroup, RunResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox, NativeSelect } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert, attempt, EmptyState, Markdown } from "@/features/shared";
import { type ProposeResult, parseProposeResult, STATUS, useApplyGroups, useApplyItems, useProposeRun } from "./hooks";
import { RunPanel } from "./RunPanel";

const toggle = <T,>(set: Set<T>, value: T) => {
  const next = new Set(set);
  if (next.has(value)) next.delete(value);
  else next.add(value);
  return next;
};

function GroupRow({ group, checked, onToggle }: { group: RemediationGroup; checked: boolean; onToggle: () => void }) {
  const { t } = useTranslation("remediation");
  const id = `apply-group-${group.key}`;
  return (
    <li className="flex items-start gap-3 rounded-md border border-border px-3 py-2">
      <Checkbox id={id} checked={checked} onChange={onToggle} />
      <label htmlFor={id} className="min-w-0 flex-1 text-sm">
        <span className="font-medium">
          {group.ref} <span className="text-muted-foreground">({group.refType})</span>
        </span>
        <span className="ml-2 text-xs text-muted-foreground">
          {t("apply.propose.groupItems", { count: group.items.length })} ·{" "}
          {t("apply.propose.groupObjects", { count: group.objects.length })}
        </span>
        <span className="mt-1 flex flex-wrap gap-1.5">
          <Badge>{group.complexity}</Badge>
          {group.standard && <Badge tone="warning">{t("apply.propose.standard")}</Badge>}
          {group.kbTop ? (
            <Badge tone="success">{t("apply.propose.kbTop", { id: group.kbTop.id, count: group.kbSolutions })}</Badge>
          ) : (
            <Badge tone="neutral">{t("apply.propose.noKb")}</Badge>
          )}
          {group.kbTop && group.kbTop.alerts.length > 0 && (
            <Badge tone="warning">{t("apply.propose.kbAlerts", { count: group.kbTop.alerts.length })}</Badge>
          )}
        </span>
      </label>
    </li>
  );
}

/** Passo 1: escolher grupos/itens pendentes, modo (KB ou IA), modo automático, e disparar o run Propose. */
export function ProposeSection({
  sessionId,
  autoModeSession,
  onResult,
  onDone,
}: {
  sessionId: string;
  autoModeSession: boolean;
  onResult: (r: ProposeResult) => void;
  onDone: () => void;
}) {
  const { t } = useTranslation("remediation");
  const groups = useApplyGroups(sessionId);
  const items = useApplyItems(sessionId);
  const propose = useProposeRun(sessionId);
  const [groupKeys, setGroupKeys] = useState<Set<string>>(new Set());
  const [itemIds, setItemIds] = useState<Set<string>>(new Set());
  const [mode, setMode] = useState<"kb" | "ia">("kb");
  const [auto, setAuto] = useState(false);
  const [runId, setRunId] = useState<string | null>(null);

  const pending = items.items.filter((i) => i.status === STATUS.pending);
  const selected = groupKeys.size + itemIds.size;

  const submit = () =>
    attempt(async () => {
      const run = await propose.mutateAsync({
        groupKeys: groupKeys.size ? [...groupKeys] : null,
        itemIds: itemIds.size ? [...itemIds] : null,
        mode,
        autoConfirm: auto,
      });
      setRunId(run.id);
    });

  const finished = (run: RunResponse) => {
    onResult(parseProposeResult(run.result));
    onDone();
  };

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("apply.propose.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">{t("apply.propose.intro")}</p>
          {groups.isLoading || items.isLoading ? (
            <Loading />
          ) : (
            <>
              <ApiErrorAlert error={groups.error ?? items.error} />
              <section aria-label={t("apply.propose.groups")} className="space-y-2">
                <h3 className="text-base font-semibold">{t("apply.propose.groups")}</h3>
                {groups.data && groups.data.groups.length > 0 ? (
                  <ul className="space-y-2">
                    {groups.data.groups.map((g) => (
                      <GroupRow
                        key={g.key}
                        group={g}
                        checked={groupKeys.has(g.key)}
                        onToggle={() => setGroupKeys((s) => toggle(s, g.key))}
                      />
                    ))}
                  </ul>
                ) : (
                  <EmptyState text={t("apply.propose.noGroups")} />
                )}
              </section>
              {pending.length > 0 && (
                <details className="rounded-md border border-border px-3 py-2">
                  <summary className="cursor-pointer text-sm font-semibold">
                    {t("apply.propose.individualItems", { count: pending.length })}
                  </summary>
                  <ul className="mt-2 max-h-64 space-y-1 overflow-auto">
                    {pending.map((i) => (
                      <li key={i.id} className="flex items-center gap-2 text-sm">
                        <Checkbox
                          id={`apply-item-${i.id}`}
                          checked={itemIds.has(i.id)}
                          onChange={() => setItemIds((s) => toggle(s, i.id))}
                        />
                        <label htmlFor={`apply-item-${i.id}`}>
                          #{i.n} {i.objectName} · {i.refObject ?? "—"}
                        </label>
                      </li>
                    ))}
                  </ul>
                </details>
              )}
            </>
          )}

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="apply-mode">{t("apply.propose.mode")}</Label>
              <NativeSelect id="apply-mode" value={mode} onChange={(e) => setMode(e.target.value as "kb" | "ia")}>
                <option value="kb">{t("apply.propose.modeKb")}</option>
                <option value="ia">{t("apply.propose.modeIa")}</option>
              </NativeSelect>
              <p className="text-xs text-muted-foreground">
                {mode === "kb" ? t("apply.propose.modeKbHint") : t("apply.propose.modeIaHint")}
              </p>
            </div>
            <div className="space-y-1.5">
              <div className="flex items-center gap-2">
                <Checkbox
                  id="apply-auto"
                  checked={auto}
                  disabled={!autoModeSession}
                  onChange={(e) => setAuto(e.target.checked)}
                />
                <Label htmlFor="apply-auto">{t("apply.propose.auto")}</Label>
              </div>
              {!autoModeSession && (
                <p className="text-xs text-muted-foreground">{t("apply.propose.autoUnavailable")}</p>
              )}
            </div>
          </div>

          {auto && (
            <Alert tone="warning" title={t("apply.propose.autoWarnTitle")}>
              <p>{t("apply.propose.autoWarn")}</p>
              <ul className="mt-1 list-disc pl-5">
                <li>{t("apply.propose.stop1")}</li>
                <li>{t("apply.propose.stop2")}</li>
                <li>{t("apply.propose.stop3")}</li>
                <li>{t("apply.propose.stop4")}</li>
                <li>{t("apply.propose.stop5")}</li>
              </ul>
            </Alert>
          )}

          <ApiErrorAlert error={propose.error} />
          <div className="flex items-center gap-3">
            <Button disabled={selected === 0 || propose.isPending} onClick={submit}>
              {t("apply.propose.submit")}
            </Button>
            <span className="text-sm text-muted-foreground">{t("apply.propose.selected", { count: selected })}</span>
          </div>
        </CardContent>
      </Card>

      {runId && (
        <RunPanel runId={runId} onDone={finished} onRetried={(r) => setRunId(r.id)}>
          {(run) => <ProposeResultCard run={run} />}
        </RunPanel>
      )}
    </div>
  );
}

function ProposeResultCard({ run }: { run: RunResponse }) {
  const { t } = useTranslation("remediation");
  const r = parseProposeResult(run.result);
  if (run.status !== "Completed" || (!r.reportMarkdown && !r.summary)) return null;
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("apply.propose.resultTitle")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {r.summary && (
          <dl className="grid gap-2 text-sm sm:grid-cols-3 lg:grid-cols-5">
            {Object.entries(r.summary).map(([k, v]) => (
              <div key={k}>
                <dt className="text-xs text-muted-foreground">{t(`apply.summary.${k}`, { defaultValue: k })}</dt>
                <dd className="font-medium">{v}</dd>
              </div>
            ))}
          </dl>
        )}
        {r.reportMarkdown && (
          <section aria-label={t("apply.propose.report")}>
            <h3 className="mb-2 text-base font-semibold">{t("apply.propose.report")}</h3>
            <Markdown text={r.reportMarkdown} />
          </section>
        )}
      </CardContent>
    </Card>
  );
}
