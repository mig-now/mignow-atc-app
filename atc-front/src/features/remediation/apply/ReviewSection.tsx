import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert, attempt, EmptyState } from "@/features/shared";
import { DecisionDialog, type DecisionInput, type DecisionKind } from "./DecisionDialog";
import {
  type ProposeResult,
  useApplyGroups,
  useApplyItems,
  useDecideItems,
  useDecideProposal,
  useProposeRun,
} from "./hooks";
import { STATUS_TONE } from "./labels";
import { ProposalDetail } from "./ProposalDetail";
import { RunPanel } from "./RunPanel";

/** Passo 2: revisão das propostas por item/grupo e decisão do consultor (nada é gravado no SAP aqui). */
export function ReviewSection({
  sessionId,
  candidates,
  onChanged,
}: {
  sessionId: string;
  candidates: ProposeResult["factCandidates"];
  onChanged: () => void;
}) {
  const { t } = useTranslation("remediation");
  const toast = useToast();
  const items = useApplyItems(sessionId);
  const groups = useApplyGroups(sessionId);
  const decideProposal = useDecideProposal(sessionId);
  const decideItems = useDecideItems(sessionId);
  const propose = useProposeRun(sessionId);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [activeId, setActiveId] = useState<string | null>(null);
  const [dialog, setDialog] = useState<{ kind: DecisionKind; ids: string[] } | null>(null);
  const [iaRunId, setIaRunId] = useState<string | null>(null);

  const withProposal = items.items.filter((i) => i.hasProposal);
  const active = withProposal.find((i) => i.id === activeId) ?? null;
  const busy = decideProposal.isPending || decideItems.isPending || propose.isPending;
  const nOf = (ids: string[]) => new Set(items.items.filter((i) => ids.includes(i.id)).map((i) => i.n));
  const dialogCandidates = dialog ? candidates.filter((c) => nOf(dialog.ids).has(c.n)) : [];

  const done = () => {
    toast({ tone: "success", title: t("apply.review.saved") });
    onChanged();
  };
  const confirmKb = (ids: string[]) =>
    attempt(async () => {
      await decideProposal.mutateAsync({ itemIds: ids, decision: "ConfirmKb" });
      done();
    });
  const askIa = (ids: string[]) =>
    attempt(async () => {
      const run = await propose.mutateAsync({ itemIds: ids, mode: "ia", autoConfirm: false });
      setIaRunId(run.id);
    });
  const submitDialog = async (input: DecisionInput) => {
    if (!dialog) return;
    if (dialog.kind === "ConfirmIa") {
      await decideProposal.mutateAsync({ itemIds: dialog.ids, decision: "ConfirmIa", ...input });
    } else {
      await decideItems.mutateAsync({ itemIds: dialog.ids, decision: dialog.kind, obs: input.obs });
    }
    done();
  };
  const selectGroup = (key: string) => {
    const g = groups.data?.groups.find((x) => x.key === key);
    if (!g) return;
    const ns = new Set(g.items);
    setSelected(new Set(withProposal.filter((i) => ns.has(i.n)).map((i) => i.id)));
  };
  const ids = [...selected];

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("apply.review.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">{t("apply.review.intro")}</p>
          <ApiErrorAlert error={items.error ?? decideProposal.error ?? decideItems.error ?? propose.error} />
          {items.isLoading ? (
            <Loading />
          ) : withProposal.length === 0 ? (
            <EmptyState text={t("apply.review.empty")} />
          ) : (
            <>
              <div className="flex flex-wrap items-center gap-2">
                <NativeSelect
                  aria-label={t("apply.review.selectGroup")}
                  className="w-auto"
                  value=""
                  onChange={(e) => selectGroup(e.target.value)}
                >
                  <option value="">{t("apply.review.selectGroup")}</option>
                  {groups.data?.groups.map((g) => (
                    <option key={g.key} value={g.key}>
                      {g.ref} ({g.refType}) · {g.items.length}
                    </option>
                  ))}
                </NativeSelect>
                <Button size="sm" disabled={busy || ids.length === 0} onClick={() => confirmKb(ids)}>
                  {t("apply.review.bulkConfirmKb", { count: ids.length })}
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  disabled={busy || ids.length === 0}
                  onClick={() => setDialog({ kind: "Exemption", ids })}
                >
                  {t("apply.review.bulkExemption", { count: ids.length })}
                </Button>
                <Button
                  size="sm"
                  variant="destructive"
                  disabled={busy || ids.length === 0}
                  onClick={() => setDialog({ kind: "Cancel", ids })}
                >
                  {t("apply.review.bulkCancel", { count: ids.length })}
                </Button>
              </div>
              <ul className="max-h-72 divide-y divide-border overflow-auto rounded-md border border-border">
                {withProposal.map((i) => (
                  <li key={i.id} className="flex items-center gap-3 px-3 py-1.5 text-sm">
                    <Checkbox
                      aria-label={t("apply.review.selectItem", { n: i.n })}
                      checked={selected.has(i.id)}
                      onChange={() =>
                        setSelected((s) => {
                          const next = new Set(s);
                          if (next.has(i.id)) next.delete(i.id);
                          else next.add(i.id);
                          return next;
                        })
                      }
                    />
                    <span className="min-w-0 flex-1 truncate">
                      #{i.n} {i.objectName} · {i.refObject ?? "—"}
                    </span>
                    <Badge tone={STATUS_TONE[i.status] ?? "neutral"}>{i.status || t("apply.status.pending")}</Badge>
                    <Button
                      size="sm"
                      variant={i.id === activeId ? "default" : "outline"}
                      onClick={() => setActiveId(i.id)}
                    >
                      {t("apply.review.open", { n: i.n })}
                    </Button>
                  </li>
                ))}
              </ul>
            </>
          )}
        </CardContent>
      </Card>

      {active && (
        <Card>
          <CardContent>
            <ProposalDetail
              sessionId={sessionId}
              item={active}
              busy={busy}
              onConfirmKb={() => confirmKb([active.id])}
              onConfirmIa={() => setDialog({ kind: "ConfirmIa", ids: [active.id] })}
              onCancel={() => setDialog({ kind: "Cancel", ids: [active.id] })}
              onExemption={() => setDialog({ kind: "Exemption", ids: [active.id] })}
              onAskIa={() => askIa([active.id])}
            />
          </CardContent>
        </Card>
      )}

      {iaRunId && <RunPanel runId={iaRunId} onDone={onChanged} onRetried={(r) => setIaRunId(r.id)} />}

      {dialog && (
        <DecisionDialog
          kind={dialog.kind}
          count={dialog.ids.length}
          candidates={dialogCandidates}
          error={decideProposal.error ?? decideItems.error}
          pending={busy}
          onSubmit={submitDialog}
          onClose={() => setDialog(null)}
        />
      )}
    </div>
  );
}
