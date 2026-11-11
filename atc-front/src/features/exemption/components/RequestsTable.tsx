import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import { Checkbox, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { useBatchRequests } from "../hooks/useExemption";
import { REQUEST_STATUSES } from "../lib";
import { StatusBadge } from "./StatusBadge";

const PAGE_SIZE = 20;

/** Pedidos do lote com filtro de status (na URL, controlado por quem usa); seleção opcional dos SIMULADO para o envio. */
export function RequestsTable({
  batchId,
  status,
  page,
  onStatus,
  onPage,
  selection,
}: {
  batchId: string;
  status: string;
  page: number;
  onStatus: (s: string) => void;
  onPage: (p: number) => void;
  selection?: { selected: number[]; onChange: (seq: number[]) => void };
}) {
  const { t } = useTranslation("exemption");
  const query = useBatchRequests(batchId, { page, pageSize: PAGE_SIZE, status: status || undefined });
  const items = query.data?.items ?? [];
  const selectable = items.filter((r) => r.status === "SIMULADO").map((r) => r.seq);
  const allSelected = selectable.length > 0 && selectable.every((s) => selection?.selected.includes(s));

  return (
    <div className="space-y-3">
      <NativeSelect
        className="max-w-xs"
        aria-label={t("requests.filterStatus")}
        value={status}
        onChange={(e) => onStatus(e.target.value)}
      >
        <option value="">{t("requests.allStatuses")}</option>
        {REQUEST_STATUSES.map((s) => (
          <option key={s} value={s}>
            {t(`status.${s}`)}
          </option>
        ))}
      </NativeSelect>
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : !items.length ? (
          <EmptyState text={t("requests.empty")} />
        ) : (
          <>
            <Table>
              <THead>
                <Tr>
                  {selection && (
                    <Th className="w-8">
                      <Checkbox
                        aria-label={t("requests.selectPage")}
                        checked={allSelected}
                        disabled={selectable.length === 0}
                        onChange={() =>
                          selection.onChange(
                            allSelected
                              ? selection.selected.filter((s) => !selectable.includes(s))
                              : [...new Set([...selection.selected, ...selectable])],
                          )
                        }
                      />
                    </Th>
                  )}
                  <Th>{t("col.seq")}</Th>
                  <Th>{t("col.item")}</Th>
                  <Th>{t("col.object")}</Th>
                  <Th>{t("col.ref")}</Th>
                  <Th>{t("col.check")}</Th>
                  <Th>{t("col.reason")}</Th>
                  <Th>{t("col.status")}</Th>
                  <Th>{t("col.sapMessage")}</Th>
                  <Th>{t("col.justification")}</Th>
                </Tr>
              </THead>
              <TBody>
                {items.map((r) => (
                  <Tr key={r.id}>
                    {selection && (
                      <Td>
                        <Checkbox
                          aria-label={t("requests.selectSeq", { seq: r.seq })}
                          disabled={r.status !== "SIMULADO"}
                          checked={selection.selected.includes(r.seq)}
                          onChange={() =>
                            selection.onChange(
                              selection.selected.includes(r.seq)
                                ? selection.selected.filter((s) => s !== r.seq)
                                : [...selection.selected, r.seq],
                            )
                          }
                        />
                      </Td>
                    )}
                    <Td>{r.seq}</Td>
                    <Td>{r.itemNumber}</Td>
                    <Td className="font-mono">
                      {r.objectName}
                      <span className="text-muted-foreground"> ({r.objectType})</span>
                    </Td>
                    <Td className="font-mono">{r.refObject ?? "—"}</Td>
                    <Td className="max-w-xs" title={r.message ?? undefined}>
                      {r.check ?? "—"}
                    </Td>
                    <Td className="font-mono">{r.reason}</Td>
                    <Td>
                      <div className="flex flex-wrap gap-1">
                        <StatusBadge status={r.status ?? "PENDENTE"} />
                        {r.probablyNoScope && <Badge tone="warning">{t("requests.probablyNoScope")}</Badge>}
                      </div>
                    </Td>
                    <Td className="max-w-xs">{r.sapMessage ?? "—"}</Td>
                    <Td className="max-w-xs truncate" title={r.justification}>
                      {r.justification}
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
            <Pagination page={page} pageSize={PAGE_SIZE} total={query.data?.total ?? 0} onPage={onPage} />
          </>
        )}
      </Card>
    </div>
  );
}
