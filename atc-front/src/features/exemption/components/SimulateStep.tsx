import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ExemptionBatchResponse, RunResponse } from "@/api/generated/atc/model";
import { Field } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { resultOf } from "../lib";
import { RequestsTable } from "./RequestsTable";
import { RunStepPanel } from "./RunStepPanel";

type TableState = { status: string; page: number; onStatus: (s: string) => void; onPage: (p: number) => void };

/** "1, 3,5" -> [1,3,5]; vazio ou inválido = todos. */
export function parseSeq(text: string): number[] | undefined {
  const seq = text
    .split(/[,\s]+/)
    .filter(Boolean)
    .map(Number);
  return seq.length > 0 && seq.every((n) => Number.isInteger(n) && n > 0) ? seq : undefined;
}

/** Etapa 3: simulação (somente leitura no SAP) + pedidos com status SIMULADO / BLOQUEADO / SEM_ESCOPO / JA_PEDIDO e filtro. */
export function SimulateStep({
  batch,
  runs,
  table,
}: {
  batch: ExemptionBatchResponse;
  runs: RunResponse[];
  table: TableState;
}) {
  const { t } = useTranslation("exemption");
  const [seqText, setSeqText] = useState("");
  const lastSimulation = runs.find((r) => r.kind === "ExemptionSimulate" && r.status === "Completed");
  const example = resultOf(lastSimulation)?.exampleJustification;
  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">{t("simulate.intro")}</p>
      <RunStepPanel
        batch={batch}
        runs={runs}
        action="simulate"
        label={t("simulate.run")}
        seq={parseSeq(seqText)}
        extra={
          <Field id="sim-seq" label={t("simulate.seq")} hint={t("simulate.seqHint")}>
            <Input id="sim-seq" className="w-48" value={seqText} onChange={(e) => setSeqText(e.target.value)} />
          </Field>
        }
      />
      {example && (
        <p className="rounded-md border border-border bg-muted/40 px-3 py-2 text-sm">
          <span className="font-medium">{t("simulate.example")}</span> <span className="font-mono">{example}</span>
        </p>
      )}
      <RequestsTable batchId={batch.id} {...table} />
    </div>
  );
}
