import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import { useCustomersList } from "@/api/generated/atc/customers/customers";
import type { HighlightsRequest, HighlightsResponse } from "@/api/generated/atc/model";
import {
  getReportsGetHighlightsQueryKey,
  getReportsGetQueryKey,
  reportsPutHighlights,
  useReportsGet,
  useReportsGetAnalysis,
  useReportsGetHighlights,
  useReportsGetHistory,
} from "@/api/generated/atc/reports/reports";
import { useSapSystemsList } from "@/api/generated/atc/sap-systems/sap-systems";
import { payload, unwrapped } from "@/api/unwrapped";

export const ANALYSIS_KINDS = ["isencao", "pendencias", "mignow"] as const;
export type AnalysisKind = (typeof ANALYSIS_KINDS)[number];
export const HISTORY_MODULES = ["extraction", "remediation", "exemption", "scan"] as const;

export function useReportCustomers() {
  return unwrapped(useCustomersList({ active: true, page: 1, pageSize: 200 }));
}

/** Ambientes do cliente (filtro opcional do relatório). */
export function useReportSystems(customerId: string) {
  return unwrapped(
    useSapSystemsList({ customerId, active: true, page: 1, pageSize: 200 }, { query: { enabled: !!customerId } }),
  );
}

/** Dados JSON do relatório (GET customers/{id}/report). */
export function useReport(customerId: string, sapSystemId: string) {
  const params = sapSystemId ? { sapSystemId } : undefined;
  return unwrapped(useReportsGet(customerId, params, { query: { enabled: !!customerId } }));
}

/** Resumo de uma análise (isencao | pendencias | mignow); 404 quando ainda não existe. */
export function useAnalysis(customerId: string, kind: AnalysisKind, sapSystemId: string) {
  const params = sapSystemId ? { sapSystemId } : undefined;
  return unwrapped(useReportsGetAnalysis(customerId, kind, params, { query: { enabled: !!customerId, retry: false } }));
}

export function useHighlights(customerId: string) {
  return unwrapped(useReportsGetHighlights(customerId, { query: { enabled: !!customerId } }));
}

export function useSaveHighlights(customerId: string) {
  const qc = useQueryClient();
  return useMutation<HighlightsResponse, ApiError, HighlightsRequest>({
    mutationFn: (body) => payload(reportsPutHighlights(customerId, body)),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: getReportsGetHighlightsQueryKey(customerId) });
      void qc.invalidateQueries({ queryKey: getReportsGetQueryKey(customerId) });
    },
  });
}

/** Histórico de processo (tempo e tokens por módulo); o back exige cliente ou ambiente. */
export function useProcessHistory(customerId: string, sapSystemId: string, module: string) {
  return unwrapped(
    useReportsGetHistory(
      { customerId: customerId || undefined, sapSystemId: sapSystemId || undefined, module: module || undefined },
      { query: { enabled: !!customerId || !!sapSystemId } },
    ),
  );
}
