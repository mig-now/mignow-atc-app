import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import type { SituationCounts } from "@/api/generated/atc/model";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { RunStatusBadge } from "@/features/shared/status";
import { formatDateTime, formatMoney, formatNumber } from "@/lib/format";
import { useSessionHistory } from "../hooks/useSessions";
import { CountsBar, SITUATIONS } from "./status";

function duration(seconds: number) {
  const s = Math.max(0, Math.round(seconds));
  const h = Math.floor(s / 3600);
  const m = Math.floor((s % 3600) / 60);
  return [h ? `${h}h` : "", h || m ? `${m}min` : "", `${s % 60}s`].filter(Boolean).join(" ");
}

function CountsRow({ label, c }: { label: string; c: SituationCounts }) {
  return (
    <Tr>
      <Th scope="row">{label}</Th>
      {SITUATIONS.map((s) => (
        <Td key={s}>{c[s]}</Td>
      ))}
      <Td>{c.total}</Td>
      <Td>{c.objects}</Td>
      <Td>{c.pendingObjects}</Td>
    </Tr>
  );
}

/** Aba Histórico: antes/depois por situação, transições, linha do tempo (barras), runs e tempo líquido. */
export function HistoryTab({ sessionId }: { sessionId: string }) {
  const { t, i18n } = useTranslation("remediation");
  const lang = i18n.language;
  const q = useSessionHistory(sessionId);
  if (q.isLoading) return <Loading />;
  const h = q.data;
  if (!h) return <ApiErrorAlert error={q.error} />;
  const transitions = Object.entries(h.transitions);

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("history.beforeAfter")}</CardTitle>
        </CardHeader>
        <Table>
          <THead>
            <Tr>
              <Th />
              {SITUATIONS.map((s) => (
                <Th key={s}>{t(`situation.${s}`)}</Th>
              ))}
              <Th>{t("history.total")}</Th>
              <Th>{t("history.objects")}</Th>
              <Th>{t("history.pendingObjects")}</Th>
            </Tr>
          </THead>
          <TBody>
            <CountsRow label={t("history.before")} c={h.before} />
            <CountsRow label={t("history.after")} c={h.after} />
          </TBody>
        </Table>
        <CardContent>
          <p className="text-sm">{t("history.handled", { count: h.handled })}</p>
        </CardContent>
      </Card>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>{t("history.transitions")}</CardTitle>
          </CardHeader>
          <CardContent>
            {transitions.length === 0 ? (
              <p className="text-sm text-muted-foreground">{t("history.noTransitions")}</p>
            ) : (
              <ul className="space-y-1 text-sm">
                {transitions.map(([k, v]) => (
                  <li key={k} className="flex justify-between gap-2">
                    <span>{k}</span>
                    <span className="font-medium">{v}</span>
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>{t("history.time")}</CardTitle>
          </CardHeader>
          <CardContent>
            <dl className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <dt className="text-xs text-muted-foreground">{t("history.netTime")}</dt>
                <dd>{duration(h.netSeconds)}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">{t("history.wallTime")}</dt>
                <dd>{duration(h.wallSeconds)}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">{t("history.tokens")}</dt>
                <dd>
                  {formatNumber(h.aiInputTokens, lang)} / {formatNumber(h.aiOutputTokens, lang)}
                </dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">{t("history.cost")}</dt>
                <dd>{formatMoney(h.aiCost, lang)}</dd>
              </div>
            </dl>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t("history.timeline")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {h.timeline.length === 0 ? (
            <p className="text-sm text-muted-foreground">{t("history.noTimeline")}</p>
          ) : (
            h.timeline.map((p) => (
              <div key={p.date} className="space-y-1">
                <p className="text-xs text-muted-foreground">{formatDateTime(p.date, lang)}</p>
                <CountsBar counts={p.situation} />
              </div>
            ))
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t("history.runs")}</CardTitle>
        </CardHeader>
        {h.runs.length === 0 ? (
          <CardContent>
            <p className="text-sm text-muted-foreground">{t("history.noRuns")}</p>
          </CardContent>
        ) : (
          <Table>
            <THead>
              <Tr>
                <Th>{t("history.run.kind")}</Th>
                <Th>{t("history.run.status")}</Th>
                <Th>{t("history.run.startedAt")}</Th>
                <Th>{t("history.run.tokens")}</Th>
                <Th>{t("history.run.cost")}</Th>
              </Tr>
            </THead>
            <TBody>
              {h.runs.map((r) => (
                <Tr key={r.id}>
                  <Td>
                    <Link className="text-primary hover:underline" to={`/execucoes/${r.id}`}>
                      {t(`runs:kind.${r.kind}`)}
                    </Link>
                  </Td>
                  <Td>
                    <RunStatusBadge status={r.status} />
                  </Td>
                  <Td>{formatDateTime(r.startedAt ?? r.queuedAt, lang)}</Td>
                  <Td>
                    {formatNumber(r.aiInputTokens, lang)} / {formatNumber(r.aiOutputTokens, lang)}
                  </Td>
                  <Td>{formatMoney(r.aiCost, lang)}</Td>
                </Tr>
              ))}
            </TBody>
          </Table>
        )}
      </Card>
    </div>
  );
}
