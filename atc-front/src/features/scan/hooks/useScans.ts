import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import type { ApiError } from "@/api/client";
import { useExtractionsList } from "@/api/generated/atc/extractions/extractions";
import type {
  CreateScanCommand,
  RunResponse,
  ScanResponse,
  ScanStartedResponse,
  ScansOccurrencesParams,
  SessionResponse,
  StartExecuteBody,
} from "@/api/generated/atc/model";
import { getRunsListQueryKey, useRunsList } from "@/api/generated/atc/runs/runs";
import { useSapSystemsList } from "@/api/generated/atc/sap-systems/sap-systems";
import {
  getScansGetQueryKey,
  getScansListQueryKey,
  getScansOccurrencesQueryKey,
  scansCleanup,
  scansCreate,
  scansExecute,
  scansImport,
  scansInventory,
  scansSetScope,
  scansSimulate,
  useScansGet,
  useScansList,
  useScansOccurrences,
  useScansRules,
} from "@/api/generated/atc/scans/scans";
import { payload, unwrapped } from "@/api/unwrapped";
import { isRunActive } from "@/features/shared/status";

export const SCAN_POLL_MS = 4000;
export const OCCURRENCES_PAGE_SIZE = 25;

/** Formas lidas dos JSON livres da API (escopo gravado pelo inventário; resumo do cruzamento). */
export type ScanScope = {
  escopo?: number;
  tadir_lidos?: number;
  por_tipo?: Record<string, number>;
  excluidos?: { tmp?: number; marcados_exclusao?: number; gerados?: number };
  incluir_gerados?: boolean;
  peso_por_lote?: number;
  lotes?: number;
};
export type ScanSummary = {
  ocorrencias?: Record<string, number>;
  cruzamento_pendentes?: Record<string, number>;
  mignow?: { ocorrencias?: number; objetos?: number };
  csv_remediacao?: { arquivo?: string; linhas?: number };
  lidos?: Record<string, number>;
  comparacao_original_x_refinada?: Record<string, number> | null;
};
export const scopeOf = (s: ScanResponse) => (s.scope ?? {}) as ScanScope;
export const summaryOf = (s: ScanResponse) => (s.summary ?? null) as ScanSummary | null;

export function useScanSystems() {
  return unwrapped(useSapSystemsList({ active: true, page: 1, pageSize: 200 }));
}

export function useScanList(sapSystemId: string, page: number) {
  return unwrapped(
    useScansList({ sapSystemId: sapSystemId || undefined, page, pageSize: 20 }, { query: { enabled: !!sapSystemId } }),
  );
}

/** Extrações do ambiente (a mais recente primeiro): base do cruzamento. */
export function useScanExtractions(sapSystemId: string) {
  return unwrapped(useExtractionsList({ sapSystemId, page: 1, pageSize: 50 }, { query: { enabled: !!sapSystemId } }));
}

export function useCreateScan() {
  const qc = useQueryClient();
  return useMutation<ScanStartedResponse, ApiError, CreateScanCommand>({
    mutationFn: (cmd) => payload(scansCreate(cmd)),
    onSuccess: () => void qc.invalidateQueries({ queryKey: getScansListQueryKey() }),
  });
}

export function useScan(scanId: string, polling: boolean) {
  return unwrapped(
    useScansGet(scanId, { query: { enabled: !!scanId, refetchInterval: polling ? SCAN_POLL_MS : false } }),
  );
}

export function useScanRules() {
  return unwrapped(useScansRules());
}

/** Run de varredura ativo do ambiente (sobrevive a recarregar a página). */
export function useActiveScanRun(sapSystemId: string | undefined) {
  const q = unwrapped(
    useRunsList(
      { sapSystemId, activeOnly: true, pageSize: 50 },
      { query: { enabled: !!sapSystemId, refetchInterval: SCAN_POLL_MS } },
    ),
  );
  return q.data?.items.find((r) => r.kind.startsWith("Scan"));
}

export function useOccurrences(
  scanId: string,
  filters: Omit<ScansOccurrencesParams, "page" | "pageSize">,
  page: number,
) {
  const params: ScansOccurrencesParams = {
    ruleId: filters.ruleId || undefined,
    cross: filters.cross || undefined,
    status: filters.status || undefined,
    objectName: filters.objectName || undefined,
    page,
    pageSize: OCCURRENCES_PAGE_SIZE,
  };
  return unwrapped(useScansOccurrences(scanId, params, { query: { enabled: !!scanId } }));
}

/** Atualiza o escopo (regras ativas) e a varredura quando o run termina. */
export function useScanRefreshOnRunEnd(scanId: string, run: RunResponse | undefined) {
  const qc = useQueryClient();
  const status = run?.status;
  const running = !!status && isRunActive(status);
  // Enquanto o Run está ativo, atualiza lotes/status da varredura a cada ciclo.
  useEffect(() => {
    if (!running) return;
    const timer = setInterval(() => void qc.invalidateQueries({ queryKey: getScansGetQueryKey(scanId) }), SCAN_POLL_MS);
    return () => clearInterval(timer);
  }, [running, scanId, qc]);
  useEffect(() => {
    if (status && !isRunActive(status)) {
      void qc.invalidateQueries({ queryKey: getScansGetQueryKey(scanId) });
      void qc.invalidateQueries({ queryKey: getScansOccurrencesQueryKey(scanId) });
      void qc.invalidateQueries({ queryKey: getRunsListQueryKey() });
    }
  }, [status, scanId, qc]);
}

export function useSetScope(scanId: string) {
  const qc = useQueryClient();
  return useMutation<ScanResponse, ApiError, string[]>({
    mutationFn: (ruleIds) => payload(scansSetScope(scanId, { ruleIds })),
    onSuccess: () => void qc.invalidateQueries({ queryKey: getScansGetQueryKey(scanId) }),
  });
}

/** Ações que disparam Run (202): o chamador guarda o run devolvido na URL. */
export function useScanActions(scanId: string) {
  const qc = useQueryClient();
  const onSuccess = () => void qc.invalidateQueries({ queryKey: getRunsListQueryKey() });
  const simulate = useMutation<RunResponse, ApiError, { lots?: string; parallel?: number }>({
    mutationFn: (p) => payload(scansSimulate(scanId, p)),
    onSuccess,
  });
  const execute = useMutation<RunResponse, ApiError, StartExecuteBody>({
    mutationFn: (b) => payload(scansExecute(scanId, b)),
    onSuccess,
  });
  const cleanup = useMutation<RunResponse, ApiError, void>({
    mutationFn: () => payload(scansCleanup(scanId)),
    onSuccess,
  });
  const inventory = useMutation<RunResponse, ApiError, boolean>({
    mutationFn: (includeGenerated) => payload(scansInventory(scanId, { includeGenerated })),
    onSuccess,
  });
  return { simulate, execute, cleanup, inventory };
}

/** Importa o CSV P1 como nova sessão de remediação (arquiva a sessão atual do ambiente). */
export function useImportScan(scanId: string) {
  return useMutation<SessionResponse, ApiError, boolean>({
    mutationFn: (includeOtherLine) => payload(scansImport(scanId, { includeOtherLine })),
  });
}
