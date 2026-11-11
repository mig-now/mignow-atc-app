/** Tipos locais e simples dos componentes shared: independentes do client gerado (o módulo mapeia o DTO para estes). */
export type MessageArgs = Record<string, string | number | null | undefined> | null | undefined;

export type RunStep = {
  code: string;
  order: number;
  /** Pending | Processing | Completed | Error | Skipped */
  status: string;
  messageCode?: string | null;
  messageArgs?: MessageArgs;
  /** Progresso `done/total` do passo, quando houver. */
  progress?: { done: number; total: number } | null;
};

export type RunEvent = {
  id: number | string;
  at: string;
  /** Info | Warning | Error */
  level: string;
  code: string;
  args?: MessageArgs;
  /** Referência do item (objeto, finding…) a que o evento se refere. */
  itemRef?: string | null;
};

export type AiCall = {
  attempt?: number | null;
  /** Mechanical | Review | Analysis */
  usage: string;
  model: string;
  inputTokens: number;
  outputTokens: number;
  /** null = o serviço só informou tokens. */
  cost?: number | null;
};
