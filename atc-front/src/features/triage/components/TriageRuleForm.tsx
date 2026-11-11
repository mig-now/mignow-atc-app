import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";
import { type TriageRuleDefinitionResponse, TriageScope } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { Checkbox, Input, NativeSelect, Textarea } from "@/components/ui/input";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { applyServerErrors, orNull } from "@/features/shared/form";
import { useSaveTriageRule } from "../hooks/useTriageRules";

export const EFFECTS = ["Cancelado", "Confirmado KB", "Isenção", "signal"] as const;

const schema = z.object({
  code: z
    .string()
    .trim()
    .regex(/^[A-Za-z][A-Za-z0-9_]{0,39}$/),
  description: z.string().trim().min(1).max(500),
  effect: z.string(),
  scope: z.enum(TriageScope),
  active: z.boolean(),
  refObjects: z.string(),
  objectPattern: z.string().max(120),
  messagePattern: z.string().max(200),
  obs: z.string().max(500),
  parameters: z.record(z.string(), z.string()),
});
type Values = z.infer<typeof schema>;

const refsOf = (text: string) =>
  text
    .split(/[\s,;/]+/)
    .map((s) => s.trim().toUpperCase())
    .filter(Boolean);

/**
 * Regra padrão (R1..R9, produtos MIGNOW): o código (renomear), descrição, escopo, ativa, objetos (R7/R9), parâmetros da lógica e filtro adicional;
 * o efeito e a obs "Triagem <chave>: …" ficam no código. Regra nova: código, efeito e critérios (objetos referenciados, nome do objeto com curinga *,
 * texto do check/mensagem; os informados valem juntos).
 */
