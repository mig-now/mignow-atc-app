import { ArrowLeft } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { PageHeader } from "@/features/shared/page-header";
import { cn } from "@/lib/cn";
import { formatDateTime } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import { BatchHistory } from "../components/BatchHistory";
import { BatchTotals } from "../components/BatchTotals";
import { CheckStep } from "../components/CheckStep";
import { DiscoveryStep } from "../components/DiscoveryStep";
import { ParamsStep } from "../components/ParamsStep";
import { SendStep } from "../components/SendStep";
import { SimulateStep } from "../components/SimulateStep";
import { useBatch, useBatchRuns, useSystem } from "../hooks/useExemption";
import { maxStep, recommendedStep } from "../lib";

const DEFAULTS = { step: "", status: "", page: "1" };
const STEPS = ["discovery", "params", "simulate", "send", "check"] as const;

/** `/isencao/:batchId`: lote como assistente em 5 etapas (descoberta, parâmetros, simular, enviar, conferir). */
export function ExemptionBatchPage() {
  const { t, i18n } = useTranslation("exemption");
  const { batchId = "" } = useParams();
  const [state, set] = useSearchState(DEFAULTS);
  const batch = useBatch(batchId);
  const system = useSystem(batch.data?.sapSystemId ?? "");
  const { runs } = useBatchRuns(batch.data);

  if (batch.isLoading) return <Loading />;
  if (batch.error || !batch.data) return <ApiErrorAlert error={batch.error ?? new Error("not-found")} />;
  const b = batch.data;
  const allowed = maxStep(b);
  const current = Math.min(Number(state.step) || recommendedStep(b), allowed);
  const table = {
    status: state.status,
    page: Number(state.page),
    onStatus: (s: string) => set({ status: s }),
    onPage: (p: number) => set({ page: String(p) }),
  };

  return (
    <>
      <PageHeader
        title={t("batch.title")}
        description={t("batch.subtitle", {
          environment: system.data?.displayName ?? b.sapSystemId,
          date: formatDateTime(b.createdAt, i18n.language),
          priorities: b.priorities
            .split(",")
            .map((p) => `P${p}`)
            .join(", "),
        })}
        actions={
          <>
            <Button variant="outline" asChild>
              <Link to={`/remediacao/${b.sessionId}`}>{t("batch.session")}</Link>
            </Button>
            <Button variant="ghost" asChild>
              <Link to="/isencao">
                <ArrowLeft />
                {t("common:actions.back")}
              </Link>
            </Button>
          </>
        }
      />
      <div className="space-y-4">
        <BatchTotals batch={b} />
        <nav aria-label={t("batch.steps")}>
          <ol className="flex flex-wrap gap-2">
            {STEPS.map((s, i) => {
              const n = i + 1;
              return (
                <li key={s}>
                  <button
                    type="button"
                    disabled={n > allowed}
                    aria-current={n === current ? "step" : undefined}
                    onClick={() => set({ step: String(n) })}
                    className={cn(
                      "flex items-center gap-2 rounded-md border px-3 py-1.5 text-sm transition-colors disabled:opacity-50",
                      n === current ? "border-primary bg-primary/10 font-medium" : "border-border hover:bg-accent",
                    )}
                  >
                    <span className="flex size-5 items-center justify-center rounded-full bg-muted text-xs">{n}</span>
                    {t(`steps.${s}`)}
                  </button>
                </li>
              );
            })}
          </ol>
        </nav>
        <Card>
          <CardHeader>
            <CardTitle>{t(`steps.${STEPS[current - 1]}`)}</CardTitle>
          </CardHeader>
          <CardContent>
            {current === 1 && <DiscoveryStep batch={b} runs={runs} />}
            {current === 2 && <ParamsStep batch={b} system={system.data} onSaved={() => set({ step: "3" })} />}
            {current === 3 && <SimulateStep batch={b} runs={runs} table={table} />}
            {current === 4 && <SendStep batch={b} system={system.data} runs={runs} table={table} />}
            {current === 5 && <CheckStep batch={b} runs={runs} />}
          </CardContent>
        </Card>
        <BatchHistory runs={runs} />
      </div>
    </>
  );
}
