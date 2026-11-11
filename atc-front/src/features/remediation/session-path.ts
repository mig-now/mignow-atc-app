import type { SessionResponse } from "@/api/generated/atc/model";

/**
 * Caminho da sessão recém-criada: a leitura dos fontes começa sozinha ao carregar os itens – o painel acompanha o run (`sources`) ou mostra
 * por que não começou (`sourcesNotStarted`, ex.: ambiente ocupado).
 */
export function newSessionPath(s: SessionResponse) {
  const q = s.sourcesRunId
    ? `?sources=${s.sourcesRunId}`
    : s.sourcesNotStarted
      ? `?sourcesNotStarted=${encodeURIComponent(s.sourcesNotStarted)}`
      : "";
  return `/remediacao/${s.id}${q}`;
}
