import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import type { ApiError } from "@/api/client";
import { isApiError } from "@/api/client";
import { useCustomersList } from "@/api/generated/atc/customers/customers";
import {
  extractionsStart,
  getExtractionsFileUrl,
  useExtractionsFindings,
  useExtractionsLatest,
  useExtractionsList,
  useExtractionsMonitor,
} from "@/api/generated/atc/extractions/extractions";
import type {
  ExtractionsFindingsParams,
  ExtractionsListParams,
  RunResponse,
  SessionResponse,
} from "@/api/generated/atc/model";
import { useRunsGet, useRunsList } from "@/api/generated/atc/runs/runs";
import { useSapSystemsList } from "@/api/generated/atc/sap-systems/sap-systems";
import { sessionsCreate } from "@/api/generated/atc/sessions/sessions";
import { getAccessTokenForHub } from "@/api/token";
import { payload, unwrapped } from "@/api/unwrapped";
import { isRunActive } from "@/features/shared/status";
import { env } from "@/lib/env";

/** Clientes ativos para o seletor de ambiente. */
export function useCustomerOptions() {
  return unwrapped(useCustomersList({ page: 1, pageSize: 200, active: true }));
}

/** Ambientes ativos (do cliente escolhido, ou todos). */
export function useEnvironmentOptions(customerId: string) {
  return unwrapped(useSapSystemsList({ page: 1, pageSize: 200, active: true, customerId: customerId || undefined }));
}

/** Última execução concluída do ATC de série ZMIG_/Z_MIG_ do ambiente (consulta síncrona do back). */
export function useAtcLatest(sapSystemId: string) {
  return unwrapped(useExtractionsLatest(sapSystemId, { query: { enabled: !!sapSystemId } }));
}

export function useExtractionHistory(params: ExtractionsListParams) {
  return unwrapped(useExtractionsList(params, { query: { enabled: !!params.sapSystemId } }));
}

export function useExtractionFindings(id: string, params: ExtractionsFindingsParams) {
  return unwrapped(useExtractionsFindings(id, params, { query: { enabled: !!id } }));
}

/** Atualização do monitor: rápida enquanto há extração na fila ou rodando, lenta quando está tudo parado. */
export const MONITOR_FAST_MS = 3000;
export const MONITOR_IDLE_MS = 15000;

/** Monitor: ambientes ativos (de um cliente, opcional) com a última execução de extração e a última extração gravada. */
export function useExtractionMonitor(customerId: string) {
  return unwrapped(
    useExtractionsMonitor(customerId ? { customerId } : undefined, {
      query: {
        refetchInterval: (query) => {
          const rows = (query.state.data as unknown as { lastRun?: { status: string } | null }[] | undefined) ?? [];
          return rows.some((r) => r.lastRun && isRunActive(r.lastRun.status)) ? MONITOR_FAST_MS : MONITOR_IDLE_MS;
        },
      },
    }),
  );
}

/** Dispara o run de extração (202). Conflito de ambiente = 409 `Run.EnvironmentBusy`. */
export function useStartExtraction() {
  return useMutation<RunResponse, ApiError, { sapSystemId: string; force: boolean }>({
    mutationFn: (cmd) => payload(extractionsStart(cmd)),
  });
}

/** Resultado do disparo de um ambiente no lote: ok ou a mensagem do erro (409 ambiente ocupado, 404…). */
export type ExtractionDispatch =
  | { sapSystemId: string; ok: true; run: RunResponse }
  | { sapSystemId: string; ok: false; message: string };

/**
 * Dispara a extração de vários ambientes ao mesmo tempo (um run por ambiente; o back roda em paralelo até o limite do
 * worker e enfileira o resto). Um ambiente com erro não impede os outros. Ao final atualiza o monitor e os históricos.
 */
