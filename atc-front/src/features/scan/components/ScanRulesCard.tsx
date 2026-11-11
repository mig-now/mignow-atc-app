import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import type { ScanResponse } from "@/api/generated/atc/model";
import { Badge, type BadgeTone } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { useScanRules, useSetScope } from "../hooks/useScans";

const SEVERITY: Record<string, BadgeTone> = { ALTO: "danger", MEDIO: "warning", BAIXO: "neutral" };

/** Regras do catálogo (GET scans/rules) com seleção -> PUT scope. Só editável sem Run ativo. */
export function ScanRulesCard({ scan, locked }: { scan: ScanResponse; locked: boolean }) {
  const { t } = useTranslation("scan");
  const rules = useScanRules();
  const setScope = useSetScope(scan.id);
  const [selected, setSelected] = useState<Set<string>>(new Set(scan.activeRuleIds));
  useEffect(() => setSelected(new Set(scan.activeRuleIds)), [scan.activeRuleIds]);

  const list = rules.data?.rules ?? [];
  const dirty = selected.size !== scan.activeRuleIds.length || scan.activeRuleIds.some((id) => !selected.has(id));
  const toggle = (id: string) =>
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("rules.title")}</CardTitle>
        <div className="flex items-center gap-2">
          <span className="text-xs text-muted-foreground">
            {t("rules.selected", { n: selected.size, total: list.length })}
          </span>
          <Button
            size="sm"
            disabled={!dirty || locked || selected.size === 0 || setScope.isPending}
            onClick={() => setScope.mutate([...selected])}
          >
            {t("rules.save")}
          </Button>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {rules.data ? (
          <p className="text-xs text-muted-foreground">
            {t("rules.version", { version: rules.data.version ?? "—", date: rules.data.date ?? "—" })}
          </p>
        ) : null}
        {setScope.error ? <ApiErrorAlert error={setScope.error} /> : null}
        {rules.isLoading ? (
          <Loading />
        ) : rules.error ? (
          <ApiErrorAlert error={rules.error} />
        ) : (
          <div className="max-h-96 overflow-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th className="w-10">
                    <span className="sr-only">{t("rules.use")}</span>
                  </Th>
                  <Th>{t("rules.id")}</Th>
                  <Th>{t("rules.category")}</Th>
                  <Th>{t("rules.severity")}</Th>
                  <Th>{t("rules.description")}</Th>
                  <Th>{t("rules.default")}</Th>
                </Tr>
              </THead>
              <TBody>
                {list.map((r) => (
                  <Tr key={r.id}>
                    <Td>
                      <Checkbox
                        aria-label={t("rules.useRule", { id: r.id })}
                        checked={selected.has(r.id)}
                        disabled={locked}
                        onChange={() => toggle(r.id)}
                      />
                    </Td>
                    <Td className="font-mono">{r.id}</Td>
                    <Td>{r.category}</Td>
                    <Td>
                      <Badge tone={SEVERITY[r.severity] ?? "neutral"}>{r.severity}</Badge>
                    </Td>
                    <Td className="min-w-64">{r.description}</Td>
                    <Td>{r.active ? t("rules.active") : t("rules.inactive")}</Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
