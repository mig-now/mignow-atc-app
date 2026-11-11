import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import type { ExemptionBatchCreatedResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { Checkbox, NativeSelect } from "@/components/ui/input";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { formatDateTime } from "@/lib/format";
import { useCreateBatch, useSessionOptions } from "../hooks/useExemption";
import { ListingReport } from "./ListingReport";

const PRIORITIES = [1, 2, 3] as const;

/** "Novo lote" a partir de uma sessão: escolhe prioridades e se inclui itens por observação; mostra o relatório do listar. */
export function NewBatchDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { t, i18n } = useTranslation("exemption");
  const navigate = useNavigate();
  const sessions = useSessionOptions({ archived: false });
  const create = useCreateBatch();
  const [sessionId, setSessionId] = useState("");
  const [priorities, setPriorities] = useState<number[] | null>(null);
  const [includeObs, setIncludeObs] = useState(false);
  const [created, setCreated] = useState<ExemptionBatchCreatedResponse | null>(null);

  const session = sessions.data?.items.find((s) => s.id === sessionId);
  const sessionPriorities = session?.priorities
    .split(",")
    .map(Number)
    .filter((n) => PRIORITIES.includes(n as 1 | 2 | 3));
  const selected = priorities ?? sessionPriorities ?? [1, 2, 3];
  const toggle = (p: number) =>
    setPriorities(selected.includes(p) ? selected.filter((x) => x !== p) : [...selected, p].sort());

  const close = () => {
    setCreated(null);
    setSessionId("");
    setPriorities(null);
    setIncludeObs(false);
    create.reset();
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={(o) => !o && close()}>
      <DialogContent
        wide={!!created}
        title={created ? t("newBatch.reportTitle") : t("newBatch.title")}
        description={created ? t("newBatch.reportDescription") : t("newBatch.description")}
      >
        {created ? (
          <>
            <ListingReport report={created.report} />
            <DialogFooter>
              <Button variant="outline" onClick={close}>
                {t("common:actions.close")}
              </Button>
              <Button
                onClick={() => {
                  navigate(`/isencao/${created.batch.id}`);
                  close();
                }}
              >
                {t("newBatch.open")}
              </Button>
            </DialogFooter>
          </>
        ) : (
          <form
            className="space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              create.mutate({ sessionId, priorities: selected, includeObs }, { onSuccess: setCreated });
            }}
          >
            <Field id="nb-session" label={t("newBatch.session")} hint={t("newBatch.sessionHint")}>
              <NativeSelect
                id="nb-session"
                value={sessionId}
                onChange={(e) => {
                  setSessionId(e.target.value);
                  setPriorities(null);
                }}
              >
                <option value="">{t("common:common.select")}</option>
                {sessions.data?.items.map((s) => (
                  <option key={s.id} value={s.id}>
                    {`${s.customerName} · ${s.sapSystemName} · ${t("newBatch.cycle", { n: s.cycle })} · ${formatDateTime(s.createdAt, i18n.language)}`}
                  </option>
                ))}
              </NativeSelect>
            </Field>
            <fieldset className="space-y-2">
              <legend className="text-sm font-medium">{t("newBatch.priorities")}</legend>
              <div className="flex gap-4">
                {PRIORITIES.map((p) => (
                  <label key={p} className="flex items-center gap-2 text-sm">
                    <Checkbox checked={selected.includes(p)} onChange={() => toggle(p)} />P{p}
                  </label>
                ))}
              </div>
              <p className="text-xs text-muted-foreground">{t("newBatch.prioritiesHint")}</p>
            </fieldset>
            <label className="flex items-start gap-2 text-sm">
              <Checkbox className="mt-0.5" checked={includeObs} onChange={(e) => setIncludeObs(e.target.checked)} />
              <span>
                {t("newBatch.includeObs")}
                <span className="block text-xs text-muted-foreground">{t("newBatch.includeObsHint")}</span>
              </span>
            </label>
            <ApiErrorAlert error={create.error} />
            <DialogFooter>
              <Button type="button" variant="outline" onClick={close}>
                {t("common:actions.cancel")}
              </Button>
              <Button type="submit" disabled={!sessionId || selected.length === 0 || create.isPending}>
                {t("newBatch.create")}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
}
