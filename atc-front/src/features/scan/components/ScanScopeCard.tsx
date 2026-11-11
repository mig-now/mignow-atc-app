import { useTranslation } from "react-i18next";
import type { ScanResponse } from "@/api/generated/atc/model";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatNumber } from "@/lib/format";
import { scopeOf } from "../hooks/useScans";

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="text-lg font-semibold tabular-nums">{value}</dd>
    </div>
  );
}

/** Escopo do inventário: objetos por tipo, excluídos, peso e lotes. */
export function ScanScopeCard({ scan }: { scan: ScanResponse }) {
  const { t, i18n } = useTranslation("scan");
  const lang = i18n.language;
  const scope = scopeOf(scan);
  const byType = Object.entries(scope.por_tipo ?? {}).sort((a, b) => b[1] - a[1]);
  const n = (v: number | undefined) => (v == null ? "—" : formatNumber(v, lang));
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("scope.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {scope.escopo == null ? (
          <p className="text-sm text-muted-foreground">{t("scope.pending")}</p>
        ) : (
          <>
            <dl className="grid grid-cols-2 gap-4 md:grid-cols-4">
              <Stat label={t("scope.objects")} value={n(scope.escopo)} />
              <Stat label={t("scope.batches")} value={n(scope.lotes ?? scan.batches.length)} />
              <Stat label={t("scope.weightPerBatch")} value={n(scope.peso_por_lote)} />
              <Stat label={t("scope.tadir")} value={n(scope.tadir_lidos)} />
            </dl>
            <div>
              <p className="mb-1 text-xs font-medium text-muted-foreground">{t("scope.byType")}</p>
              <ul className="flex flex-wrap gap-2 text-sm">
                {byType.map(([type, q]) => (
                  <li key={type} className="rounded-md border border-border px-2 py-1">
                    <span className="font-mono">{type}</span> · {formatNumber(q, lang)}
                  </li>
                ))}
              </ul>
            </div>
            <p className="text-xs text-muted-foreground">
              {t("scope.excluded", {
                tmp: n(scope.excluidos?.tmp),
                deleted: n(scope.excluidos?.marcados_exclusao),
                generated: n(scope.excluidos?.gerados),
              })}
              {" · "}
              {scope.incluir_gerados ? t("scope.generatedIncluded") : t("scope.generatedExcluded")}
            </p>
          </>
        )}
      </CardContent>
    </Card>
  );
}
