import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import type { RemediationMonitorRow, RunResponse, SessionResponse } from "@/api/generated/atc/model";
import {
  sessionsSetRequest,
  sessionsStartAutoRemediation,
  useSessionsMonitor,
} from "@/api/generated/atc/sessions/sessions";
import { payload, unwrapped } from "@/api/unwrapped";
import { isRunActive } from "@/features/shared/status";
import { invalidateSessions } from "./useSessions";

/** Atualização do monitor: rápida enquanto alguma sessão tem execução na fila ou rodando, lenta quando está tudo parado. */
export const MONITOR_FAST_MS = 3000;
export const MONITOR_IDLE_MS = 15000;

/** Monitor de remediação: sessões (cliente opcional, arquivadas só a pedido) com a situação dos itens e a última execução. */
export function useRemediationMonitor(customerId: string, includeArchived: boolean) {
  return unwrapped(
    useSessionsMonitor(
      { customerId: customerId || undefined, includeArchived },
      {
        query: {
          refetchInterval: (query) => {
            const rows = (query.state.data as unknown as RemediationMonitorRow[] | undefined) ?? [];
            return rows.some((r) => r.lastRun && isRunActive(r.lastRun.status)) ? MONITOR_FAST_MS : MONITOR_IDLE_MS;
          },
        },
      },
    ),
  );
}

/**
 * "Play" da remediação (202): os itens escolhidos (vazio = todos os pendentes) passam por fontes, triagem, R1, proposta KB/IA,
 * confirmação, simulação, revisão e aplicação, sem parada. As regras de triagem vêm do tipo de projeto da sessão (back). Conflito de ambiente = 409; produção = 403; sem request = 409.
 */
export function useStartAutoRemediation(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation<RunResponse, ApiError, { itemIds: string[] }>({
    mutationFn: ({ itemIds }) =>
      payload(sessionsStartAutoRemediation(sessionId, { itemIds: itemIds.length ? itemIds : null })),
    onSuccess: () => invalidateSessions(queryClient),
  });
}

/** Define a request de transporte da sessão (exigida para aplicar, inclusive pelo play). */
export function useSetSessionRequest(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation<SessionResponse, ApiError, string>({
    mutationFn: (request) => payload(sessionsSetRequest(sessionId, { request })),
    onSuccess: () => invalidateSessions(queryClient),
  });
}
