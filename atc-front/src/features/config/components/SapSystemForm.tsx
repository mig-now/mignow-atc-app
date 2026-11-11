import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";
import { ProjectType, type SapSystemResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { Checkbox, Input, NativeSelect } from "@/components/ui/input";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { applyServerErrors, orNull } from "@/features/shared/form";
import { useCustomers } from "../hooks/useCustomers";
import { useSaveSapSystem } from "../hooks/useSapSystems";

const isSim = (host: string) => host.trim().toLowerCase().startsWith("sim://");

/** Senha só na criação (e não no simulador `sim://`); depois, "Trocar senha". */
const makeSchema = (isEdit: boolean) =>
  z
    .object({
      customerId: z.string().min(1),
      sid: z.string().trim().min(1).max(8),
      client: z.string().regex(/^\d{3}$/),
      host: z.string().trim().min(1).max(200),
      adtPort: z.number().int().min(1).max(65535),
      https: z.boolean(),
      language: z.string().trim().length(2),
      certThumbprint: z.string().trim().max(128),
      isProduction: z.boolean(),
      projectType: z.enum(ProjectType),
      author: z
        .string()
        .trim()
        .regex(/^AUT-[A-Za-z0-9]{2,}$/),
      defaultRequest: z.string().trim().max(20),
      exemptionApprover: z.string().trim().max(12),
      username: z.string().trim().min(1).max(40),
      password: z.string(),
      active: z.boolean(),
    })
    .superRefine((v, ctx) => {
      if (!isEdit && !isSim(v.host) && v.password.length === 0)
        ctx.addIssue({ code: "too_small", origin: "string", minimum: 1, input: v.password, path: ["password"] });
    });
type Values = z.infer<ReturnType<typeof makeSchema>>;

const FIELDS = [
  "customerId",
  "sid",
  "client",
  "host",
  "adtPort",
  "language",
  "certThumbprint",
  "author",
  "defaultRequest",
  "exemptionApprover",
  "username",
  "password",
] as const;

export function SapSystemForm({
  system,
  defaultCustomerId,
  onDone,
}: {
  system?: SapSystemResponse;
  defaultCustomerId?: string;
  onDone: () => void;
}) {
  const { t } = useTranslation("config");
  const toast = useToast();
  const save = useSaveSapSystem();
  const customers = useCustomers({ page: 1, pageSize: 200, active: true });
  const form = useForm<Values>({
    resolver: zodResolver(makeSchema(!!system)),
    defaultValues: {
      customerId: system?.customerId ?? defaultCustomerId ?? "",
      sid: system?.sid ?? "",
      client: system?.client ?? "",
      host: system?.host ?? "",
      adtPort: system?.adtPort ?? 8001,
      https: system?.https ?? false,
      language: system?.language ?? "PT",
      certThumbprint: system?.certThumbprint ?? "",
      isProduction: system?.isProduction ?? false,
      projectType: system?.projectType ?? ProjectType.Conversion,
      author: system?.author ?? "",
      defaultRequest: system?.defaultRequest ?? "",
      exemptionApprover: system?.exemptionApprover ?? "",
      username: system?.username ?? "",
      password: "",
      active: system?.active ?? true,
    },
  });
  const { errors } = form.formState;
  const host = form.watch("host");
  const isProduction = form.watch("isProduction");
  const onSubmit = form.handleSubmit(async (v) => {
    const common = {
      sid: v.sid.toUpperCase(),
      client: v.client,
      host: v.host,
      adtPort: v.adtPort,
      https: v.https,
      language: v.language.toUpperCase(),
      certThumbprint: orNull(v.certThumbprint),
      isProduction: v.isProduction,
      projectType: v.projectType,
      author: v.author.toUpperCase(),
      defaultRequest: orNull(v.defaultRequest),
      exemptionApprover: orNull(v.exemptionApprover),
      username: v.username,
    };
    try {
      if (system) await save.mutateAsync({ id: system.id, input: { ...common, active: v.active } });
      else await save.mutateAsync({ input: { ...common, customerId: v.customerId, password: orNull(v.password) } });
      toast({ tone: "success", title: t("common:common.saved") });
      onDone();
    } catch (e) {
      applyServerErrors(e, form.setError, FIELDS);
    }
  });
  const err = (k: keyof Values) => errors[k]?.message;
  const text = (id: keyof Values, label: string, extra?: { hint?: string; type?: string; className?: string }) => (
    <Field id={`sap-${id}`} label={label} error={err(id)} hint={extra?.hint}>
      <Input
        id={`sap-${id}`}
        type={extra?.type}
        autoComplete={id === "password" ? "new-password" : "off"}
        aria-invalid={!!errors[id]}
        {...form.register(id as Exclude<keyof Values, "adtPort" | "https" | "isProduction" | "active">)}
      />
    </Field>
  );
  return (
    <form onSubmit={onSubmit} noValidate className="space-y-4">
      <ApiErrorAlert error={save.error} />
      {!system && (
        <Field id="sap-customerId" label={t("sapSystems.customer")} error={err("customerId")}>
          <NativeSelect id="sap-customerId" aria-invalid={!!errors.customerId} {...form.register("customerId")}>
            <option value="">{t("common:common.select")}</option>
            {customers.data?.items.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </NativeSelect>
        </Field>
      )}
      <div className="grid gap-4 sm:grid-cols-3">
        {text("sid", t("sapSystems.sid"))}
        {text("client", t("sapSystems.client"))}
        {text("language", t("sapSystems.language"))}
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        <div className="sm:col-span-2">{text("host", t("sapSystems.host"), { hint: t("sapSystems.hostHint") })}</div>
        <Field id="sap-adtPort" label={t("sapSystems.adtPort")} error={err("adtPort")}>
          <Input
            id="sap-adtPort"
            type="number"
            aria-invalid={!!errors.adtPort}
            {...form.register("adtPort", { valueAsNumber: true })}
          />
        </Field>
      </div>
      {isSim(host) && <p className="text-xs text-muted-foreground">{t("sapSystems.simulatorHint")}</p>}
      <label className="flex items-center gap-2 text-sm">
        <Checkbox {...form.register("https")} />
        {t("sapSystems.https")}
      </label>
      {text("certThumbprint", t("sapSystems.certThumbprint"), { hint: t("sapSystems.certThumbprintHint") })}
      <div className="grid gap-4 sm:grid-cols-2">
        <Field id="sap-projectType" label={t("sapSystems.projectType")} error={err("projectType")}>
          <NativeSelect id="sap-projectType" {...form.register("projectType")}>
            {Object.values(ProjectType).map((p) => (
              <option key={p} value={p}>
                {t(`sapSystems.projectType.${p}`)}
              </option>
            ))}
          </NativeSelect>
        </Field>
        {text("author", t("sapSystems.author"), { hint: t("sapSystems.authorHint") })}
        {text("defaultRequest", t("sapSystems.defaultRequest"))}
        {text("exemptionApprover", t("sapSystems.exemptionApprover"))}
        {text("username", t("sapSystems.username"))}
        {!system &&
          text("password", t("sapSystems.password"), { type: "password", hint: t("sapSystems.passwordHint") })}
      </div>
      <label className="flex items-center gap-2 text-sm">
        <Checkbox {...form.register("isProduction")} />
        {t("sapSystems.isProduction")}
      </label>
      {isProduction && <p className="text-xs text-warning">{t("sapSystems.productionHint")}</p>}
      {system && (
        <label className="flex items-center gap-2 text-sm">
          <Checkbox {...form.register("active")} />
          {t("common:common.active")}
        </label>
      )}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onDone}>
          {t("common:actions.cancel")}
        </Button>
        <Button type="submit" disabled={save.isPending}>
          {t("common:actions.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
