import { useTranslation } from "react-i18next";
import type { ExemptionBatchResponse, RunResponse } from "@/api/generated/atc/model";
import { Card } from "@/components/ui/card";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { resultOf } from "../lib";
import { RunStepPanel } from "./RunStepPanel";

/** Etapa 5: conferência no SAP (somente leitura): situação de cada finding pedido, do último run de conferência. */
export function CheckStep({ batch, runs }: { batch: ExemptionBatchResponse; runs: RunResponse[] }) {
  const { t } = useTranslation("exemption");
  const last = runs.find((r) => r.kind === "ExemptionCheck" && r.status === "Completed");
  const situations = resultOf(last)?.situations ?? [];
  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">{t("check.intro")}</p>
      <RunStepPanel batch={batch} runs={runs} action="check" label={t("check.run")} />
      {last && (
        <Card>
          {situations.length === 0 ? (
            <p className="px-4 py-3 text-sm text-muted-foreground">{t("check.empty")}</p>
          ) : (
            <Table>
              <THead>
                <Tr>
                  <Th>{t("col.seq")}</Th>
                  <Th>{t("col.item")}</Th>
                  <Th>{t("col.object")}</Th>
                  <Th>{t("col.marker")}</Th>
                  <Th>{t("col.sapMessage")}</Th>
                </Tr>
              </THead>
              <TBody>
                {situations.map((s) => (
                  <Tr key={`${s.seq}-${s.markerId}`}>
                    <Td>{s.seq}</Td>
                    <Td>{s.itemNumber ?? "—"}</Td>
                    <Td className="font-mono">{s.objectName ?? "—"}</Td>
                    <Td className="font-mono">{s.markerId ?? "—"}</Td>
                    <Td>{s.sapMessage ?? s.situationCode ?? "—"}</Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          )}
        </Card>
      )}
    </div>
  );
}
