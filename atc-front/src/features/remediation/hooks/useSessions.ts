import { type QueryClient, useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import type {
  DecideItemsResponse,
  RemediationGroup,
  ReuseRequest,
  ReuseResult,
  SessionDecision,
  SessionResponse,
  SessionsGroupsParams,
  SessionsImportCsvBody,
  SessionsItemsParams,
  SessionsListParams,
} from "@/api/generated/atc/model";
import {
  getSessionsExportUrl,
  sessionsCreate,
  sessionsDecide,
  sessionsImportCsv,
  sessionsItems,
  sessionsReuse,
  useSessionsGet,
  useSessionsGetItem,
  useSessionsGroups,
  useSessionsHistory,
  useSessionsItems,
  useSessionsList,
  useSessionsSource,
  useSessionsSources,
} from "@/api/generated/atc/sessions/sessions";
import { getAccessTokenForHub } from "@/api/token";
import { payload, unwrapped } from "@/api/unwrapped";
import { env } from "@/lib/env";

/** Invalida toda query de sessão (lista, cabeçalho, itens, grupos, triagem, histórico, propostas). */
export function invalidateSessions(queryClient: QueryClient) {
  return queryClient.invalidateQueries({
    predicate: (q) => typeof q.queryKey[0] === "string" && q.queryKey[0].startsWith("/v1/sessions"),
  });
}

export function useSessionList(params: SessionsListParams) {
  return unwrapped(useSessionsList(params));
}

export function useSession(id: string) {
  return unwrapped(useSessionsGet(id));
}

export function useSessionItems(id: string, params: SessionsItemsParams) {
  return unwrapped(useSessionsItems(id, params));
}

export function useSessionItem(id: string, itemId: string | undefined) {
  return unwrapped(useSessionsGetItem(id, itemId ?? "", { query: { enabled: !!itemId } }));
}

export function useGroups(id: string, params?: SessionsGroupsParams) {
  return unwrapped(useSessionsGroups(id, params));
}

/** Objetos da sessão com fonte lido (só consulta; a leitura é automática). */
export function useSessionSources(id: string) {
  return unwrapped(useSessionsSources(id));
}

/** Conteúdo de um fonte lido, para o popup. */
export function useSessionSource(id: string, sourceId: string) {
  return unwrapped(useSessionsSource(id, sourceId));
}

export function useSessionHistory(id: string) {
  return unwrapped(useSessionsHistory(id));
}

/** Cria a sessão a partir de uma extração (prioridades + request). */
export function useCreateSession() {
  const queryClient = useQueryClient();
  return useMutation<SessionResponse, ApiError, { extractionId: string; priorities: string; request: string | null }>({
    mutationFn: (body) => payload(sessionsCreate(body)),
    onSuccess: () => invalidateSessions(queryClient),
  });
}

/** Importa o CSV do ATC (multipart); devolve a sessão com o resumo da importação. */
export function useImportCsv() {
  const queryClient = useQueryClient();
  return useMutation<SessionResponse, ApiError, SessionsImportCsvBody>({
    mutationFn: (body) => payload(sessionsImportCsv(body)),
    onSuccess: () => invalidateSessions(queryClient),
  });
}

/** Ids dos itens de um grupo (a API de grupos devolve só os nº; a decisão pede os ids). */
export async function resolveGroupItemIds(sessionId: string, group: RemediationGroup): Promise<string[]> {
  const ns = new Set(group.items);
  const ids: string[] = [];
  for (let page = 1; ; page++) {
    const r = await payload(sessionsItems(sessionId, { group: group.ref, page, pageSize: 200 }));
    for (const it of r.items) if (ns.has(it.n)) ids.push(it.id);
    if (r.items.length === 0 || page * r.pageSize >= r.total) return ids;
  }
}

export type DecideInput = {
  decision: SessionDecision;
  obs: string | null;
  itemIds?: string[];
  groups?: RemediationGroup[];
};

/** Decisão simples (Cancelar / Isenção / Reset) por itens ou grupos. */
export function useDecide(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation<DecideItemsResponse, ApiError, DecideInput>({
    mutationFn: async ({ decision, obs, itemIds = [], groups = [] }) => {
      const ids = new Set(itemIds);
      for (const g of groups) for (const id of await resolveGroupItemIds(sessionId, g)) ids.add(id);
      return payload(sessionsDecide(sessionId, { itemIds: [...ids], decision, obs }));
    },
    onSuccess: () => invalidateSessions(queryClient),
  });
}

export function useReuse(sessionId: string) {
  return useMutation<ReuseResult, ApiError, ReuseRequest>({
    mutationFn: (body) => payload(sessionsReuse(sessionId, body)),
  });
}

/** Baixa o atc_status.csv (o mutator só lê JSON, por isso o download usa fetch com o mesmo token). */
export function useExportCsv(sessionId: string) {
  return useMutation<void, ApiError, void>({
    mutationFn: async () => {
      const token = await getAccessTokenForHub();
      const res = await fetch(`${env.VITE_API_URL}${getSessionsExportUrl(sessionId)}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!res.ok) {
        const b = (await res.json().catch(() => null)) as { title?: string; detail?: string } | null;
        throw {
          status: res.status,
          title: b?.title ?? "Erro inesperado",
          code: b?.detail ?? "Unknown",
        } satisfies ApiError;
      }
      const url = URL.createObjectURL(await res.blob());
      const a = document.createElement("a");
      a.href = url;
      a.download = "atc_status.csv";
      a.click();
      URL.revokeObjectURL(url);
    },
  });
}
