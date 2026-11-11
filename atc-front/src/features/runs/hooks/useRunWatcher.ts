import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import type { ApiError } from "@/api/client";
import type {
  EventLevel,
  RunAiCallResponsePagedResult,
  RunEventResponsePagedResult,
  RunResponse,
} from "@/api/generated/atc/model";
import {
  getRunsAiCallsQueryKey,
  getRunsEventsQueryKey,
  getRunsGetQueryKey,
  getRunsListQueryKey,
  runsCancel,
  runsRetry,
  useRunsAiCalls,
  useRunsEvents,
  useRunsGet,
} from "@/api/generated/atc/runs/runs";
import { useRunLiveUpdates } from "@/api/run-hub";
import { payload, unwrapped } from "@/api/unwrapped";
import { isRunActive } from "@/features/shared/status";
import type { AiCall, RunEvent, RunStep } from "@/features/shared/types";

export const RUN_POLL_MS = 3000;
export const EVENTS_PAGE_SIZE = 50;

/** DTO do passo -> tipo simples do StepsTimeline. */
export function toRunSteps(run: RunResponse): RunStep[] {
  return run.steps.map((s) => ({
    code: s.code,
    order: s.order,
    status: s.status,
    messageCode: s.messageCode,
    messageArgs: s.messageArgs,
    progress: s.progressTotal > 0 ? { done: s.progressDone, total: s.progressTotal } : null,
  }));
}

export function toRunEvents(page: RunEventResponsePagedResult | undefined): RunEvent[] {
  return (page?.items ?? []).map((e) => ({
    id: e.id,
    at: e.at,
    level: e.level,
    code: e.code,
    args: e.args,
    itemRef: e.itemRef,
  }));
}

export function toAiCalls(page: RunAiCallResponsePagedResult | undefined): AiCall[] {
  return (page?.items ?? []).map((c) => ({
    usage: c.usage,
    model: c.model,
    inputTokens: c.inputTokens,
    outputTokens: c.outputTokens,
    cost: c.cost,
  }));
}

/**
 * Acompanha um Run: dados, passos, log e chamadas de IA, ao vivo (SignalR) + polling de 3 s enquanto ativo.
 * Reutilizável pelos outros módulos (ex.: depois de disparar um run, mostrar o painel inline).
 */
export function useRunWatcher(runId: string | undefined, opts: { eventsLevel?: EventLevel } = {}) {
  const queryClient = useQueryClient();
  const id = runId ?? "";
  const enabled = !!runId;
  const [eventsPage, setEventsPage] = useState(1);
  const [aiPage, setAiPage] = useState(1);

  const runQuery = unwrapped(
    useRunsGet(id, {
      query: {
        enabled,
        refetchInterval: (q) => {
          const data = q.state.data as unknown as RunResponse | undefined;
          return data && isRunActive(data.status) ? RUN_POLL_MS : false;
        },
      },
    }),
  );
  const run = runQuery.data;
  const active = !!run && isRunActive(run.status);
  const refetchInterval = active ? RUN_POLL_MS : false;

  const eventsParams = { page: eventsPage, pageSize: EVENTS_PAGE_SIZE, level: opts.eventsLevel };
  const aiParams = { page: aiPage, pageSize: EVENTS_PAGE_SIZE };
  const eventsQuery = unwrapped(useRunsEvents(id, eventsParams, { query: { enabled, refetchInterval } }));
  const aiCallsQuery = unwrapped(useRunsAiCalls(id, aiParams, { query: { enabled, refetchInterval } }));

  useRunLiveUpdates(runId, active, [getRunsGetQueryKey(id), getRunsEventsQueryKey(id), getRunsAiCallsQueryKey(id)]);

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: getRunsGetQueryKey(id) });
    void queryClient.invalidateQueries({ queryKey: getRunsEventsQueryKey(id) });
    void queryClient.invalidateQueries({ queryKey: getRunsListQueryKey() });
  };
  const cancel = useMutation<RunResponse, ApiError, void>({
    mutationFn: () => payload(runsCancel(id)),
    onSuccess: invalidate,
  });
  /** Tentar de novo devolve o run novo (id diferente): quem chama decide navegar. */
  const retry = useMutation<RunResponse, ApiError, void>({
    mutationFn: () => payload(runsRetry(id)),
    onSuccess: invalidate,
  });

  return {
    run,
    steps: run ? toRunSteps(run) : [],
    active,
    isLoading: runQuery.isLoading,
    error: runQuery.error,
    events: {
      items: toRunEvents(eventsQuery.data),
      page: eventsPage,
      pageSize: EVENTS_PAGE_SIZE,
      total: eventsQuery.data?.total ?? 0,
      isLoading: eventsQuery.isLoading,
      error: eventsQuery.error,
      setPage: setEventsPage,
    },
    aiCalls: {
      items: toAiCalls(aiCallsQuery.data),
      page: aiPage,
      total: aiCallsQuery.data?.total ?? 0,
      pageSize: EVENTS_PAGE_SIZE,
      isLoading: aiCallsQuery.isLoading,
      error: aiCallsQuery.error,
      setPage: setAiPage,
    },
    cancel,
    retry,
  };
}
