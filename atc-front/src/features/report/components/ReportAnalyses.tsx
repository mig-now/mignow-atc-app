import { ExternalLink } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { getReportsGetAnalysisHtmlUrl } from "@/api/generated/atc/reports/reports";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { openHtml } from "@/features/scan/lib/download";
import { formatNumber } from "@/lib/format";
import { ANALYSIS_KINDS, type AnalysisKind, useAnalysis } from "../hooks/useReport";

function AnalysisCard({
  customerId,
  sapSystemId,
  kind,
}: {
  customerId: string;
  sapSystemId: string;
  kind: AnalysisKind;
}) {
  const { t, i18n } = useTranslation("report");
  const q = useAnalysis(customerId, kind, sapSystemId);
  const [failed, setFailed] = useState(false);
  const d = q.data;
  const fmt = (n: number) => formatNumber(n, i18n.language);
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t(`analyses.${kind}`)}</CardTitle>
        <Button
          size="sm"
          variant="outline"
          disabled={!d}
          onClick={() => {
            setFailed(false);
            openHtml(getReportsGetAnalysisHtmlUrl(customerId, kind, sapSystemId ? { sapSystemId } : undefined)).catch(
              () => setFailed(true),
            );
          }}
        >
          <ExternalLink aria-hidden /> {t("analyses.open")}
        </Button>
      </CardHeader>
      <CardContent className="space-y-2">
        <p className="text-sm text-muted-foreground">{t(`analyses.${kind}Hint`)}</p>
        {q.isLoading ? (
          <Loading />
        ) : d ? (
          <dl className="grid grid-cols-2 gap-3 text-sm md:grid-cols-4">
            <div>
              <dt className="text-xs text-muted-foreground">{t("analyses.items")}</dt>
              <dd className="font-semibold tabular-nums">{fmt(d.items)}</dd>
            </div>
            <div>
              <dt className="text-xs text-muted-foreground">{t("analyses.objects")}</dt>
              <dd className="font-semibold tabular-nums">{fmt(d.objects)}</dd>
            </div>
            <div>
              <dt className="text-xs text-muted-foreground">{t("analyses.groups")}</dt>
              <dd className="font-semibold tabular-nums">{fmt(d.groups)}</dd>
            </div>
            <div>
              <dt className="text-xs text-muted-foreground">{t("analyses.withCode")}</dt>
              <dd className="font-semibold tabular-nums">{fmt(d.withCode)}</dd>
            </div>
          </dl>
        ) : (
          <p className="text-sm text-muted-foreground">{t("analyses.unavailable")}</p>
        )}
        {failed ? <Alert tone="danger">{t("analyses.openFailed")}</Alert> : null}
      </CardContent>
    </Card>
  );
}

/** Análises complementares (isenção, pendências, MIGNOW): resumo JSON e abertura do HTML. */
export function ReportAnalyses({ customerId, sapSystemId }: { customerId: string; sapSystemId: string }) {
  return (
    <div className="space-y-4">
      {ANALYSIS_KINDS.map((k) => (
        <AnalysisCard key={k} customerId={customerId} sapSystemId={sapSystemId} kind={k} />
      ))}
    </div>
  );
}
