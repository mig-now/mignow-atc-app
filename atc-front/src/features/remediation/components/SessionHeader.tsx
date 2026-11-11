import { useTranslation } from "react-i18next";
import type { SessionResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { formatDateTime } from "@/lib/format";
import { CountsBar, formatPriorities } from "./status";

function Info({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="truncate text-sm">{children ?? "—"}</dd>
    </div>
  );
}

/** Cabeçalho da sessão: cliente, ambiente, tipo de projeto, prioridades, request, autor, ciclo e contagens com barra. */
export function SessionHeader({ session: s }: { session: SessionResponse }) {
  const { t, i18n } = useTranslation("remediation");
  return (
    <Card className="mb-4">
      <CardContent className="space-y-4">
        <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <Info label={t("header.customer")}>{s.customerName}</Info>
          <Info label={t("header.system")}>{s.sapSystemName}</Info>
          <Info label={t("header.projectType")}>{t(`projectType.${s.projectType}`)}</Info>
          <Info label={t("header.priorities")}>{formatPriorities(s.priorities)}</Info>
          <Info label={t("header.request")}>{s.request}</Info>
          <Info label={t("header.author")}>{s.author}</Info>
          <Info label={t("header.cycle")}>
            {s.cycle} {s.archived && <Badge tone="neutral">{t("header.archived")}</Badge>}
          </Info>
          <Info label={t("header.source")}>
            {t(`source.${s.source}`)}
            {s.sourceRef ? ` · ${s.sourceRef}` : ""} · {formatDateTime(s.createdAt, i18n.language)}
          </Info>
        </dl>
        <CountsBar counts={s.counts} />
        <p className="text-sm text-muted-foreground">
          {t("header.totals", {
            total: s.counts.total,
            objects: s.counts.objects,
            pendingObjects: s.counts.pendingObjects,
          })}
        </p>
      </CardContent>
    </Card>
  );
}
