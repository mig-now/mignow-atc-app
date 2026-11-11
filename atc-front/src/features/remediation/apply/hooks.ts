import { type QueryClient, useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import type {
  ActivationDecisionResponse,
  DecideItemsResponse,
  DecideProposalResponse,
  DecisionRequest,
  ItemResponse,
  RunResponse,
  SessionApplyDecisionRequest,
  SessionApplyItemsRequest,
  SessionApplyProposeRequest,
} from "@/api/generated/atc/model";
import { useSapSystemsGet } from "@/api/generated/atc/sap-systems/sap-systems";
import {
  sessionApplyActivationDecision,
  sessionApplyApply,
  sessionApplyApplySimulate,
  sessionApplyDecide,
  sessionApplyPropose,
  useSessionApplyGetProposal,
} from "@/api/generated/atc/session-apply/session-apply";
import {
  sessionsDecide,
  useSessionsGet,
  useSessionsGroups,
  useSessionsItems,
} from "@/api/generated/atc/sessions/sessions";
import { payload, unwrapped } from "@/api/unwrapped";

/** Sem paginação na tela: a sessão inteira (o back aceita até 500 por página). */
export const ITEMS_PAGE_SIZE = 500;

/** Status exatos da skill (`""` = pendente). */
export const STATUS = {
  pending: "",
  kb: "Confirmado KB",
  ai: "Confirmado IA",
  cancelled: "Cancelado",
  exemption: "Isenção",
} as const;

/** Invalida tudo o que é da sessão (itens, grupos, propostas, sessão) depois de uma gravação. */
function invalidateSession(queryClient: QueryClient, sessionId: string) {
  const prefix = `/v1/sessions/${sessionId}`;
  return queryClient.invalidateQueries({
    predicate: (q) => typeof q.queryKey[0] === "string" && q.queryKey[0].startsWith(prefix),
  });
}

/** Sessão + ambiente SAP (para a request e o bloqueio de produção). */
export function useApplyContext(sessionId: string) {
  const session = unwrapped(useSessionsGet(sessionId));
  const sapSystemId = session.data?.sapSystemId ?? "";
  const system = unwrapped(useSapSystemsGet(sapSystemId, { query: { enabled: !!sapSystemId } }));
  return {
    session: session.data,
    system: system.data,
    isLoading: session.isLoading || (!!sapSystemId && system.isLoading),
    error: session.error ?? system.error,
    isProduction: system.data?.isProduction ?? false,
  };
}

export function useApplyItems(sessionId: string) {
  const q = unwrapped(useSessionsItems(sessionId, { pageSize: ITEMS_PAGE_SIZE }));
  return { ...q, items: (q.data?.items ?? []) as ItemResponse[] };
}

export function useApplyGroups(sessionId: string) {
  return unwrapped(useSessionsGroups(sessionId));
}

/** Proposta de um item (carrega só quando `enabled`). */
export function useProposal(sessionId: string, itemId: string, enabled = true) {
  return unwrapped(useSessionApplyGetProposal(sessionId, itemId, { query: { enabled } }));
}

export function useProposeRun(sessionId: string) {
  return useMutation<RunResponse, ApiError, SessionApplyProposeRequest>({
    mutationFn: (body) => payload(sessionApplyPropose(sessionId, body)),
  });
}

/** Confirmar KB / Confirmar IA da proposta (`proposals/decision`). */
export function useDecideProposal(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation<DecideProposalResponse, ApiError, SessionApplyDecisionRequest>({
    mutationFn: (body) => payload(sessionApplyDecide(sessionId, body)),
    onSuccess: () => invalidateSession(queryClient, sessionId),
  });
}

/** Cancelar / Isenção / Reset (`items/decision`). */
export function useDecideItems(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation<DecideItemsResponse, ApiError, DecisionRequest>({
    mutationFn: (body) => payload(sessionsDecide(sessionId, body)),
    onSuccess: () => invalidateSession(queryClient, sessionId),
  });
}

export function useSimulateRun(sessionId: string) {
  return useMutation<RunResponse, ApiError, SessionApplyItemsRequest>({
    mutationFn: (body) => payload(sessionApplyApplySimulate(sessionId, body)),
  });
}

export function useApplyRun(sessionId: string) {
  return useMutation<RunResponse, ApiError, SessionApplyItemsRequest>({
    mutationFn: (body) => payload(sessionApplyApply(sessionId, body)),
  });
}

/** Erro de ativação: `keepInactive` ou `rollback` (este devolve o run Rollback). */
export function useActivationDecision(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation<ActivationDecisionResponse, ApiError, { itemId: string; decision: "keepInactive" | "rollback" }>({
    mutationFn: ({ itemId, decision }) => payload(sessionApplyActivationDecision(sessionId, itemId, { decision })),
    onSuccess: () => invalidateSession(queryClient, sessionId),
  });
}

/** Chama `invalidateSession` quando um run termina (propose/simulate/apply/rollback mudam itens e propostas). */
export function useInvalidateSession(sessionId: string) {
  const queryClient = useQueryClient();
  return () => invalidateSession(queryClient, sessionId);
}

/** Resumo do `ResultJson` do run Propose (com ou sem o envelope do modo automático). */
export type ProposeResult = {
  reportMarkdown?: string;
  factCandidates: { n: number; element: string; fact: string; proof?: string | null; impact?: string | null }[];
  summary?: Record<string, number>;
};

export function parseProposeResult(result: unknown): ProposeResult {
  const r = (result ?? {}) as Record<string, unknown>;
  const summary = ((r.propose as Record<string, unknown> | undefined) ?? r) as Record<string, unknown>;
  const facts = Array.isArray(summary.factCandidates)
    ? (summary.factCandidates as ProposeResult["factCandidates"])
    : [];
  const counts = Object.fromEntries(Object.entries(summary).filter(([, v]) => typeof v === "number")) as Record<
    string,
    number
  >;
  return {
    reportMarkdown: typeof r.reportMarkdown === "string" ? r.reportMarkdown : undefined,
    factCandidates: facts,
    summary: Object.keys(counts).length ? counts : undefined,
  };
}
