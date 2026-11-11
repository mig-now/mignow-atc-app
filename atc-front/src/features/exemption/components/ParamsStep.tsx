import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";
import type { ExemptionBatchResponse, SapSystemResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/ui/field";
import { Input, NativeSelect } from "@/components/ui/input";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { applyServerErrors, orNull } from "@/features/shared/form";
import { useUpdateParams } from "../hooks/useExemption";
import { asDiscovery, parseValidity } from "../lib";

type Values = { approver: string; reason: string; validity: string; justificationPrefix: string };

/** O aprovador não pode ser o próprio usuário SAP do ambiente (princípio dos 4 olhos); aceita "USUARIO AREA". */
export function sameAsSapUser(approver: string, username: string) {
  return approver.trim().split(/\s+/)[0]?.toUpperCase() === username.trim().toUpperCase();
}

/** Etapa 2: aprovador, motivo (padrão FPOS), validade e prefixo da justificativa. */
export function ParamsStep({
  batch,
  system,
  onSaved,
}: {
  batch: ExemptionBatchResponse;
  system: SapSystemResponse | undefined;
  onSaved: () => void;
}) {
  const { t } = useTranslation("exemption");
  const toast = useToast();
  const save = useUpdateParams(batch.id);
  const d = asDiscovery(batch.discovery);
  const reasons = d?.reasons?.length ? d.reasons : [{ id: batch.reason, title: "" }];
  const validities = (d?.validities?.length ? d.validities : [batch.validity]).map(parseValidity);
  const username = system?.username ?? "";

  const schema = z
    .object({
      approver: z.string().trim().min(1),
      reason: z.string().trim().min(1),
      validity: z.string().trim().min(1),
      justificationPrefix: z.string(),
    })
    .superRefine((v, ctx) => {
      if (username && sameAsSapUser(v.approver, username))
        ctx.addIssue({ code: "custom", path: ["approver"], message: t("params.approverSame", { user: username }) });
    });

  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      approver: batch.approver ?? system?.exemptionApprover ?? "",
      reason: batch.reason || "FPOS",
      validity: batch.validity || "U",
      justificationPrefix: batch.justificationPrefix ?? "",
    },
  });
  const { errors } = form.formState;
  const reason = form.watch("reason");
  const onSubmit = form.handleSubmit(async (v) => {
    try {
      await save.mutateAsync({
        approver: v.approver.trim(),
        reason: v.reason,
        validity: v.validity,
        justificationPrefix: orNull(v.justificationPrefix),
      });
      toast({ tone: "success", title: t("params.saved") });
      onSaved();
    } catch (e) {
      applyServerErrors(e, form.setError, ["approver", "reason", "validity", "justificationPrefix"]);
    }
  });

  return (
    <form onSubmit={onSubmit} noValidate className="max-w-2xl space-y-4">
      <p className="text-sm text-muted-foreground">{t("params.intro")}</p>
      <ApiErrorAlert error={save.error} />
      <Field
        id="ex-approver"
        label={t("params.approver")}
        error={errors.approver?.message}
        hint={t("params.approverHint", { user: username || "—" })}
      >
        <Input id="ex-approver" autoComplete="off" aria-invalid={!!errors.approver} {...form.register("approver")} />
      </Field>
      <div className="grid gap-4 sm:grid-cols-2">
        <Field id="ex-reason" label={t("params.reason")} error={errors.reason?.message} hint={t("params.reasonHint")}>
          <NativeSelect id="ex-reason" {...form.register("reason")}>
            {reasons.map((r) => (
              <option key={r.id} value={r.id}>
                {r.title ? `${r.id} — ${r.title}` : r.id}
              </option>
            ))}
          </NativeSelect>
        </Field>
        <Field id="ex-validity" label={t("params.validity")} error={errors.validity?.message}>
          <NativeSelect id="ex-validity" {...form.register("validity")}>
            {validities.map((v) => (
              <option key={v.value} value={v.value}>
                {v.label}
              </option>
            ))}
          </NativeSelect>
        </Field>
      </div>
      <Field
        id="ex-prefix"
        label={t("params.prefix")}
        hint={t("params.prefixHint", { pattern: "{MOTIVO} - POS - ATC - MIGNOW | " })}
      >
        <Input
          id="ex-prefix"
          autoComplete="off"
          placeholder={`${reason} - POS - ATC - MIGNOW | `}
          {...form.register("justificationPrefix")}
        />
      </Field>
      <p className="text-xs text-muted-foreground">
        {t("params.effective")} <span className="font-mono">{batch.effectiveJustificationPrefix}</span>
      </p>
      <Button type="submit" disabled={save.isPending}>
        {t("params.save")}
      </Button>
    </form>
  );
}
