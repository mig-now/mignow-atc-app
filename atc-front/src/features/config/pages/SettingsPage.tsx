import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";
import type { SettingsResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { applyServerErrors } from "@/features/shared/form";
import { useSaveSettings, useSettings } from "../hooks/useSettings";

const int = (min: number, max: number) => z.number().int().min(min).max(max);
const schema = z.object({
  parallelism: int(1, 64),
  sourceReadParallelism: int(1, 64),
  batchSize: int(1, 10000),
  kbPath: z.string().trim().min(1).max(500),
  mignowRulesPath: z.string().trim().min(1).max(500),
  mignowParallel: int(1, 64),
  mignowWeightPerBatch: int(1, 1000000),
  manualObjectsPerDay: int(1, 10000),
  hoursPerDay: z.number().min(0.5).max(24),
});
type Values = z.infer<typeof schema>;

const NUMBERS = [
  "parallelism",
  "sourceReadParallelism",
  "batchSize",
  "mignowParallel",
  "mignowWeightPerBatch",
  "manualObjectsPerDay",
  "hoursPerDay",
] as const;
const PATHS = ["kbPath", "mignowRulesPath"] as const;

function SettingsForm({ settings }: { settings: SettingsResponse }) {
  const { t } = useTranslation("config");
  const toast = useToast();
  const save = useSaveSettings();
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: settings });
  useEffect(() => form.reset(settings), [settings, form]);
  const { errors } = form.formState;
  const onSubmit = form.handleSubmit(async (v) => {
    try {
      await save.mutateAsync(v);
      toast({ tone: "success", title: t("common:common.saved") });
    } catch (e) {
      applyServerErrors(e, form.setError, [...NUMBERS, ...PATHS]);
    }
  });
  return (
    <form onSubmit={onSubmit} noValidate className="space-y-5">
      <ApiErrorAlert error={save.error} />
      <Card>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          {NUMBERS.map((k) => (
            <Field
              key={k}
              id={`set-${k}`}
              label={t(`settings.${k}`)}
              hint={t(`settings.${k}.hint`)}
              error={errors[k]?.message}
            >
              <Input
                id={`set-${k}`}
                type="number"
                step={k === "hoursPerDay" ? "0.5" : "1"}
                aria-invalid={!!errors[k]}
                {...form.register(k, { valueAsNumber: true })}
              />
            </Field>
          ))}
        </CardContent>
      </Card>
      <Card>
        <CardContent className="grid gap-4">
          {PATHS.map((k) => (
            <Field
              key={k}
              id={`set-${k}`}
              label={t(`settings.${k}`)}
              hint={t(`settings.${k}.hint`)}
              error={errors[k]?.message}
            >
              <Input id={`set-${k}`} aria-invalid={!!errors[k]} {...form.register(k)} />
            </Field>
          ))}
        </CardContent>
      </Card>
      <Button type="submit" disabled={save.isPending}>
        {t("common:actions.save")}
      </Button>
    </form>
  );
}

/** `/config/parametros`: GeneralSettings (paralelismo, lote, caminhos da KB/regras, estimativa manual). */
export function SettingsPage() {
  const query = useSettings();
  return (
    <>
      <ApiErrorAlert error={query.error} />
      {query.isLoading || !query.data ? query.error ? null : <Loading /> : <SettingsForm settings={query.data} />}
    </>
  );
}
