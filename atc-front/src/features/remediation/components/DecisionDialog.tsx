import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ItemResponse, RemediationGroup, SessionDecision } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { NativeSelect, Textarea } from "@/components/ui/input";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { useDecide } from "../hooks/useSessions";

const DECISIONS = ["AcceptSuggestion", "Cancel", "Exemption", "Reset"] as const satisfies readonly SessionDecision[];

export type DecisionTarget = { itemIds?: string[]; groups?: RemediationGroup[]; count: number; items?: ItemResponse[] };

/** Sugestão e justificativa da proposta de cada item escolhido, para o consultor decidir sabendo o que a IA/KB recomenda. */
function Suggestions({ items }: { items: ItemResponse[] }) {
  const { t } = useTranslation("remediation");
  if (items.length === 0) return null;
  return (
    <section className="space-y-2" aria-label={t("decision.suggestions")}>
      <h4 className="text-sm font-semibold">{t("decision.suggestions")}</h4>
      <ul className="max-h-96 space-y-2 overflow-y-auto rounded-md border border-border p-2">
        {[...items]
          .sort((a, b) => a.n - b.n)
          .map((i) => (
            <li key={i.id} className="space-y-1 text-sm">
              <div className="flex flex-wrap items-center gap-2">
                <span className="font-medium">
                  #{i.n} {i.objectName}
                </span>
                {i.refObject && <span className="text-xs text-muted-foreground">{i.refObject}</span>}
                <Badge tone={i.awaitingApproval ? "primary" : "neutral"}>
                  {i.awaitingApproval
                    ? t(`suggestion.${i.proposalAction ?? "pendente"}`, { defaultValue: i.proposalAction ?? "" })
                    : t("decision.noSuggestion")}
                </Badge>
              </div>
              {i.awaitingApproval && i.proposalNote && <p className="text-muted-foreground">{i.proposalNote}</p>}
            </li>
          ))}
      </ul>
    </section>
  );
}

/**
 * Decisão por itens ou grupos: aceitar a sugestão da proposta de cada item (padrão – para os pendentes de aprovação), Cancelar,
 * Isenção (observação obrigatória) ou Reset.
 */
export function DecisionDialog({
  sessionId,
  target,
  onClose,
}: {
  sessionId: string;
  target: DecisionTarget;
  onClose: (done: boolean) => void;
}) {
  const { t } = useTranslation("remediation");
  const toast = useToast();
  const decide = useDecide(sessionId);
  const [decision, setDecision] = useState<(typeof DECISIONS)[number]>("AcceptSuggestion");
  const [obs, setObs] = useState("");
  const [touched, setTouched] = useState(false);
  const obsMissing = decision === "Exemption" && obs.trim() === "";

  const submit = () => {
    setTouched(true);
    if (obsMissing) return;
    decide.mutate(
      { decision, obs: obs.trim() || null, itemIds: target.itemIds, groups: target.groups },
      {
        onSuccess: (r) => {
          toast({
            tone: "success",
            title: r.skipped
              ? t("decision.doneSkipped", { count: r.items.length - r.skipped, skipped: r.skipped })
              : t("decision.done", { count: r.items.length, pending: r.pending }),
          });
          onClose(true);
        },
      },
    );
  };

  return (
    <Dialog open onOpenChange={(o) => !o && onClose(false)}>
      <DialogContent
        wide={(target.items?.length ?? 0) > 0}
        title={t("decision.title")}
        description={t("decision.description", { count: target.count })}
      >
        <div className="space-y-4">
          <Field id="decision-kind" label={t("decision.kind")}>
            <NativeSelect
              id="decision-kind"
              value={decision}
              onChange={(e) => setDecision(e.target.value as (typeof DECISIONS)[number])}
            >
              {DECISIONS.map((d) => (
                <option key={d} value={d}>
                  {t(`decision.kind.${d}`)}
                </option>
              ))}
            </NativeSelect>
          </Field>
          <p className="text-xs text-muted-foreground">{t(`decision.help.${decision}`)}</p>
          <Suggestions items={target.items ?? []} />
          <Field
            id="decision-obs"
            label={decision === "Exemption" ? t("decision.obsRequired") : t("decision.obs")}
            error={touched && obsMissing ? t("decision.obsMissing") : undefined}
          >
            <Textarea
              id="decision-obs"
              value={obs}
              aria-invalid={touched && obsMissing}
              onChange={(e) => setObs(e.target.value)}
            />
          </Field>
          <ApiErrorAlert error={decide.error} />
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onClose(false)}>
            {t("common:actions.cancel")}
          </Button>
          <Button disabled={decide.isPending} onClick={submit}>
            {t("decision.apply")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
