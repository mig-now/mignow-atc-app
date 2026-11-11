import { Play } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { RunResponse, SessionResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Checkbox, Input } from "@/components/ui/input";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { TriageScopeBadge, useTriageRules } from "@/features/triage";
import { useSetSessionRequest, useStartAutoRemediation } from "../hooks";

const STEPS = ["sources", "triage", "propose", "confirm", "apply", "retry", "report"] as const;

/**
 * Confirmação do play: o que vai acontecer, as travas e onde grava (ambiente e request). É a confirmação explícita da gravação no SAP
 * (regra do projeto): sem marcar o "confirmo", não inicia.
 */
export function AutoRemediationDialog({
  session,
  itemIds,
  open,
  onOpenChange,
  onStarted,
}: {
  session: SessionResponse;
  itemIds: string[];
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onStarted: (run: RunResponse) => void;
}) {
  const { t } = useTranslation("remediation");
  const start = useStartAutoRemediation(session.id);
  const [ack, setAck] = useState(false);
  const setRequest = useSetSessionRequest(session.id);
  const [request, setRequestText] = useState("");
  // regras de triagem do cadastro que valem para o tipo de projeto da sessão: só exibição (o back aplica as mesmas, sem escolha)
  const triageRules = useTriageRules(session.projectType, open);
  const rules = triageRules.data ?? [];
  const scope = itemIds.length
    ? t("auto.scopeSelected", { count: itemIds.length })
    : session.counts.applyErrors > 0
      ? t("auto.scopeAllWithErrors", { count: session.counts.pending, errors: session.counts.applyErrors })
      : t("auto.scopeAllPending", { count: session.counts.pending });

  return (
    <Dialog
      open={open}
      onOpenChange={(o) => {
        setAck(false);
        start.reset();
        onOpenChange(o);
      }}
    >
      <DialogContent title={t("auto.dialogTitle")} description={scope}>
        <div className="space-y-3 text-sm">
          <ol className="list-decimal space-y-1 pl-5">
            {STEPS.map((s) => (
              <li key={s}>{t(`auto.step.${s}`)}</li>
            ))}
          </ol>
          {rules.length > 0 && (
            <section aria-label={t("auto.triageRulesLabel")} className="space-y-1 rounded-md border p-3">
              <h4 className="text-sm font-semibold">
                {t("auto.triageRules", { projectType: t(`projectType.${session.projectType}`) })}
              </h4>
              <p className="text-xs text-muted-foreground">{t("auto.triageRulesHint")}</p>
              <ul className="grid gap-1 sm:grid-cols-2">
                {rules.map((r) => (
                  <li key={r.code} className="flex items-start gap-2 text-sm" title={r.condition}>
                    <span className="font-medium">{r.code}</span>
                    <span className="text-muted-foreground">{r.description}</span>
                    {r.scope !== "All" && <TriageScopeBadge scope={r.scope} />}
                  </li>
                ))}
              </ul>
            </section>
          )}
          <Alert tone="warning" title={t("auto.locksTitle")}>
            <ul className="mt-1 list-disc space-y-0.5 pl-5 text-sm">
              <li>{t("auto.lock.ai")}</li>
              <li>{t("auto.lock.review")}</li>
              <li>{t("auto.lock.standard")}</li>
              <li>{t("auto.lock.errors")}</li>
            </ul>
          </Alert>
          <p>{t("auto.target", { system: session.sapSystemName, request: session.request ?? "—" })}</p>
          {!session.request && (
            <div className="space-y-2">
              <Alert tone="danger" title={t("auto.noRequest")} />
              <div className="flex gap-2">
                <Input
                  aria-label={t("auto.requestLabel")}
                  placeholder={t("auto.requestLabel")}
                  maxLength={20}
                  value={request}
                  onChange={(e) => setRequestText(e.target.value)}
                />
                <Button
                  variant="outline"
                  disabled={!request.trim() || setRequest.isPending}
                  onClick={() => setRequest.mutate(request.trim())}
                >
                  {t("auto.setRequest")}
                </Button>
              </div>
              <ApiErrorAlert error={setRequest.error} />
            </div>
          )}
          <ApiErrorAlert error={start.error} />
          <label className="flex items-center gap-2">
            <Checkbox checked={ack} onChange={(e) => setAck(e.target.checked)} />
            {t("auto.ack", { request: session.request ?? "—" })}
          </label>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            {t("common:actions.cancel")}
          </Button>
          <Button
            disabled={!ack || !session.request || start.isPending}
            onClick={() =>
              start.mutate(
                { itemIds },
                {
                  onSuccess: (run) => {
                    setAck(false);
                    onStarted(run);
                  },
                },
              )
            }
          >
            <Play />
            {t("auto.start")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
