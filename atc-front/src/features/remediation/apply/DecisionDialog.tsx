import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Checkbox, Input, Textarea } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ApiErrorAlert, attempt } from "@/features/shared";
import type { ProposeResult } from "./hooks";

export type DecisionKind = "ConfirmIa" | "Cancel" | "Exemption";

export type DecisionInput = {
  obs: string | null;
  replaces: string[] | null;
  reason: string | null;
  facts: { elements: string[]; fact: string; proof?: string | null; impact?: string | null }[] | null;
};

/**
 * Diálogo das decisões que pedem mais do que um clique: Confirmar IA (substitui cartão + motivo, fatos candidatos),
 * Cancelar e Isenção (observação). Confirmar KB não precisa de diálogo.
 */
export function DecisionDialog({
  kind,
  count,
  candidates,
  error,
  pending,
  onSubmit,
  onClose,
}: {
  kind: DecisionKind;
  count: number;
  candidates: ProposeResult["factCandidates"];
  error: unknown;
  pending: boolean;
  onSubmit: (input: DecisionInput) => Promise<unknown>;
  onClose: () => void;
}) {
  const { t } = useTranslation("remediation");
  const [obs, setObs] = useState("");
  const [replaces, setReplaces] = useState("");
  const [reason, setReason] = useState("");
  const [chosen, setChosen] = useState<Set<number>>(new Set());
  const ia = kind === "ConfirmIa";
  const ids = replaces
    .split(/[,;\s]+/)
    .map((s) => s.trim())
    .filter(Boolean);
  const invalid = ia && ids.length > 0 && reason.trim() === "";

  const submit = () =>
    attempt(async () => {
      await onSubmit({
        obs: obs.trim() || null,
        replaces: ia && ids.length ? ids : null,
        reason: ia && reason.trim() ? reason.trim() : null,
        facts:
          ia && chosen.size
            ? candidates
                .filter((_, idx) => chosen.has(idx))
                .map((c) => ({ elements: [c.element], fact: c.fact, proof: c.proof ?? null, impact: c.impact ?? null }))
            : null,
      });
      onClose();
    });

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent
        title={t(`apply.decision.${kind}.title`, { count })}
        description={t(`apply.decision.${kind}.description`)}
      >
        <div className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="apply-decision-obs">{t("apply.decision.obs")}</Label>
            <Textarea id="apply-decision-obs" value={obs} onChange={(e) => setObs(e.target.value)} rows={3} />
          </div>
          {ia && (
            <>
              <div className="space-y-1.5">
                <Label htmlFor="apply-decision-replaces">{t("apply.decision.replaces")}</Label>
                <Input
                  id="apply-decision-replaces"
                  value={replaces}
                  onChange={(e) => setReplaces(e.target.value)}
                  placeholder="S02753, C-BSEG-001"
                />
                <p className="text-xs text-muted-foreground">{t("apply.decision.replacesHint")}</p>
              </div>
              {ids.length > 0 && (
                <div className="space-y-1.5">
                  <Label htmlFor="apply-decision-reason">{t("apply.decision.reason")}</Label>
                  <Textarea
                    id="apply-decision-reason"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    rows={2}
                  />
                  {invalid && <p className="text-sm text-destructive">{t("apply.decision.reasonRequired")}</p>}
                </div>
              )}
              {candidates.length > 0 && (
                <fieldset className="space-y-2">
                  <legend className="text-sm font-medium">{t("apply.decision.facts")}</legend>
                  <p className="text-xs text-muted-foreground">{t("apply.decision.factsHint")}</p>
                  {candidates.map((c, idx) => (
                    <div key={`${c.n}-${c.element}-${c.fact}`} className="flex items-start gap-2 text-sm">
                      <Checkbox
                        id={`apply-fact-${idx}`}
                        checked={chosen.has(idx)}
                        onChange={() =>
                          setChosen((s) => {
                            const next = new Set(s);
                            if (next.has(idx)) next.delete(idx);
                            else next.add(idx);
                            return next;
                          })
                        }
                      />
                      <label htmlFor={`apply-fact-${idx}`}>
                        <span className="font-medium">{c.element}</span>: {c.fact}
                        {c.proof && <span className="block text-xs text-muted-foreground">{c.proof}</span>}
                      </label>
                    </div>
                  ))}
                </fieldset>
              )}
            </>
          )}
          <ApiErrorAlert error={error} />
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            {t("apply.decision.back")}
          </Button>
          <Button
            variant={kind === "Cancel" ? "destructive" : "default"}
            disabled={pending || invalid}
            onClick={submit}
          >
            {t(`apply.decision.${kind}.confirm`)}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
