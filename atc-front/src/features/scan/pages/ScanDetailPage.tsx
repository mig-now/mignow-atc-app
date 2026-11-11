import { ArrowLeft } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { useRunWatcher } from "@/features/runs/hooks/useRunWatcher";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { PageHeader } from "@/features/shared/page-header";
import { useSearchState } from "@/lib/use-search-state";
import { ScanActionsCard } from "../components/ScanActionsCard";
import { ScanBatchesCard } from "../components/ScanBatchesCard";
import { ScanCrossCard } from "../components/ScanCrossCard";
import { ScanExportCard } from "../components/ScanExportCard";
import { ScanOccurrencesCard } from "../components/ScanOccurrencesCard";
import { ScanOptionalNotice } from "../components/ScanOptionalNotice";
import { ScanRulesCard } from "../components/ScanRulesCard";
import { ScanScopeCard } from "../components/ScanScopeCard";
import { useActiveScanRun, useScan, useScanActions, useScanRefreshOnRunEnd } from "../hooks/useScans";

const RUN_DEFAULT = { run: "" };

/** Detalhe da varredura: escopo, regras, ações (simular/executar), lotes, cruzamento, ocorrências e importação. */
export function ScanDetailPage() {
  const { t } = useTranslation("scan");
  const { scanId = "" } = useParams();
  const [url, setUrl] = useSearchState(RUN_DEFAULT);
  const scanQuery = useScan(scanId, false);
  const scan = scanQuery.data;
  const activeRun = useActiveScanRun(scan?.sapSystemId);
  const runId = url.run || activeRun?.id || "";
  const watcher = useRunWatcher(runId || undefined);
  const active = watcher.active;
  useScanRefreshOnRunEnd(scanId, watcher.run);
  const cleanup = useScanActions(scanId).cleanup;

  const back = (
    <Button asChild variant="ghost">
      <Link to="/varredura">
        <ArrowLeft aria-hidden /> {t("detail.back")}
      </Link>
    </Button>
  );
  if (scanQuery.isLoading) return <Loading />;
  if (scanQuery.error || !scan) {
    return (
      <>
        <PageHeader title={t("detail.title")} actions={back} />
        <ApiErrorAlert error={scanQuery.error ?? new Error("not-found")} />
      </>
    );
  }
  return (
    <>
      <PageHeader title={t("detail.title")} description={scan.sapSystemName} actions={back} />
      <div className="space-y-4">
        <ScanOptionalNotice />
        <ScanActionsCard scan={scan} runId={runId} active={active} onRun={(id) => setUrl({ run: id })} />
        <ScanBatchesCard
          scan={scan}
          locked={active}
          cleaning={cleanup.isPending}
          onCleanup={() => cleanup.mutate(undefined, { onSuccess: (r) => setUrl({ run: r.id }) })}
        />
        {cleanup.error ? <ApiErrorAlert error={cleanup.error} /> : null}
        <ScanScopeCard scan={scan} />
        <ScanRulesCard scan={scan} locked={active} />
        <ScanCrossCard scan={scan} />
        <ScanOccurrencesCard scan={scan} />
        <ScanExportCard scan={scan} />
      </div>
    </>
  );
}
