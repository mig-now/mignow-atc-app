import { useTranslation } from "react-i18next";
import type { ImportSummary } from "@/api/generated/atc/model";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <div>
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="text-lg font-semibold">{value}</dd>
    </div>
  );
}

/** Resumo da importação do CSV: linhas, ignoradas, fora e dentro do escopo. */
export function ImportSummaryCard({ summary: s }: { summary: ImportSummary }) {
  const { t } = useTranslation("remediation");
  const rows = Object.values(s.byPriorityInCsv).reduce((a, b) => a + b, 0) + s.ignoredRows;
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("import.summary")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <dl className="grid grid-cols-2 gap-3 sm:grid-cols-5">
          <Stat label={t("import.rows")} value={rows} />
          <Stat label={t("import.ignored")} value={s.ignoredRows} />
          <Stat label={t("import.outOfScope")} value={s.outOfScope} />
          <Stat label={t("import.inScope")} value={s.items} />
          <Stat label={t("import.objects")} value={s.distinctObjects} />
        </dl>
        {s.shiftedColumns > 0 && (
          <p className="text-sm text-warning">{t("import.shifted", { count: s.shiftedColumns })}</p>
        )}
        <p className="text-sm text-muted-foreground">
          {t("import.byPriority")}:{" "}
          {Object.entries(s.byPriorityInCsv)
            .map(([p, n]) => `P${p} ${n}`)
            .join(" · ") || "—"}
        </p>
      </CardContent>
    </Card>
  );
}
