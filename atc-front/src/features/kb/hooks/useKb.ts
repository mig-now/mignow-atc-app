import { useKnowledgeGetElement, useKnowledgeIndex, useKnowledgeLookup } from "@/api/generated/atc/knowledge/knowledge";
import { unwrapped } from "@/api/unwrapped";

/** Índice da KB (elementos + padrões P01..P99), com filtro por categoria e busca. */
export function useKbIndex(params: { category?: string; search?: string }) {
  const clean = { category: params.category || undefined, search: params.search || undefined };
  return unwrapped(useKnowledgeIndex(clean));
}

/** Página de um elemento (o `element` da rota é o slug do índice). */
export function useKbElement(category: string, element: string) {
  return unwrapped(useKnowledgeGetElement(category, element));
}

/** Busca rápida (ref + RObT -> top N soluções); só consulta com `ref` preenchida. */
export function useKbLookup(params: { ref: string; robt: string; top: number }) {
  const enabled = params.ref.trim().length > 0;
  return unwrapped(
    useKnowledgeLookup(
      { ref: params.ref.trim(), robt: params.robt.trim() || undefined, top: params.top },
      { query: { enabled } },
    ),
  );
}
