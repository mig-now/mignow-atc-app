import { useTranslation } from "react-i18next";
import { Field } from "@/components/ui/field";
import { NativeSelect } from "@/components/ui/input";
import { useCustomerOptions, useEnvironmentOptions } from "../hooks/useExtraction";

/** Cliente → ambiente (o ambiente só lista os do cliente escolhido). */
export function EnvironmentPicker({
  customerId,
  systemId,
  onChange,
}: {
  customerId: string;
  systemId: string;
  onChange: (patch: { customerId?: string; systemId?: string }) => void;
}) {
  const { t } = useTranslation("extraction");
  const customers = useCustomerOptions();
  const systems = useEnvironmentOptions(customerId);
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <Field id="ext-customer" label={t("picker.customer")}>
        <NativeSelect
          id="ext-customer"
          value={customerId}
          onChange={(e) => onChange({ customerId: e.target.value, systemId: "" })}
        >
          <option value="">{t("picker.allCustomers")}</option>
          {customers.data?.items.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </NativeSelect>
      </Field>
      <Field id="ext-system" label={t("picker.environment")}>
        <NativeSelect id="ext-system" value={systemId} onChange={(e) => onChange({ systemId: e.target.value })}>
          <option value="">{t("picker.selectEnvironment")}</option>
          {systems.data?.items.map((s) => (
            <option key={s.id} value={s.id}>
              {customerId ? s.displayName : `${s.customerName} · ${s.displayName}`}
            </option>
          ))}
        </NativeSelect>
      </Field>
    </div>
  );
}
