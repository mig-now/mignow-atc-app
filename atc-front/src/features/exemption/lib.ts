import type { ExemptionBatchResponse, RunResponse } from "@/api/generated/atc/model";

/** Resultado da descoberta (`batch.discovery`, camelCase), lido defensivamente porque o contrato é `unknown`. */
export type Discovery = {
  ok?: boolean;
  policy?: { display?: string; value?: string; text?: string; allowsLocal?: boolean };
  reasons?: { id: string; title?: string; justificationMandatory?: boolean }[];
  approvers?: number;
  approvalAreas?: number;
  validities?: string[];
  blockers?: unknown[];
  warnings?: unknown[];
  adtApply?: boolean;
  userExemptions?: number;
  model?: { hasModel?: boolean; http?: number; message?: string };
};

export function asDiscovery(v: unknown): Discovery | null {
  return v && typeof v === "object" ? (v as Discovery) : null;
}

/** Política diferente de `A` (qualquer resultado) bloqueia o lote: o ADT só aceita pedido com `A`. */
export function isBlocked(d: Discovery | null): boolean {
  if (!d) return false;
  return d.ok === false || (d.policy?.value !== undefined && d.policy.value !== "A");
}

/** Texto de um bloqueio/aviso (string ou objeto com `message`). */
export function noteText(x: unknown): string {
  if (typeof x === "string") return x;
  if (x && typeof x === "object" && "message" in x && typeof (x as { message: unknown }).message === "string")
    return (x as { message: string }).message;
  return JSON.stringify(x);
}

/** "U = Unlimited" -> { value: "U", label: "U = Unlimited" }. */
export function parseValidity(raw: string) {
  const value = raw.split("=")[0]?.trim() || raw;
  return { value, label: raw };
}

export type RunHistory = { totals?: Record<string, number>; policy?: string; elapsedSeconds?: number };
export type RunResult = {
  history?: RunHistory;
  exampleJustification?: string;
  messages?: string[];
  situations?: {
    seq: number;
    type?: string;
    markerId?: string;
    itemNumber?: number;
    objectName?: string;
    sapMessage?: string;
    situationCode?: string;
  }[];
};

export function resultOf(run: RunResponse | undefined): RunResult | null {
  return run?.result && typeof run.result === "object" ? (run.result as RunResult) : null;
}

export const BATCH_RUN_KINDS = ["ExemptionDiscover", "ExemptionSimulate", "ExemptionSend", "ExemptionCheck"] as const;

/** Etapa recomendada pelo estado do lote (1 Descoberta … 5 Conferir). */
export function recommendedStep(batch: ExemptionBatchResponse): number {
  const d = asDiscovery(batch.discovery);
  if (!d || isBlocked(d)) return 1;
  if (!batch.approver) return 2;
  switch (batch.status) {
    case "Created":
    case "Discovered":
      return 3;
    case "Simulated":
      return 4;
    default:
      return 5;
  }
}

/** Etapa 2+ só depois da descoberta sem bloqueio; 3 exige parâmetros (aprovador); 4 exige simulação; 5 exige envio. */
export function maxStep(batch: ExemptionBatchResponse): number {
  const d = asDiscovery(batch.discovery);
  if (!d || isBlocked(d)) return 1;
  if (!batch.approver) return 2;
  if (batch.status === "Created" || batch.status === "Discovered") return 3;
  if (batch.status === "Simulated") return 4;
  return 5;
}

export const REQUEST_STATUSES = [
  "PENDENTE",
  "SIMULADO",
  "BLOQUEADO",
  "SEM_ESCOPO",
  "JA_PEDIDO",
  "PEDIDO",
  "INFO",
  "ERRO",
] as const;

export const STATUS_TONE: Record<string, "neutral" | "info" | "success" | "warning" | "danger"> = {
  PENDENTE: "neutral",
  SIMULADO: "info",
  BLOQUEADO: "warning",
  SEM_ESCOPO: "warning",
  JA_PEDIDO: "neutral",
  PEDIDO: "success",
  INFO: "info",
  ERRO: "danger",
};
