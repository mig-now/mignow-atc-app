import { Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { Pagination } from "@/features/shared/pagination";
import { formatDateTime } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import { NewBatchDialog } from "../components/NewBatchDialog";
import { StatusBadge } from "../components/StatusBadge";
import { useBatches, useSessionOptions, useSystemOptions } from "../hooks/useExemption";

const DEFAULTS = { page: "1", sapSystemId: "", sessionId: "" };

/** `/isencao`: lotes de isenção (filtros de ambiente e sessão e página na URL) e "Novo lote" a partir de uma sessão. */
export function ExemptionListPage() {
  const { t, i18n } = useTranslation("exemption");
  const [state, set] = useSearchState(DEFAULTS);
  const [creating, setCreating] = useState(false);
  const systems = useSystemOptions();
  const sessions = useSessionOptions({ sapSystemId: state.sapSystemId, archived: undefined });
  const query = useBatches({
    page: Number(state.page),
    pageSize: 20,
    sapSystemId: state.sapSystemId || undefined,
    sessionId: state.sessionId || undefined,
  });
  const systemName = (id: string) => systems.data?.items.find((s) => s.id === id)?.displayName ?? id;

  return (
    <>
      <PageHeader
        title={t("title")}
        description={t("description")}
        actions={
          <Button onClick={() => setCreating(true)}>
            <Plus />
            {t("list.new")}
          </Button>
        }
      />
      <div className="mb-3 grid gap-3 sm:grid-cols-2">
        <NativeSelect
          aria-label={t("list.filterEnvironment")}
          value={state.sapSystemId}
          onChange={(e) => set({ sapSystemId: e.target.value, sessionId: "" })}
        >
          <option value="">{t("list.allEnvironments")}</option>
          {systems.data?.items.map((s) => (
            <option key={s.id} value={s.id}>
              {s.customerName} · {s.displayName}
            </option>
          ))}
        </NativeSelect>
        <NativeSelect
          aria-label={t("list.filterSession")}
          value={state.sessionId}
          onChange={(e) => set({ sessionId: e.target.value })}
        >
          <option value="">{t("list.allSessions")}</option>
          {sessions.data?.items.map((s) => (
            <option key={s.id} value={s.id}>
              {`${s.sapSystemName} · ${t("newBatch.cycle", { n: s.cycle })} · ${formatDateTime(s.createdAt, i18n.language)}`}
            </option>
          ))}
        </NativeSelect>
      </div>
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : !query.data?.items.length ? (
          <EmptyState text={t("list.empty")} />
        ) : (
          <>
            <Table>
              <THead>
                <Tr>
                  <Th>{t("list.col.created")}</Th>
                  <Th>{t("list.col.environment")}</Th>
                  <Th>{t("list.col.session")}</Th>
                  <Th>{t("list.col.priorities")}</Th>
                  <Th>{t("list.col.approver")}</Th>
                  <Th>{t("list.col.status")}</Th>
                  <Th>{t("list.col.requests")}</Th>
                  <Th>{t("list.col.totals")}</Th>
                </Tr>
              </THead>
              <TBody>
                {query.data.items.map((b) => (
                  <Tr key={b.id}>
                    <Td>
                      <Link
                        className="font-medium text-primary underline-offset-4 hover:underline"
                        to={`/isencao/${b.id}`}
                      >
                        {formatDateTime(b.createdAt, i18n.language)}
                      </Link>
                    </Td>
                    <Td>{systemName(b.sapSystemId)}</Td>
                    <Td>
                      <Link
                        className="font-mono text-sm underline-offset-4 hover:underline"
                        to={`/remediacao/${b.sessionId}`}
                      >
                        {b.sessionId.slice(0, 8)}
                      </Link>
                    </Td>
                    <Td>
                      {b.priorities
                        .split(",")
                        .map((p) => `P${p}`)
                        .join(", ")}
                    </Td>
                    <Td className="font-mono">{b.approver ?? "—"}</Td>
                    <Td>{t(`batchStatus.${b.status}`)}</Td>
                    <Td>{b.requests}</Td>
                    <Td>
                      <div className="flex flex-wrap gap-1">
                        {Object.entries(b.totals)
                          .filter(([, n]) => n > 0)
                          .map(([status, n]) => (
                            <span key={status} className="inline-flex items-center gap-1 text-sm">
                              <StatusBadge status={status} />
                              {n}
                            </span>
                          ))}
                      </div>
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
            <Pagination
              page={Number(state.page)}
              pageSize={20}
              total={query.data.total}
              onPage={(p) => set({ page: String(p) })}
            />
          </>
        )}
      </Card>
      <NewBatchDialog open={creating} onClose={() => setCreating(false)} />
    </>
  );
}
