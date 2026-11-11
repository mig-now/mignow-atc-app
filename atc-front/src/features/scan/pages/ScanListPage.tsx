import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Checkbox, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { Pagination } from "@/features/shared/pagination";
import { formatDateTime, formatNumber } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import { ScanOptionalNotice } from "../components/ScanOptionalNotice";
import { scopeOf, summaryOf, useCreateScan, useScanExtractions, useScanList, useScanSystems } from "../hooks/useScans";

/** Lista de varreduras MIGNOW por ambiente + nova varredura (POST scans já enfileira o inventário). */
export function ScanListPage() {
  const { t, i18n } = useTranslation("scan");
  const lang = i18n.language;
  const navigate = useNavigate();
  const [s, set] = useSearchState({ system: "", page: "1" });
  const systems = useScanSystems();
  const systemItems = systems.data?.items ?? [];
  const systemId = s.system || systemItems[0]?.id || "";
  const page = Math.max(1, Number(s.page) || 1);
  const list = useScanList(systemId, page);
  const extractions = useScanExtractions(systemId);
  const create = useCreateScan();

  const sorted = useMemo(
    () => [...(extractions.data?.items ?? [])].sort((a, b) => b.extractedAt.localeCompare(a.extractedAt)),
    [extractions.data],
  );
  const [extractionId, setExtractionId] = useState("");
  const [includeGenerated, setIncludeGenerated] = useState(false);
  useEffect(() => {
    setExtractionId(sorted[0]?.id ?? "");
  }, [sorted]);

  return (
    <>
      <PageHeader title={t("title")} description={t("description")} />
      <div className="space-y-4">
        <ScanOptionalNotice />
        <Card>
          <CardContent className="pt-4">
            <Field id="scan-system" label={t("list.system")}>
              <NativeSelect
                id="scan-system"
                className="max-w-md"
                value={systemId}
                onChange={(e) => set({ system: e.target.value })}
              >
                {systemItems.map((x) => (
                  <option key={x.id} value={x.id}>
                    {x.customerName} · {x.displayName}
                  </option>
                ))}
              </NativeSelect>
            </Field>
          </CardContent>
        </Card>

        {systems.isLoading ? <Loading /> : null}
        {systems.error ? <ApiErrorAlert error={systems.error} /> : null}
        {!systems.isLoading && systemItems.length === 0 ? <EmptyState text={t("list.noSystems")} /> : null}

        {systemId ? (
          <>
            <Card>
              <CardHeader>
                <CardTitle>{t("new.title")}</CardTitle>
              </CardHeader>
              <CardContent>
                <form
                  className="grid items-end gap-4 md:grid-cols-[1fr_auto_auto]"
                  onSubmit={(e) => {
                    e.preventDefault();
                    create.mutate(
                      { sapSystemId: systemId, extractionId: extractionId || null, includeGenerated },
                      { onSuccess: (r) => navigate(`/varredura/${r.scan.id}?run=${r.run.id}`) },
                    );
                  }}
                >
                  <Field id="scan-extraction" label={t("new.extraction")} hint={t("new.extractionHint")}>
                    <NativeSelect
                      id="scan-extraction"
                      value={extractionId}
                      onChange={(e) => setExtractionId(e.target.value)}
                    >
                      {sorted.length === 0 ? <option value="">{t("new.noExtraction")}</option> : null}
                      {sorted.map((x, i) => (
                        <option key={x.id} value={x.id}>
                          {x.displayId} · {formatDateTime(x.executedAtUtc ?? x.extractedAt, lang)}
                          {i === 0 ? ` · ${t("new.latest")}` : ""}
                        </option>
                      ))}
                    </NativeSelect>
                  </Field>
                  <label className="flex items-center gap-2 pb-2 text-sm">
                    <Checkbox checked={includeGenerated} onChange={(e) => setIncludeGenerated(e.target.checked)} />
                    {t("new.includeGenerated")}
                  </label>
                  <Button type="submit" disabled={create.isPending}>
                    {t("new.create")}
                  </Button>
                </form>
                {create.error ? (
                  <div className="mt-3">
                    <ApiErrorAlert error={create.error} />
                  </div>
                ) : null}
                {sorted.length === 0 && !extractions.isLoading ? (
                  <div className="mt-3">
                    <Alert tone="warning">{t("new.noExtractionHint")}</Alert>
                  </div>
                ) : null}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>{t("list.scans")}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {list.isLoading ? (
                  <Loading />
                ) : list.error ? (
                  <ApiErrorAlert error={list.error} />
                ) : !list.data || list.data.items.length === 0 ? (
                  <EmptyState text={t("list.empty")} />
                ) : (
                  <div className="overflow-x-auto rounded-md border border-border">
                    <Table>
                      <THead>
                        <Tr>
                          <Th>{t("list.createdAt")}</Th>
                          <Th className="text-right">{t("list.objects")}</Th>
                          <Th className="text-right">{t("list.batches")}</Th>
                          <Th className="text-right">{t("list.rules")}</Th>
                          <Th className="text-right">{t("list.mignow")}</Th>
                          <Th>{t("list.by")}</Th>
                        </Tr>
                      </THead>
                      <TBody>
                        {list.data.items.map((sc) => {
                          const sum = summaryOf(sc);
                          return (
                            <Tr key={sc.id}>
                              <Td>
                                <Link className="underline-offset-2 hover:underline" to={`/varredura/${sc.id}`}>
                                  {formatDateTime(sc.createdAt, lang)}
                                </Link>
                              </Td>
                              <Td className="text-right">{formatNumber(scopeOf(sc).escopo ?? 0, lang)}</Td>
                              <Td className="text-right">{sc.batches.length}</Td>
                              <Td className="text-right">
                                {sc.activeRuleIds.length}/{sc.ruleCount}
                              </Td>
                              <Td className="text-right">
                                {sum?.mignow?.ocorrencias != null ? formatNumber(sum.mignow.ocorrencias, lang) : "—"}
                              </Td>
                              <Td>{sc.createdBy}</Td>
                            </Tr>
                          );
                        })}
                      </TBody>
                    </Table>
                    <Pagination
                      page={page}
                      pageSize={list.data.pageSize}
                      total={list.data.total}
                      onPage={(p) => set({ page: String(p) })}
                    />
                  </div>
                )}
              </CardContent>
            </Card>
          </>
        ) : null}
      </div>
    </>
  );
}
