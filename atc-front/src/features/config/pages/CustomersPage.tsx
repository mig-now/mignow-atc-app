import { Pencil, Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { CustomerResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { DeleteButton } from "@/features/shared/delete-button";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { ActiveBadge } from "@/features/shared/status";
import { useSearchState } from "@/lib/use-search-state";
import { CustomerForm } from "../components/CustomerForm";
import { useCustomers, useDeleteCustomer } from "../hooks/useCustomers";

const DEFAULTS = { page: "1", search: "" };

/** `/config/clientes`: lista (busca e página na URL) + criar/editar/excluir. */
export function CustomersPage() {
  const { t } = useTranslation("config");
  const [state, set] = useSearchState(DEFAULTS);
  const query = useCustomers({ page: Number(state.page), pageSize: 20, search: state.search || undefined });
  const remove = useDeleteCustomer();
  const [editing, setEditing] = useState<CustomerResponse | "new" | null>(null);
  return (
    <>
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <Input
          type="search"
          className="max-w-xs"
          aria-label={t("common:actions.search")}
          placeholder={t("customers.searchPlaceholder")}
          defaultValue={state.search}
          onChange={(e) => set({ search: e.target.value })}
        />
        <Button onClick={() => setEditing("new")}>
          <Plus />
          {t("customers.new")}
        </Button>
      </div>
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : !query.data?.items.length ? (
          <EmptyState text={t("customers.empty")} />
        ) : (
          <>
            <Table>
              <THead>
                <Tr>
                  <Th>{t("customers.name")}</Th>
                  <Th>{t("customers.code")}</Th>
                  <Th>{t("customers.sapSystems")}</Th>
                  <Th>{t("common:common.status")}</Th>
                  <Th className="w-24">
                    <span className="sr-only">{t("common:common.actions")}</span>
                  </Th>
                </Tr>
              </THead>
              <TBody>
                {query.data.items.map((c) => (
                  <Tr key={c.id}>
                    <Td className="font-medium">{c.name}</Td>
                    <Td>{c.code ?? "—"}</Td>
                    <Td>{c.sapSystemCount}</Td>
                    <Td>
                      <ActiveBadge active={c.active} />
                    </Td>
                    <Td>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label={t("common:actions.editItem", { name: c.name })}
                        onClick={() => setEditing(c)}
                      >
                        <Pencil />
                      </Button>
                      <DeleteButton
                        name={c.name}
                        description={t("customers.deleteHint")}
                        onDelete={() => remove.mutateAsync(c.id)}
                      />
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
            <Pagination
              page={query.data.page}
              pageSize={query.data.pageSize}
              total={query.data.total}
              onPage={(p) => set({ page: String(p) })}
            />
          </>
        )}
      </Card>
      <Dialog open={editing !== null} onOpenChange={(o) => !o && setEditing(null)}>
        {editing !== null && (
          <DialogContent title={editing === "new" ? t("customers.new") : t("customers.edit")}>
            <CustomerForm customer={editing === "new" ? undefined : editing} onDone={() => setEditing(null)} />
          </DialogContent>
        )}
      </Dialog>
    </>
  );
}
