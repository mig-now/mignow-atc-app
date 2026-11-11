import { useTranslation } from "react-i18next";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { formatDateTime, formatMoney, formatNumber } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import { HISTORY_MODULES, useProcessHistory } from "../hooks/useReport";
import { formatSeconds } from "../lib/duration";

/** Histórico de processo (GET v1/history): tempo de relógio e líquido, runs e tokens, filtro por módulo (URL). */
export function ReportHistory({ customerId, sapSystemId }: { customerId: string; sapSystemId: string }) {
  const { t, i18n } = useTranslation("report");
  const lang = i18n.language;
  const [f, set] = useSearchState({ module: "" });
  const q = useProcessHistory(customerId, sapSystemId, f.module);
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("history.title")}</CardTitle>
        <NativeSelect
          className="w-52"
          aria-label={t("history.module")}
          value={f.module}
          onChange={(e) => set({ module: e.target.value })}
        >
          <option value="">{t("history.allModules")}</option>
          {HISTORY_MODULES.map((m) => (
            <option key={m} value={m}>
              {t(`history.modules.${m}`)}
            </option>
          ))}
        </NativeSelect>
      </CardHeader>
      <CardContent>
        {q.isLoading ? (
          <Loading />
        ) : q.error ? (
          <ApiErrorAlert error={q.error} />
        ) : !q.data || q.data.length === 0 ? (
          <EmptyState text={t("history.empty")} />
        ) : (
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>{t("history.stage")}</Th>
                  <Th>{t("history.module")}</Th>
                  <Th>{t("history.start")}</Th>
                  <Th className="text-right">{t("history.clock")}</Th>
                  <Th className="text-right">{t("history.net")}</Th>
                  <Th className="text-right">{t("history.runs")}</Th>
                  <Th className="text-right">{t("history.tokens")}</Th>
                  <Th className="text-right">{t("history.cost")}</Th>
                  <Th className="text-right">{t("history.parallelism")}</Th>
                </Tr>
              </THead>
              <TBody>
                {q.data.map((h) => (
                  <Tr key={`${h.module}-${h.start}-${h.subjectId ?? ""}`}>
                    <Td>{h.stage}</Td>
                    <Td>{t(`history.modules.${h.module.toLowerCase()}`, { defaultValue: h.module })}</Td>
                    <Td>{formatDateTime(h.start, lang)}</Td>
                    <Td className="text-right tabular-nums">{formatSeconds(h.clockSeconds)}</Td>
                    <Td className="text-right tabular-nums">{formatSeconds(h.netSeconds)}</Td>
                    <Td className="text-right tabular-nums">
                      {h.runsCompleted}/{h.runs}
                      {h.runsFailed > 0 ? ` (${t("history.failed", { n: h.runsFailed })})` : ""}
                    </Td>
                    <Td className="text-right tabular-nums">{formatNumber(h.totalTokens, lang)}</Td>
                    <Td className="text-right tabular-nums">{formatMoney(h.cost, lang)}</Td>
                    <Td className="text-right tabular-nums">{h.maxParallelism}</Td>
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
