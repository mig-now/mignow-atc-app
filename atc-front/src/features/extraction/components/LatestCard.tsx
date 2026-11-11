import { useTranslation } from "react-i18next";
import type { AtcLatestResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatDateTime, formatNumber } from "@/lib/format";

function Info({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="truncate text-sm">{children}</dd>
    </div>
  );
}

/** "Última execução do ATC": série, display id, data, perfil, P1–P4, em andamento e se já foi extraída. */
export function LatestCard({ latest }: { latest: AtcLatestResponse }) {
  const { t, i18n } = useTranslation("extraction");
  const lang = i18n.language;
  return (
    <Card>
      <CardHeader>
        <div className="min-w-0">
          <CardTitle>{t("latest.title")}</CardTitle>
          {latest.title ? <p className="mt-1 text-xs text-muted-foreground">{latest.title}</p> : null}
        </div>
        <Badge tone={latest.alreadyExtracted ? "success" : "warning"}>
          {latest.alreadyExtracted ? t("latest.alreadyExtracted") : t("latest.notExtracted")}
        </Badge>
      </CardHeader>
      <CardContent className="space-y-4">
        <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <Info label={t("latest.series")}>{latest.series}</Info>
          <Info label={t("latest.displayId")}>{latest.displayId}</Info>
          <Info label={t("latest.executedAt")}>{formatDateTime(latest.executedAtUtc, lang)}</Info>
          <Info label={t("latest.profile")}>{latest.checkProfile ?? "—"}</Info>
          <Info label={t("latest.scheduledBy")}>{latest.scheduledBy ?? "—"}</Info>
          <Info label={t("latest.planningErrors")}>{formatNumber(latest.planningErrors, lang)}</Info>
        </dl>
        <ul className="flex flex-wrap gap-2" aria-label={t("latest.priorities")}>
          {(
            [
              ["P1", latest.prio1, "danger"],
              ["P2", latest.prio2, "warning"],
              ["P3", latest.prio3, "info"],
              ["P4", latest.prio4, "neutral"],
            ] as const
          ).map(([label, value, tone]) => (
            <li key={label}>
              <Badge tone={tone}>
                {label}: {formatNumber(value, lang)}
              </Badge>
            </li>
          ))}
        </ul>
        {latest.inProgress.length > 0 && (
          <div className="rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-sm">
            <p className="font-medium">{t("latest.inProgress", { count: latest.inProgress.length })}</p>
            <ul className="mt-1 text-sm text-muted-foreground">
              {latest.inProgress.map((r) => (
                <li key={r.displayId}>
                  {r.series} · {r.displayId}
                  {r.scheduledAtUtc ? ` · ${formatDateTime(r.scheduledAtUtc, lang)}` : ""}
                </li>
              ))}
            </ul>
            <p className="mt-1 text-sm">{t("latest.inProgressHint")}</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
