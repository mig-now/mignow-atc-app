import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";
import { AiProviderKind, type AiProviderResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { Checkbox, Input, NativeSelect } from "@/components/ui/input";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { applyServerErrors, orNull } from "@/features/shared/form";
import { useSaveAiProvider } from "../hooks/useAiProviders";

const schema = z.object({
  name: z.string().trim().min(1).max(120),
  kind: z.enum(AiProviderKind),
  model: z.string().trim().min(1).max(120),
  apiKey: z.string(),
  isDefault: z.boolean(),
  active: z.boolean(),
});
type Values = z.infer<typeof schema>;

/** A chave só é enviada na criação ou na troca (campo preenchido); nunca é exibida. */
export function AiProviderForm({ provider, onDone }: { provider?: AiProviderResponse; onDone: () => void }) {
  const { t } = useTranslation("config");
  const toast = useToast();
  const save = useSaveAiProvider();
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: provider?.name ?? "",
      kind: provider?.kind ?? AiProviderKind.Anthropic,
      model: provider?.model ?? "",
      apiKey: "",
      isDefault: provider?.isDefault ?? false,
      active: provider?.active ?? true,
    },
  });
  const { errors } = form.formState;
  const kind = form.watch("kind");
  const onSubmit = form.handleSubmit(async (v) => {
    try {
      await save.mutateAsync({
        id: provider?.id,
        input: { ...v, name: v.name, apiKey: orNull(v.apiKey) },
      });
      toast({ tone: "success", title: t("common:common.saved") });
      onDone();
    } catch (e) {
      applyServerErrors(e, form.setError, ["name", "model", "apiKey"]);
    }
  });
  return (
    <form onSubmit={onSubmit} noValidate className="space-y-4">
      <ApiErrorAlert error={save.error} />
      <Field id="ai-name" label={t("ai.name")} error={errors.name?.message}>
        <Input id="ai-name" aria-invalid={!!errors.name} {...form.register("name")} />
      </Field>
      <Field id="ai-kind" label={t("ai.kind")}>
        <NativeSelect id="ai-kind" {...form.register("kind")}>
          {Object.values(AiProviderKind).map((k) => (
            <option key={k} value={k}>
              {t(`ai.kind.${k}`)}
            </option>
          ))}
        </NativeSelect>
      </Field>
      <Field id="ai-model" label={t("ai.model")} error={errors.model?.message}>
        <Input id="ai-model" aria-invalid={!!errors.model} {...form.register("model")} />
      </Field>
      {kind === "Anthropic" && (
        <Field
          id="ai-apiKey"
          label={provider ? t("ai.newApiKey") : t("ai.apiKey")}
          hint={provider?.hasApiKey ? t("ai.keyKept") : t("ai.keyNeverShown")}
          error={errors.apiKey?.message}
        >
          <Input
            id="ai-apiKey"
            type="password"
            autoComplete="new-password"
            aria-invalid={!!errors.apiKey}
            {...form.register("apiKey")}
          />
        </Field>
      )}
      <label className="flex items-center gap-2 text-sm">
        <Checkbox {...form.register("isDefault")} />
        {t("ai.isDefault")}
      </label>
      <p className="text-muted-foreground text-xs">{t("ai.isDefaultHint")}</p>
      {provider && (
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
