/** Estado da página da sessão na URL (aba, filtros dos itens, item aberto, run em exibição). */
export const SESSION_DEFAULTS = {
  tab: "items",
  page: "1",
  status: "",
  priority: "",
  objectName: "",
  group: "",
  triage: "",
  item: "",
  run: "",
  /** Run do play (remediação automática) em exibição no painel acima das abas. */
  auto: "",
  /** Run da leitura automática dos fontes (sessão recém-criada), ou o código do motivo de não ter começado. */
  sources: "",
  sourcesNotStarted: "",
};

export type SessionSearch = typeof SESSION_DEFAULTS;
export type SetSessionSearch = (patch: Partial<SessionSearch>) => void;

export const TABS = ["items", "sources", "triage", "groups", "apply", "export", "history"] as const;
