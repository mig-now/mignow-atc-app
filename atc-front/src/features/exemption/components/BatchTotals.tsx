import { useTranslation } from "react-i18next";
import type { ExemptionBatchResponse } from "@/api/generated/atc/model";
import { Card } from "@/components/ui/card";
import { formatNumber } from "@/lib/format";
import { StatusBadge } from "./StatusBadge";

/** Totais por status do lote (os mesmos `totals` do GET {id}) + total de findings. */
export function BatchTotals({ batch }: { batch: ExemptionBatchResponse }) {
  const { t, i18n } = useTranslation("exemption");
  const entries = Object.entries(batch.totals).filter(([, n]) => n > 0);
  return (
    <Card>
      <dl aria-label={t("totals.title")} className="flex flex-wrap items-center gap-x-6 gap-y-3 px-4 py-3">
        <div>
          <dt className="text-xs text-muted-foreground">{t("totals.findings")}</dt>
          <dd className="text-lg font-semibold tabular-nums">
            {formatNumber(batch.quantities.findings, i18n.language)}
          </dd>
        </div>
        {entries.map(([status, n]) => (
          <div key={status}>
            <dt className="text-xs">
              <StatusBadge status={status} />
            </dt>
            <dd className="mt-1 text-lg font-semibold tabular-nums">{formatNumber(n, i18n.language)}</dd>
          </div>
        ))}
      </dl>
    </Card>
  );
}
