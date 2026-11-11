import type { AiCall, RunEvent, RunStep } from "@/features/shared/types";

/** Factories de objetos locais simples (props dos componentes shared). Adicione as dos tipos gerados nos módulos. */
export function runStep(overrides: Partial<RunStep> = {}): RunStep {
  return { code: "READ_SOURCES", order: 1, status: "Pending", messageCode: null, messageArgs: null, ...overrides };
}

export function runEvent(overrides: Partial<RunEvent> = {}): RunEvent {
  return { id: 1, at: "2026-11-06T10:00:00Z", level: "Info", code: "Run.Started", args: null, ...overrides };
}

export function aiCall(overrides: Partial<AiCall> = {}): AiCall {
  return {
    usage: "Review",
    model: "claude-sonnet-5-5",
    inputTokens: 1000,
    outputTokens: 500,
    cost: 0.01,
    ...overrides,
  };
}
