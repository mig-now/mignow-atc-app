import { ArrowLeft } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { useSearchState } from "@/lib/use-search-state";
import { EnvironmentPicker } from "../components/EnvironmentPicker";
import { ExtractionHistory } from "../components/ExtractionHistory";
import { ExtractionRunner } from "../components/ExtractionRunner";
import { LatestCard } from "../components/LatestCard";
import { useAtcLatest, useEnvironmentOptions } from "../hooks/useExtraction";

const DEFAULTS = { customerId: "", systemId: "", page: "1" };

/** `/extracao/nova` (a partir do monitor): cliente → ambiente; última execução do ATC, extrair (run inline) e histórico com CSVs e sessão de remediação. */
export function ExtractionPage() {
  const { t } = useTranslation("extraction");
  const [state, set] = useSearchState(DEFAULTS);
  const latest = useAtcLatest(state.systemId);
  const systems = useEnvironmentOptions(state.customerId);
  const system = systems.data?.items.find((s) => s.id === state.systemId);

  return (
    <>
      <PageHeader
        title={t("new.title")}
        description={t("description")}
        actions={
          <Button variant="outline" asChild>
            <Link to="/extracao">
              <ArrowLeft />
              {t("new.backToMonitor")}
            </Link>
          </Button>
        }
      />
      <div className="space-y-4">
        <EnvironmentPicker customerId={state.customerId} systemId={state.systemId} onChange={set} />
        {!state.systemId ? (
          <EmptyState text={t("pickEnvironment")} />
        ) : (
          <>
            <ApiErrorAlert error={latest.error} />
            {latest.isLoading ? (
              <Loading />
            ) : latest.data ? (
              <>
                <LatestCard latest={latest.data} />
                <ExtractionRunner key={state.systemId} sapSystemId={state.systemId} latest={latest.data} />
              </>
            ) : null}
            <ExtractionHistory
              sapSystemId={state.systemId}
              defaultRequest={system?.defaultRequest}
              page={Number(state.page)}
              onPage={(p) => set({ page: String(p) })}
            />
          </>
        )}
      </div>
    </>
  );
}
