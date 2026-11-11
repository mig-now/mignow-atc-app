import { Download, ExternalLink } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { getReportsGetHtmlUrl } from "@/api/generated/atc/reports/reports";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { downloadFile, openHtml } from "@/features/scan/lib/download";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { useSearchState } from "@/lib/use-search-state";
import { ReportAnalyses } from "../components/ReportAnalyses";
import { ReportHighlights } from "../components/ReportHighlights";
import { ReportHistory } from "../components/ReportHistory";
import {
  CheckSolutionSection,
  CyclesSection,
  EvolutionSection,
  ExemptionKbSection,
  ManualVsAiSection,
  ProcessSection,
  SummarySection,
} from "../components/ReportSections";
import { useReport, useReportCustomers, useReportSystems } from "../hooks/useReport";

const TABS = [
  "summary",
  "evolution",
  "cycles",
  "check",
  "manual",
  "exemption",
  "process",
  "analyses",
  "history",
  "highlights",
];
const VIEWS = ["findings", "objetos"] as const;

/** Relatório final do cliente: dados JSON renderizados + HTML (findings/objetos), análises, histórico e destaques. */
export function ReportPage() {
  const { t } = useTranslation("report");
  const [s, set] = useSearchState({ customer: "", system: "", tab: "summary" });
  const customers = useReportCustomers();
  const customerItems = customers.data?.items ?? [];
  const customerId = s.customer || customerItems[0]?.id || "";
  const systems = useReportSystems(customerId);
  const report = useReport(customerId, s.system);
  const [fileError, setFileError] = useState(false);
  const tab = TABS.includes(s.tab) ? s.tab : "summary";
  const params = s.system ? { sapSystemId: s.system } : {};
  const run = (action: Promise<unknown>) => {
    setFileError(false);
    action.catch(() => setFileError(true));
  };

  const actions = (
    <div className="flex flex-wrap gap-2">
      {VIEWS.map((view) => (
        <Button
          key={view}
          variant="outline"
          disabled={!customerId || !report.data}
          onClick={() => run(openHtml(getReportsGetHtmlUrl(customerId, { view, ...params })))}
        >
          <ExternalLink aria-hidden /> {t(`html.open.${view}`)}
        </Button>
      ))}
      <Button
        variant="outline"
        disabled={!customerId || !report.data}
        onClick={() =>
          run(
            downloadFile(
              getReportsGetHtmlUrl(customerId, { view: "findings", ...params }),
              `relatorio_${customerId}.html`,
            ),
          )
        }
      >
        <Download aria-hidden /> {t("html.download")}
      </Button>
    </div>
  );

  return (
    <>
      <PageHeader title={t("title")} description={t("description")} actions={actions} />
      <div className="space-y-4">
        <Card>
          <CardContent className="grid gap-4 pt-4 md:grid-cols-2">
            <Field id="report-customer" label={t("filters.customer")}>
              <NativeSelect
                id="report-customer"
                value={customerId}
                onChange={(e) => set({ customer: e.target.value, system: "" })}
              >
                {customerItems.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </NativeSelect>
            </Field>
            <Field id="report-system" label={t("filters.system")} hint={t("filters.systemHint")}>
              <NativeSelect id="report-system" value={s.system} onChange={(e) => set({ system: e.target.value })}>
                <option value="">{t("filters.allSystems")}</option>
                {(systems.data?.items ?? []).map((x) => (
                  <option key={x.id} value={x.id}>
                    {x.displayName}
                  </option>
                ))}
              </NativeSelect>
            </Field>
          </CardContent>
        </Card>

        {fileError ? <Alert tone="danger">{t("html.failed")}</Alert> : null}
        {customers.isLoading ? <Loading /> : null}
        {customers.error ? <ApiErrorAlert error={customers.error} /> : null}
        {!customers.isLoading && customerItems.length === 0 ? <EmptyState text={t("filters.noCustomers")} /> : null}

        {customerId ? (
          <Tabs value={tab} onValueChange={(v) => set({ tab: v })}>
            <TabsList className="h-auto flex-wrap">
              {TABS.map((k) => (
                <TabsTrigger key={k} value={k}>
                  {t(`tabs.${k}`)}
                </TabsTrigger>
              ))}
            </TabsList>
            <div className="mt-4">
              {["analyses", "history", "highlights"].includes(tab) ? (
                <>
                  <TabsContent value="analyses">
                    <ReportAnalyses customerId={customerId} sapSystemId={s.system} />
                  </TabsContent>
                  <TabsContent value="history">
                    <ReportHistory customerId={customerId} sapSystemId={s.system} />
                  </TabsContent>
                  <TabsContent value="highlights">
                    <ReportHighlights customerId={customerId} />
                  </TabsContent>
                </>
              ) : report.isLoading ? (
                <Loading />
              ) : report.error || !report.data ? (
                <ApiErrorAlert error={report.error ?? new Error("not-found")} />
              ) : (
                <>
                  <TabsContent value="summary">
                    <SummarySection data={report.data} />
                  </TabsContent>
                  <TabsContent value="evolution">
                    <EvolutionSection data={report.data} />
                  </TabsContent>
                  <TabsContent value="cycles">
                    <CyclesSection data={report.data} />
                  </TabsContent>
                  <TabsContent value="check">
                    <CheckSolutionSection data={report.data} />
                  </TabsContent>
                  <TabsContent value="manual">
                    <ManualVsAiSection data={report.data} />
                  </TabsContent>
                  <TabsContent value="exemption">
                    <ExemptionKbSection data={report.data} />
                  </TabsContent>
                  <TabsContent value="process">
                    <ProcessSection data={report.data} />
                  </TabsContent>
                </>
              )}
            </div>
          </Tabs>
        ) : null}
      </div>
    </>
  );
}
