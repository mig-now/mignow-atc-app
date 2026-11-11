import { Ban } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { RunKind, type RunResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { isRunActive, RunStatusBadge } from "@/features/shared/status";
import { formatDateTime } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import { useActiveRuns, useCancelRun, useRunsHistory } from "../hooks/useConfigRuns";
import { useSapSystems } from "../hooks/useSapSystems";

const DEFAULTS = { page: "1", sapSystemId: "", kind: "", status: "" };
const STATUSES = ["Queued", "Running", "Cancelling", "Cancelled", "Completed", "Failed"] as const;

function RunRow({ run, onCancel }: { run: RunResponse; onCancel?: (id: string) => void }) {
  const { t, i18n } = useTranslation("config");
  return (
    <Tr>
      <Td>
        <Link className="font-medium text-primary hover:underline" to={`/execucoes/${run.id}`}>
          {t(`runs:kind.${run.kind}`)}
        </Link>
      </Td>
      <Td>
        {run.sapSystemName}
        <div className="text-xs text-muted-foreground">{run.customerName}</div>
      </Td>
      <Td>
        <RunStatusBadge status={run.status} />
      </Td>
      <Td>{run.startedBy}</Td>
      <Td>{formatDateTime(run.startedAt ?? run.queuedAt, i18n.language)}</Td>
      <Td className="w-12">
        {onCancel && isRunActive(run.status) && run.status !== "Cancelling" && (
          <Button
            variant="ghost"
            size="icon"
            aria-label={t("runs.cancelItem", { name: t(`runs:kind.${run.kind}`) })}
            onClick={() => onCancel(run.id)}
          >
            <Ban />
          </Button>
        )}
      </Td>
    </Tr>
  );
}

function Head() {
  const { t } = useTranslation("config");
  return (
    <THead>
      <Tr>
        <Th>{t("runs.kind")}</Th>
        <Th>{t("runs.environment")}</Th>
        <Th>{t("common:common.status")}</Th>
        <Th>{t("runs.startedBy")}</Th>
        <Th>{t("runs.when")}</Th>
        <Th>
          <span className="sr-only">{t("common:common.actions")}</span>
        </Th>
      </Tr>
    </THead>
  );
}

/** `/config/execucoes`: ativos de todos os ambientes (com cancelar) e histórico paginado com filtros na URL. */
export function RunsPage() {
  const { t } = useTranslation("config");
  const [state, set] = useSearchState(DEFAULTS);
  const active = useActiveRuns();
  const cancel = useCancelRun();
  const systems = useSapSystems({ page: 1, pageSize: 200 });
  const history = useRunsHistory({
    page: Number(state.page),
    pageSize: 20,
    sapSystemId: state.sapSystemId || undefined,
    kind: (state.kind || undefined) as RunKind | undefined,
  });
  // O backend filtra por ambiente e tipo; a situação é filtrada aqui sobre a página recebida.
  const items = (history.data?.items ?? []).filter((r) => !state.status || r.status === state.status);
  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>{t("runs.activeTitle")}</CardTitle>
        </CardHeader>
        <ApiErrorAlert error={active.error ?? cancel.error} />
        {active.isLoading ? (
          <Loading />
        ) : !active.data?.items.length ? (
          <EmptyState text={t("runs.noActive")} />
        ) : (
          <Table>
            <Head />
            <TBody>
              {active.data.items.map((r) => (
                <RunRow key={r.id} run={r} onCancel={(id) => cancel.mutate(id)} />
              ))}
            </TBody>
          </Table>
        )}
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t("runs.historyTitle")}</CardTitle>
          <div className="flex flex-wrap gap-2">
            <NativeSelect
              className="w-44"
              aria-label={t("runs.filterEnvironment")}
              value={state.sapSystemId}
              onChange={(e) => set({ sapSystemId: e.target.value })}
            >
              <option value="">{t("runs.allEnvironments")}</option>
              {systems.data?.items.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.displayName}
                </option>
              ))}
            </NativeSelect>
            <NativeSelect
              className="w-44"
              aria-label={t("runs.filterKind")}
              value={state.kind}
              onChange={(e) => set({ kind: e.target.value })}
            >
              <option value="">{t("runs.allKinds")}</option>
              {Object.values(RunKind).map((k) => (
                <option key={k} value={k}>
                  {t(`runs:kind.${k}`)}
                </option>
              ))}
            </NativeSelect>
            <NativeSelect
              className="w-44"
              aria-label={t("runs.filterStatus")}
              value={state.status}
              onChange={(e) => set({ status: e.target.value })}
            >
              <option value="">{t("runs.allStatuses")}</option>
              {STATUSES.map((s) => (
                <option key={s} value={s}>
                  {t(`common:runStatus.${s}`)}
                </option>
              ))}
            </NativeSelect>
          </div>
        </CardHeader>
        <ApiErrorAlert error={history.error} />
        {history.isLoading ? (
          <Loading />
        ) : !items.length ? (
          <EmptyState text={t("runs.noHistory")} />
        ) : (
          <>
            <Table>
              <Head />
              <TBody>
                {items.map((r) => (
                  <RunRow key={r.id} run={r} />
                ))}
              </TBody>
            </Table>
            {history.data && (
              <Pagination
                page={history.data.page}
                pageSize={history.data.pageSize}
                total={history.data.total}
                onPage={(p) => set({ page: String(p) })}
              />
            )}
          </>
        )}
      </Card>
    </div>
  );
}
