import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";
import type { CustomerResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { Checkbox, Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { applyServerErrors, orNull } from "@/features/shared/form";
import { useSaveCustomer } from "../hooks/useCustomers";

const schema = z.object({ name: z.string().trim().min(1).max(200), code: z.string().max(40), active: z.boolean() });
type Values = z.infer<typeof schema>;

export function CustomerForm({ customer, onDone }: { customer?: CustomerResponse; onDone: () => void }) {
  const { t } = useTranslation("config");
  const toast = useToast();
  const save = useSaveCustomer();
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { name: customer?.name ?? "", code: customer?.code ?? "", active: customer?.active ?? true },
  });
  const { errors } = form.formState;
  const onSubmit = form.handleSubmit(async (v) => {
    try {
      await save.mutateAsync({ id: customer?.id, input: { name: v.name, code: orNull(v.code), active: v.active } });
      toast({ tone: "success", title: t("common:common.saved") });
      onDone();
    } catch (e) {
      applyServerErrors(e, form.setError, ["name", "code"]);
    }
  });
  return (
    <form onSubmit={onSubmit} noValidate className="space-y-4">
      <ApiErrorAlert error={save.error} />
      <Field id="customer-name" label={t("customers.name")} error={errors.name?.message}>
        <Input id="customer-name" aria-invalid={!!errors.name} {...form.register("name")} />
      </Field>
      <Field id="customer-code" label={t("customers.code")} error={errors.code?.message}>
        <Input id="customer-code" aria-invalid={!!errors.code} {...form.register("code")} />
      </Field>
      {customer && (
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