export function TriageRuleForm({ rule, onDone }: { rule?: TriageRuleDefinitionResponse; onDone: () => void }) {
  const { t } = useTranslation("triage");
  const toast = useToast();
  const save = useSaveTriageRule();
  const builtIn = rule?.builtIn ?? false;
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      code: rule?.code ?? "",
      description: rule?.description ?? "",
      effect: rule?.effect ?? "Isenção",
      scope: rule?.scope ?? TriageScope.All,
      active: rule?.active ?? true,
      refObjects: (rule?.refObjects ?? []).join(", "),
      objectPattern: rule?.objectPattern ?? "",
      messagePattern: rule?.messagePattern ?? "",
      obs: rule?.obs ?? "",
      parameters: Object.fromEntries(Object.entries(rule?.parameters ?? {}).map(([k, v]) => [k, v.join(", ")])),
    },
  });
  const paramNames = Object.keys(rule?.parameters ?? {});
  const { errors } = form.formState;
  const onSubmit = form.handleSubmit(async (v) => {
    const refs = refsOf(v.refObjects);
    if (!builtIn && refs.length === 0 && !v.objectPattern.trim() && !v.messagePattern.trim()) {
      form.setError("refObjects", { message: t("form.criteriaRequired") });
      return;
    }
    if (rule?.usesRefObjects && refs.length === 0) {
      form.setError("refObjects", { message: t("form.refsRequired") });
      return;
    }
    const parameters = Object.fromEntries(paramNames.map((n) => [n, refsOf(v.parameters[n] ?? "")]));
    const emptyParam = paramNames.find((n) => parameters[n]?.length === 0);
    if (emptyParam) {
      form.setError(`parameters.${emptyParam}`, { message: t("form.paramRequired") });
      return;
    }
    try {
      await save.mutateAsync({
        code: rule?.code,
        input: {
          code: v.code.trim().toUpperCase(),
          description: v.description.trim(),
          effect: v.effect,
          scope: v.scope,
          active: v.active,
          refObjects: refs,
          objectPattern: orNull(v.objectPattern),
          messagePattern: orNull(v.messagePattern),
          obs: orNull(v.obs),
          parameters: builtIn ? parameters : null,
        },
      });
      toast({ tone: "success", title: t("common:common.saved") });
      onDone();
    } catch (e) {
      applyServerErrors(e, form.setError, [
        "code",
        "description",
        "refObjects",
        "objectPattern",
        "messagePattern",
        "obs",
      ]);
    }
  });
  return (
    <form onSubmit={onSubmit} noValidate className="space-y-4">
      <ApiErrorAlert error={save.error} />
      {builtIn && <Alert tone="info" title={t("form.builtInHint")} />}
      {rule && (
        <div className="space-y-1 rounded-md border bg-muted/40 p-3 text-sm">
          <p className="font-medium">
            {t("condition")} · {t("orderOf", { order: rule.order })}
          </p>
          <p className="text-muted-foreground">{rule.condition}</p>
        </div>
      )}
      <Field
        id="tr-code"
        label={t("form.code")}
        hint={builtIn ? t("form.codeBuiltInHint", { key: rule?.key }) : t("form.codeHint")}
        error={errors.code && t("form.codeInvalid")}
      >
        <Input id="tr-code" aria-invalid={!!errors.code} {...form.register("code")} />
      </Field>
      <Field id="tr-description" label={t("description")} error={errors.description?.message}>
        <Textarea id="tr-description" rows={2} aria-invalid={!!errors.description} {...form.register("description")} />
      </Field>
      <div className="grid gap-4 sm:grid-cols-2">
        <Field id="tr-effect" label={t("effect")}>
          <NativeSelect id="tr-effect" disabled={builtIn} {...form.register("effect")}>
            {(builtIn && rule && !EFFECTS.includes(rule.effect as (typeof EFFECTS)[number])
              ? [rule.effect, ...EFFECTS]
              : EFFECTS
            ).map((e) => (
              <option key={e} value={e}>
                {t(`effect.${e}`, { defaultValue: e })}
              </option>
            ))}
          </NativeSelect>
        </Field>
        <Field id="tr-scope" label={t("scope")}>
          <NativeSelect id="tr-scope" {...form.register("scope")}>
            {Object.values(TriageScope).map((s) => (
              <option key={s} value={s}>
                {t(`scope.${s}`)}
              </option>
            ))}
          </NativeSelect>
        </Field>
      </div>
      {(!builtIn || rule?.usesRefObjects) && (
        <Field
          id="tr-refs"
          label={t("form.refObjects")}
          hint={t("form.refObjectsHint")}
          error={errors.refObjects?.message}
        >
          <Input id="tr-refs" aria-invalid={!!errors.refObjects} {...form.register("refObjects")} />
        </Field>
      )}
      {paramNames.map((n) => (
        <Field
          key={n}
          id={`tr-param-${n}`}
          label={t(`param.${n}`, { defaultValue: n })}
          hint={t("form.listHint")}
          error={errors.parameters?.[n]?.message}
        >
          <Input id={`tr-param-${n}`} aria-invalid={!!errors.parameters?.[n]} {...form.register(`parameters.${n}`)} />
        </Field>
      ))}
      {builtIn && (
        <fieldset className="space-y-3 rounded-md border p-3">
          <legend className="px-1 text-sm font-semibold">{t("form.extraFilter")}</legend>
          <p className="text-xs text-muted-foreground">{t("form.extraFilterHint")}</p>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field id="tr-object" label={t("form.objectPattern")} hint={t("form.objectPatternHint")}>
              <Input id="tr-object" {...form.register("objectPattern")} />
            </Field>
            <Field id="tr-message" label={t("form.messagePattern")} hint={t("form.messagePatternHint")}>
              <Input id="tr-message" {...form.register("messagePattern")} />
            </Field>
          </div>
        </fieldset>
      )}
      {!builtIn && (
        <>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field id="tr-object" label={t("form.objectPattern")} hint={t("form.objectPatternHint")}>
              <Input id="tr-object" {...form.register("objectPattern")} />
            </Field>
            <Field id="tr-message" label={t("form.messagePattern")} hint={t("form.messagePatternHint")}>
              <Input id="tr-message" {...form.register("messagePattern")} />
            </Field>
          </div>
          <Field id="tr-obs" label={t("form.obs")} hint={t("form.obsHint")}>
            <Input id="tr-obs" {...form.register("obs")} />
          </Field>
        </>
      )}
      <label className="flex items-center gap-2 text-sm">
        <Checkbox {...form.register("active")} />
        {t("form.active")}
      </label>
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
