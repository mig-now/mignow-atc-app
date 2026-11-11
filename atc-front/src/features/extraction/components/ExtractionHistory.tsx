import { Download, ListChecks, ListFilter } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ExtractionResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { formatDateTime, formatNumber } from "@/lib/format";
import { type ExtractionFileKind, useDownloadExtractionFile, useExtractionHistory } from "../hooks/useExtraction";
import { FindingsDialog } from "./FindingsDialog";
import { RemediationDialog } from "./RemediationDialog";

const PAGE_SIZE = 10;

/** A variação vem como `unknown` no contrato: mostra só quando for número (ou `{ total }`). */
export function variationValue(v: unknown): number | null {
  if (typeof v === "number") return v;
  if (v && typeof v === "object" && "total" in v && typeof (v as { total: unknown }).total === "number")
    return (v as { total: number }).total;
  return null;
}

/** Histórico de extrações do ambiente: totais, baseline, P1–P3 (total / sem baseline), variação, CSVs e ações. */
export function ExtractionHistory({
  sapSystemId,
  defaultRequest,
  page,
  onPage,
}: {
  sapSystemId: string;
  defaultRequest?: string | null;
  page: number;
  onPage: (p: number) => void;
}) {
  const { t, i18n } = useTranslation("extraction");
  const lang = i18n.language;
  const query = useExtractionHistory({ sapSystemId, page, pageSize: PAGE_SIZE });
  const download = useDownloadExtractionFile();
  const [findingsOf, setFindingsOf] = useState<ExtractionResponse | null>(null);
  const [remediationOf, setRemediationOf] = useState<ExtractionResponse | null>(null);

  const get = (e: ExtractionResponse, kind: ExtractionFileKind) =>
    download.mutate({ id: e.id, kind, fallbackName: e.fileBaseName });

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("history.title")}</CardTitle>
      </CardHeader>
      <div className="px-4">
        <ApiErrorAlert error={query.error ?? download.error} />
      </div>
      {query.isLoading ? (
        <Loading />
      ) : !query.data?.items.length ? (
        <EmptyState text={t("history.empty")} />
      ) : (
        <>
          <Table>
            <THead>
              <Tr>
                <Th>{t("history.col.date")}</Th>
                <Th>{t("history.col.series")}</Th>
                <Th>{t("history.col.displayId")}</Th>
                <Th>{t("history.col.total")}</Th>
                <Th>{t("history.col.baseline")}</Th>
                {(["p1", "p2", "p3"] as const).map((p) => (
                  <Th key={p} title={t("history.col.priorityHint")}>
                    {t("history.col.priority", { n: p.slice(1) })}
                  </Th>
                ))}
                <Th>{t("history.col.variation")}</Th>
                <Th className="w-72">
                  <span className="sr-only">{t("common:common.actions")}</span>
                </Th>
              </Tr>
            </THead>
            <TBody>
              {query.data.items.map((e) => {
                const variation = variationValue(e.variation);
                return (
                  <Tr key={e.id}>
                    <Td>{formatDateTime(e.executedAtUtc ?? e.extractedAt, lang)}</Td>
                    <Td className="font-mono">{e.series}</Td>
                    <Td className="font-mono">
                      {e.displayId}
                      {e.situation === "AlreadyExtracted" && (
                        <Badge className="ml-2" tone="neutral">
                          {t("history.reextracted")}
                        </Badge>
                      )}
                    </Td>
                    <Td>{formatNumber(e.totalCount, lang)}</Td>
                    <Td>{formatNumber(e.inBaselineCount, lang)}</Td>
                    {(["p1", "p2", "p3"] as const).map((p) => (
                      <Td key={p} className="whitespace-nowrap">
                        {formatNumber(e.total[p], lang)}
                        <span className="text-muted-foreground"> / {formatNumber(e.new[p], lang)}</span>
                      </Td>
                    ))}
                    <Td>{variation === null ? "—" : `${variation > 0 ? "+" : ""}${formatNumber(variation, lang)}`}</Td>
                    <Td>
                      <div className="flex flex-wrap justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          aria-label={t("history.findingsOf", { id: e.displayId })}
                          onClick={() => setFindingsOf(e)}
                        >
                          <ListFilter />
                          {t("history.findings")}
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          aria-label={t("history.csvTotalOf", { id: e.displayId })}
                          onClick={() => get(e, "total")}
                        >
                          <Download />
                          {t("history.csvTotal")}
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          aria-label={t("history.csvNewOf", { id: e.displayId })}
                          onClick={() => get(e, "novos")}
                        >
                          <Download />
                          {t("history.csvNew")}
                        </Button>
                        <Button
                          size="sm"
                          aria-label={t("history.remediationOf", { id: e.displayId })}
                          onClick={() => setRemediationOf(e)}
                        >
                          <ListChecks />
                          {t("history.remediation")}
                        </Button>
                      </div>
                    </Td>
                  </Tr>
                );
              })}
            </TBody>
          </Table>
          <Pagination page={page} pageSize={PAGE_SIZE} total={query.data.total} onPage={onPage} />
        </>
      )}
      <FindingsDialog extraction={findingsOf} onClose={() => setFindingsOf(null)} />
      <RemediationDialog
        extraction={remediationOf}
        defaultRequest={defaultRequest}
        onClose={() => setRemediationOf(null)}
      />
    </Card>
  );
}