export function useStartExtractions() {
  const queryClient = useQueryClient();
  return useMutation<ExtractionDispatch[], ApiError, { sapSystemIds: string[]; force: boolean; failedText: string }>({
    mutationFn: async ({ sapSystemIds, force, failedText }) => {
      const results = await Promise.allSettled(
        sapSystemIds.map((sapSystemId) => payload(extractionsStart({ sapSystemId, force }))),
      );
      return results.map((r, i): ExtractionDispatch => {
        const sapSystemId = sapSystemIds[i] as string;
        return r.status === "fulfilled"
          ? { sapSystemId, ok: true, run: r.value }
          : { sapSystemId, ok: false, message: isApiError(r.reason) ? r.reason.title : failedText };
      });
    },
    onSettled: () =>
      queryClient.invalidateQueries({
        predicate: (q) => typeof q.queryKey[0] === "string" && q.queryKey[0].startsWith("/v1/extractions"),
      }),
  });
}

/** Cria a sessão de remediação a partir de uma extração; quem chama navega para `/remediacao/:id`. */
export function useCreateSession() {
  return useMutation<
    SessionResponse,
    ApiError,
    { extractionId: string; priorities: "1" | "1,2" | "1,2,3"; request?: string | null }
  >({
    mutationFn: (cmd) => payload(sessionsCreate(cmd)),
  });
}

/** Run que está bloqueando o ambiente (o ProblemDetails do 409 não traz o run pelo `ApiError`, então busca o ativo). */
export function useBlockingRun(sapSystemId: string, error: unknown) {
  const busy = isApiError(error) && error.status === 409;
  const q = unwrapped(
    useRunsList({ sapSystemId, activeOnly: true, page: 1, pageSize: 1 }, { query: { enabled: busy && !!sapSystemId } }),
  );
  return busy ? (q.data?.items[0] ?? null) : null;
}

/** Quando o run termina, atualiza o cartão "última execução" e o histórico. */
export function useRefreshWhenRunEnds(runId: string | undefined) {
  const queryClient = useQueryClient();
  const run = unwrapped(useRunsGet(runId ?? "", { query: { enabled: !!runId } })).data;
  const status = run?.status;
  useEffect(() => {
    if (status === "Completed" || status === "Failed" || status === "Cancelled") {
      void queryClient.invalidateQueries({
        predicate: (q) =>
          typeof q.queryKey[0] === "string" &&
          (q.queryKey[0].startsWith("/v1/extractions") || q.queryKey[0].includes("/atc/latest")),
      });
    }
  }, [status, queryClient]);
  return run;
}

export type ExtractionFileKind = "total" | "novos" | "remediacao";

function fileNameFrom(disposition: string | null, fallback: string) {
  const star = disposition?.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
  if (star) return decodeURIComponent(star);
  return disposition?.match(/filename="?([^";]+)"?/i)?.[1] ?? fallback;
}

/**
 * Baixa um dos 3 CSVs (total, novos, remediação com filtro de prioridades). O mutator do client não devolve arquivo
 * (lê JSON), então a requisição é feita aqui com o mesmo token; o nome vem do Content-Disposition do back.
 */
export function useDownloadExtractionFile() {
  return useMutation<
    void,
    ApiError,
    { id: string; kind: ExtractionFileKind; priorities?: string; fallbackName: string }
  >({
    mutationFn: async ({ id, kind, priorities, fallbackName }) => {
      const token = await getAccessTokenForHub();
      const res = await fetch(
        `${env.VITE_API_URL}${getExtractionsFileUrl(id, kind, priorities ? { priorities } : undefined)}`,
        {
          headers: token ? { Authorization: `Bearer ${token}` } : {},
        },
      );
      if (!res.ok) {
        const b = (await res.json().catch(() => null)) as { title?: string; detail?: string } | null;
        throw {
          status: res.status,
          title: b?.title ?? "Erro inesperado",
          code: b?.detail ?? "Unknown",
        } satisfies ApiError;
      }
      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileNameFrom(res.headers.get("content-disposition"), `${fallbackName}_${kind}.csv`);
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
    },
  });
}
