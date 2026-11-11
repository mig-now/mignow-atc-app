import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import type { RunResponse, RunsListParams } from "@/api/generated/atc/model";
import { getRunsListQueryKey, runsCancel, useRunsList } from "@/api/generated/atc/runs/runs";
import { payload, unwrapped } from "@/api/unwrapped";

/** Runs ativos de todos os ambientes (polling de 5 s; a tela de monitor tem o ao vivo). */
export function useActiveRuns() {
  return unwrapped(useRunsList({ activeOnly: true, pageSize: 100 }, { query: { refetchInterval: 5000 } }));
}

export function useRunsHistory(params: RunsListParams) {
  return unwrapped(useRunsList(params));
}

export function useCancelRun() {
  const queryClient = useQueryClient();
  return useMutation<RunResponse, ApiError, string>({
    mutationFn: (id) => payload(runsCancel(id)),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getRunsListQueryKey().slice(0, 1) }),
  });
}
