import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import type { ApiError } from "@/api/client";
import { isApiError } from "@/api/client";
import {
  exemptionsCheck,
  exemptionsCreate,
  exemptionsDiscover,
  exemptionsSend,
  exemptionsSimulate,
  exemptionsUpdateParams,
  useExemptionsGet,
  useExemptionsList,
  useExemptionsListRequests,
} from "@/api/generated/atc/exemptions/exemptions";
import type {
  ExemptionBatchCreatedResponse,
  ExemptionBatchResponse,
  ExemptionsListParams,
  ExemptionsListRequestsParams,
  RunResponse,
  UpdateExemptionParamsRequest,
} from "@/api/generated/atc/model";
import { useRunsGet, useRunsList } from "@/api/generated/atc/runs/runs";
import { useSapSystemsGet, useSapSystemsList } from "@/api/generated/atc/sap-systems/sap-systems";
import { useSessionsList } from "@/api/generated/atc/sessions/sessions";
import { payload, unwrapped } from "@/api/unwrapped";
import { isRunActive } from "@/features/shared/status";
import { BATCH_RUN_KINDS } from "../lib";

const isBatchKey = (key: readonly unknown[]) =>
  typeof key[0] === "string" && key[0].startsWith("/v1/exemption-batches");

export function useBatches(params: ExemptionsListParams) {
  return unwrapped(useExemptionsList(params));
}

export function useBatch(id: string) {
  return unwrapped(useExemptionsGet(id, { query: { enabled: !!id } }));
}

export function useBatchRequests(id: string, params: ExemptionsListRequestsParams) {
  return unwrapped(useExemptionsListRequests(id, params, { query: { enabled: !!id } }));
}

export function useSystemOptions() {
  return unwrapped(useSapSystemsList({ page: 1, pageSize: 200 }));
}

export function useSystem(id: string) {
  return unwrapped(useSapSystemsGet(id, { query: { enabled: !!id } }));
}

/** Sessões de remediação (não arquivadas, por padrão) para filtro e para "Novo lote". */
export function useSessionOptions(opts: { sapSystemId?: string; archived?: boolean } = {}) {
  return unwrapped(
    useSessionsList({ page: 1, pageSize: 100, sapSystemId: opts.sapSystemId || undefined, archived: opts.archived }),
  );
}

/** Runs do lote (subjectId = lote), mais novos primeiro; polling de 3 s enquanto houver um ativo. */
export function useBatchRuns(batch: ExemptionBatchResponse | undefined) {
  const q = unwrapped(
    useRunsList(
      { sapSystemId: batch?.sapSystemId, subjectId: batch?.id, page: 1, pageSize: 100 },
      {
        query: {
          enabled: !!batch,
          refetchInterval: (query) => {
            const data = query.state.data as unknown as { items?: RunResponse[] } | undefined;
            return data?.items?.some((r) => isRunActive(r.status)) ? 3000 : false;
          },
        },
      },
    ),
  );
  const runs = (q.data?.items ?? [])
    .filter((r) => (BATCH_RUN_KINDS as readonly string[]).includes(r.kind))
    .sort((a, b) => b.queuedAt.localeCompare(a.queuedAt));
  return { ...q, runs };
}

export function useCreateBatch() {
  const queryClient = useQueryClient();
  return useMutation<
    ExemptionBatchCreatedResponse,
    ApiError,
    { sessionId: string; priorities: number[]; includeObs: boolean }
  >({
    mutationFn: (cmd) => payload(exemptionsCreate(cmd)),
    onSuccess: () => queryClient.invalidateQueries({ predicate: (q) => isBatchKey(q.queryKey) }),
  });
}

export function useUpdateParams(batchId: string) {
  const queryClient = useQueryClient();
  return useMutation<ExemptionBatchResponse, ApiError, UpdateExemptionParamsRequest>({
    mutationFn: (req) => payload(exemptionsUpdateParams(batchId, req)),
    onSuccess: () => queryClient.invalidateQueries({ predicate: (q) => isBatchKey(q.queryKey) }),
  });
}

export type BatchAction = "discover" | "simulate" | "send" | "check";

/** Dispara o run da etapa (202 + RunResponse). `seq` restringe a simulação/envio a alguns findings. */
export function useBatchAction(batchId: string, action: BatchAction) {
  const queryClient = useQueryClient();
  return useMutation<RunResponse, ApiError, { seq?: number[] } | undefined>({
    mutationFn: (v) => {
      const body = v?.seq?.length ? { seq: v.seq } : {};
      switch (action) {
        case "discover":
          return payload(exemptionsDiscover(batchId));
        case "simulate":
          return payload(exemptionsSimulate(batchId, body));
        case "send":
          return payload(exemptionsSend(batchId, body));
        case "check":
          return payload(exemptionsCheck(batchId));
      }
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["/v1/runs"] }),
  });
}

/** Run que bloqueia o ambiente quando a API responde 409 (o `ApiError` não carrega o run, então busca o ativo). */
export function useBlockingRun(sapSystemId: string, error: unknown) {
  const busy = isApiError(error) && error.status === 409;
  const q = unwrapped(
    useRunsList({ sapSystemId, activeOnly: true, page: 1, pageSize: 1 }, { query: { enabled: busy && !!sapSystemId } }),
  );
  return busy ? (q.data?.items[0] ?? null) : null;
}

/** Quando o run termina, recarrega o lote (status, totais, pedidos) e a lista de runs. */
export function useRefreshWhenRunEnds(runId: string | undefined) {
  const queryClient = useQueryClient();
  const status = unwrapped(useRunsGet(runId ?? "", { query: { enabled: !!runId } })).data?.status;
  useEffect(() => {
    if (status === "Completed" || status === "Failed" || status === "Cancelled") {
      void queryClient.invalidateQueries({
        predicate: (q) => isBatchKey(q.queryKey) || (typeof q.queryKey[0] === "string" && q.queryKey[0] === "/v1/runs"),
      });
    }
  }, [status, queryClient]);
}
