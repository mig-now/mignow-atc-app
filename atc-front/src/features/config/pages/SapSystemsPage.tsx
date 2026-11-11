import { KeyRound, Pencil, Plug, Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { SapSystemResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { DeleteButton } from "@/features/shared/delete-button";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { ActiveBadge } from "@/features/shared/status";
import { formatDateTime } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import { ConnectionTestResult } from "../components/ConnectionTestResult";
import { PasswordForm } from "../components/PasswordForm";
import { SapSystemForm } from "../components/SapSystemForm";
import { useCustomers } from "../hooks/useCustomers";
import { useDeleteSapSystem, useSapSystems, useTestSapConnection } from "../hooks/useSapSystems";

const DEFAULTS = { page: "1", customerId: "" };

type Dialogs =
  | { kind: "edit"; system?: SapSystemResponse }
  | { kind: "password"; system: SapSystemResponse }
  | { kind: "test"; system: SapSystemResponse };

/** `/config/ambientes`: lista por cliente (filtro e página na URL); criar/editar, trocar senha, testar conexão. */
export function SapSystemsPage() {
  const { t, i18n } = useTranslation("config");
  const [state, set] = useSearchState(DEFAULTS);
  const customers = useCustomers({ page: 1, pageSize: 200 });
  const query = useSapSystems({
    page: Number(state.page),
    pageSize: 20,
    customerId: state.customerId || undefined,
  });
  const remove = useDeleteSapSystem();
  const test = useTestSapConnection();
  const [dialog, setDialog] = useState<Dialogs | null>(null);
  const close = () => setDialog(null);

  const runTest = (system: SapSystemResponse) => {
    setDialog({ kind: "test", system });
    test.reset();
    test.mutate(system.id);
  };

  return (
    <>
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <NativeSelect
          className="max-w-xs"
          aria-label={t("sapSystems.filterCustomer")}
          value={state.customerId}
          onChange={(e) => set({ customerId: e.target.value })}
        >
          <option value="">{t("sapSystems.allCustomers")}</option>
          {customers.data?.items.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </NativeSelect>
        <Button onClick={() => setDialog({ kind: "edit" })}>
          <Plus />
          {t("sapSystems.new")}
        </Button>
      </div>
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : !query.data?.items.length ? (
          <EmptyState text={t("sapSystems.empty")} />
        ) : (
          <>
            <Table>
              <THead>
                <Tr>
                  <Th>{t("sapSystems.environment")}</Th>
                  <Th>{t("sapSystems.customer")}</Th>
                  <Th>{t("sapSystems.host")}</Th>
                  <Th>{t("sapSystems.lastTest")}</Th>
                  <Th>{t("common:common.status")}</Th>
                  <Th className="w-48">
                    <span className="sr-only">{t("common:common.actions")}</span>
                  </Th>
                </Tr>
              </THead>
              <TBody>
                {query.data.items.map((s) => (
                  <Tr key={s.id}>
                    <Td>
                      <span className="font-medium">{s.displayName}</span>
                      <div className="mt-1 flex flex-wrap gap-1">
                        {s.isProduction && <Badge tone="danger">{t("sapSystems.productionBadge")}</Badge>}
                        {s.isSimulator && <Badge tone="info">{t("sapSystems.simulatorBadge")}</Badge>}
                        <Badge>{t(`sapSystems.projectType.${s.projectType}`)}</Badge>
                      </div>
                    </Td>
                    <Td>{s.customerName}</Td>
                    <Td className="font-mono">{s.isSimulator ? s.host : `${s.host}:${s.adtPort}`}</Td>
                    <Td>
                      {s.lastConnectionTestAt ? (
                        <>
                          <Badge tone={s.lastConnectionTestOk ? "success" : "danger"}>
                            {s.lastConnectionTestOk ? t("test.ok") : t("test.failed")}
                          </Badge>
                          <div className="mt-1 text-xs text-muted-foreground">
                            {formatDateTime(s.lastConnectionTestAt, i18n.language)}
                          </div>
                        </>
                      ) : (
                        <span className="text-muted-foreground">{t("sapSystems.neverTested")}</span>
                      )}
                    </Td>
                    <Td>
                      <ActiveBadge active={s.active} />
                    </Td>
                    <Td>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label={t("sapSystems.testItem", { name: s.displayName })}
                        onClick={() => runTest(s)}
                      >
                        <Plug />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label={t("sapSystems.passwordItem", { name: s.displayName })}
                        onClick={() => setDialog({ kind: "password", system: s })}
                      >
                        <KeyRound />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label={t("common:actions.editItem", { name: s.displayName })}
                        onClick={() => setDialog({ kind: "edit", system: s })}
                      >
                        <Pencil />
                      </Button>
                      <DeleteButton name={s.displayName} onDelete={() => remove.mutateAsync(s.id)} />
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
      <Dialog open={dialog !== null} onOpenChange={(o) => !o && close()}>
        {dialog?.kind === "edit" && (
          <DialogContent
            wide
            title={dialog.system ? t("sapSystems.edit") : t("sapSystems.new")}
            description={dialog.system ? undefined : t("sapSystems.newHint")}
          >
            <SapSystemForm system={dialog.system} defaultCustomerId={state.customerId} onDone={close} />
          </DialogContent>
        )}
        {dialog?.kind === "password" && (
          <DialogContent
            title={t("sapSystems.changePassword")}
            description={t("sapSystems.passwordNeverShown", { name: dialog.system.displayName })}
          >
            <PasswordForm systemId={dialog.system.id} onDone={close} />
          </DialogContent>
        )}
        {dialog?.kind === "test" && (
          <DialogContent title={t("sapSystems.testTitle", { name: dialog.system.displayName })}>
            {test.isPending && <Loading label={t("test.running")} />}
            <ApiErrorAlert error={test.error} />
            {test.data && <ConnectionTestResult result={test.data} isProduction={dialog.system.isProduction} />}
          </DialogContent>
        )}
      </Dialog>
    </>
  );
}
