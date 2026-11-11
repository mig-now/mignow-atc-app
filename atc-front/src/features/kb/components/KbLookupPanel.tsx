import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Input, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { useKbLookup } from "../hooks/useKb";
import { KbCardView } from "./KbCardView";

const ROBT = ["", "TABL", "FUNC", "CLAS", "DTEL", "TRAN", "PROG", "DOMA", "VIEW"];

/** Busca rápida "lookup" (ref + RObT -> top N soluções), igual ao kb_lookup da skill. */
export function KbLookupPanel({
  reference,
  robt,
  top,
  onSubmit,
}: {
  reference: string;
  robt: string;
  top: number;
  onSubmit: (v: { ref: string; robt: string; top: number }) => void;
}) {
  const { t } = useTranslation("kb");
  const [form, setForm] = useState({ ref: reference, robt, top: String(top) });
  const result = useKbLookup({ ref: reference, robt, top });
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("lookup.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <p className="text-sm text-muted-foreground">{t("lookup.hint")}</p>
        <form
          className="grid items-end gap-3 sm:grid-cols-[1fr_9rem_6rem_auto]"
          onSubmit={(e) => {
            e.preventDefault();
            onSubmit({ ref: form.ref.trim(), robt: form.robt, top: Math.min(20, Math.max(1, Number(form.top) || 3)) });
          }}
        >
          <Field id="kb-lookup-ref" label={t("lookup.ref")}>
            <Input id="kb-lookup-ref" value={form.ref} onChange={(e) => setForm({ ...form, ref: e.target.value })} />
          </Field>
          <Field id="kb-lookup-robt" label={t("lookup.robt")}>
            <NativeSelect
              id="kb-lookup-robt"
              value={form.robt}
              onChange={(e) => setForm({ ...form, robt: e.target.value })}
            >
              {ROBT.map((r) => (
                <option key={r} value={r}>
                  {r || t("lookup.anyRobt")}
                </option>
              ))}
            </NativeSelect>
          </Field>
          <Field id="kb-lookup-top" label={t("lookup.top")}>
            <Input
              id="kb-lookup-top"
              type="number"
              min={1}
              max={20}
              value={form.top}
              onChange={(e) => setForm({ ...form, top: e.target.value })}
            />
          </Field>
          <Button type="submit" disabled={!form.ref.trim()}>
            {t("lookup.run")}
          </Button>
        </form>
        {!reference ? null : result.isLoading ? (
          <Loading />
        ) : result.error ? (
          <ApiErrorAlert error={result.error} />
        ) : result.data && result.data.cards.length > 0 ? (
          <div className="space-y-3">
            <p className="text-xs text-muted-foreground">
              {t("lookup.total", { shown: result.data.cards.length, total: result.data.total })}
            </p>
            {result.data.cards.map((c) => (
              <KbCardView key={c.id} card={c} />
            ))}
          </div>
        ) : (
          <EmptyState text={t("lookup.empty")} />
        )}
      </CardContent>
    </Card>
  );
}
