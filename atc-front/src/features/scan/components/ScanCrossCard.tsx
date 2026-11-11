import { useTranslation } from "react-i18next";
import type { ScanResponse } from "@/api/generated/atc/model";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatNumber } from "@/lib/format";
import { summaryOf } from "../hooks/useScans";
import { type Bar, HBars } from "./HBars";

export const CROSS_CLASSES = [
  { key: "ATC_PEGOU", tone: "success" },
  { key: "ATC_OUTRA_LINHA", tone: "info" },
  { key: "MIGNOW_FONTE_COM_FINDING", tone: "warning" },
  { key: "MIGNOW_FONTE_SEM_FINDING", tone: "destructive" },
] as const;

/** Resumo do cruzamento com o ATC: pendentes por classe (gráfico), remediadas (R) e informativas (I). */
export function ScanCrossCard({ scan }: { scan: ScanResponse }) {
  const { t, i18n } = useTranslation("scan");
  const lang = i18n.language;
  const sum = summaryOf(scan);
  const fmt = (n: number) => formatNumber(n, lang);
  const bars: Bar[] = CROSS_CLASSES.map((c) => ({
    key: c.key,
    label: t(`cross.${c.key}`),
    tone: c.tone,
    value: sum?.cruzamento_pendentes?.[c.key] ?? 0,
  }));
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("cross.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {!sum ? (
          <p className="text-sm text-muted-foreground">{t("cross.pending")}</p>
        ) : (
          <>
            <dl className="grid grid-cols-2 gap-4 md:grid-cols-5">
              <Total label={t("cross.total")} value={fmt(sum.ocorrencias?.total ?? 0)} />
              <Total label={t("cross.pendente")} value={fmt(sum.ocorrencias?.pendente ?? 0)} />
              <Total label={t("cross.remediated")} value={fmt(sum.ocorrencias?.ja_remediado ?? 0)} />
              <Total label={t("cross.informative")} value={fmt(sum.ocorrencias?.informativo ?? 0)} />
              <Total label={t("cross.mignow")} value={fmt(sum.mignow?.ocorrencias ?? 0)} />
            </dl>
            <div>
              <h3 className="mb-2 text-base font-semibold">{t("cross.chart")}</h3>
              <HBars bars={bars} label={t("cross.chart")} format={fmt} />
            </div>
            <p className="text-xs text-muted-foreground">{t("cross.note")}</p>
          </>
        )}
      </CardContent>
    </Card>
  );
}

function Total({ label, value }: { label: string; value: string }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="text-lg font-semibold tabular-nums">{value}</dd>
    </div>
  );
}
