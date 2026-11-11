import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { PageHeader } from "@/features/shared/page-header";
import { RunMonitorPanel } from "../components/RunMonitorPanel";

/** `/execucoes/:runId` — monitor do Run (o "tentar de novo" leva ao run novo). */
export function RunMonitorPage() {
  const { t } = useTranslation("runs");
  const { runId = "" } = useParams();
  const navigate = useNavigate();
  return (
    <>
      <PageHeader title={t("monitor.title")} />
      <RunMonitorPanel runId={runId} onRetried={(r) => navigate(`/execucoes/${r.id}`)} />
    </>
  );
}
