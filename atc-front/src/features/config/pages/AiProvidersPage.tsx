import { Pencil, Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { AiProviderResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { DeleteButton } from "@/features/shared/delete-button";
import { EmptyState } from "@/features/shared/empty-state";
import { ActiveBadge } from "@/features/shared/status";
import { AiProviderForm } from "../components/AiProviderForm";
import { useAiProviders, useDeleteAiProvider } from "../hooks/useAiProviders";

/** `/config/ia`: provedores de IA. Um só padrão, usado em todas as chamadas (padrão Claude Opus). A chave só entra na criação/troca. */
export function AiProvidersPage() {
  const { t } = useTranslation("config");
  const query = useAiProviders();
  const remove = useDeleteAiProvider();
  const [editing, setEditing] = useState<AiProviderResponse | "new" | null>(null);
  return (
    <>
      <div className="mb-3 flex items-center justify-between gap-4">
        <p className="text-muted-foreground text-sm">{t("ai.singleModel")}</p>
        <Button onClick={() => setEditing("new")}>
          <Plus />
          {t("ai.new")}
        </Button>
      </div>
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : !query.data?.length ? (
          <EmptyState text={t("ai.empty")} />
        ) : (
          <Table>
            <THead>
              <Tr>
                <Th>{t("ai.name")}</Th>
                <Th>{t("ai.kind")}</Th>
                <Th>{t("ai.model")}</Th>
                <Th>{t("ai.apiKey")}</Th>
                <Th>{t("common:common.status")}</Th>
                <Th className="w-24">
                  <span className="sr-only">{t("common:common.actions")}</span>
                </Th>
              </Tr>
            </THead>
            <TBody>
              {query.data.map((p) => (
                <Tr key={p.id}>
                  <Td className="font-medium">
                    {p.name} {p.isDefault && <Badge tone="primary">{t("ai.default")}</Badge>}
                  </Td>
                  <Td>{t(`ai.kind.${p.kind}`)}</Td>
                  <Td className="font-mono">{p.model}</Td>
                  <Td>{p.hasApiKey ? t("ai.keySet") : "—"}</Td>
                  <Td>
                    <ActiveBadge active={p.active} />
                  </Td>
                  <Td>
                    <Button
                      variant="ghost"
                      size="icon"
                      aria-label={t("common:actions.editItem", { name: p.name })}
                      onClick={() => setEditing(p)}
                    >
                      <Pencil />
                    </Button>
                    <DeleteButton name={p.name} onDelete={() => remove.mutateAsync(p.id)} />
                  </Td>
                </Tr>
              ))}
            </TBody>
          </Table>
        )}
      </Card>
      <Dialog open={editing !== null} onOpenChange={(o) => !o && setEditing(null)}>
        {editing !== null && (
          <DialogContent title={editing === "new" ? t("ai.new") : t("ai.edit")}>
            <AiProviderForm provider={editing === "new" ? undefined : editing} onDone={() => setEditing(null)} />
          </DialogContent>
        )}
      </Dialog>
    </>
  );
}
