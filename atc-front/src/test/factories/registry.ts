import type {
  AiProviderResponse,
  CustomerResponse,
  RunResponse,
  SapSystemResponse,
  SettingsResponse,
} from "@/api/generated/atc/model";

export function customer(o: Partial<CustomerResponse> = {}): CustomerResponse {
  return {
    id: "c1",
    name: "Cliente A",
    code: "A",
    active: true,
    sapSystemCount: 2,
    createdAt: "2026-10-01T00:00:00Z",
    ...o,
  };
}

export function sapSystem(o: Partial<SapSystemResponse> = {}): SapSystemResponse {
  return {
    id: "s1",
    customerId: "c1",
    customerName: "Cliente A",
    displayName: "Cliente A DI4/400",
    sid: "DI4",
    client: "400",
    host: "10.99.80.8",
    adtPort: 44332,
    https: true,
    language: "PT",
    certThumbprint: null,
    isProduction: false,
    projectType: "Conversion",
    author: "AUT-NT",
    defaultRequest: null,
    exemptionApprover: null,
    username: "CONSULTOR",
    hasPassword: true,
    active: true,
    isSimulator: false,
    release: null,
    clientCategory: null,
    lastConnectionTestAt: null,
    lastConnectionTestOk: null,
    lastConnectionTestMessage: null,
    ...o,
  };
}

export function aiProvider(o: Partial<AiProviderResponse> = {}): AiProviderResponse {
  return {
    id: "p1",
    name: "Claude Opus",
    kind: "Anthropic",
    model: "claude-opus-5-5",
    hasApiKey: true,
    isDefault: true,
    active: true,
    ...o,
  };
}

export function settings(o: Partial<SettingsResponse> = {}): SettingsResponse {
  return {
    parallelism: 4,
    sourceReadParallelism: 8,
    batchSize: 20,
    kbPath: "/kb",
    mignowRulesPath: "/regras.json",
    mignowParallel: 3,
    mignowWeightPerBatch: 500,
    manualObjectsPerDay: 10,
    hoursPerDay: 8,
    ...o,
  };
}

export function run(o: Partial<RunResponse> = {}): RunResponse {
  return {
    id: "r1",
    kind: "ConnectionTest",
    status: "Completed",
    customerId: "c1",
    customerName: "Cliente A",
    sapSystemId: "s1",
    sapSystemName: "Cliente A DI4/400",
    startedBy: "dev@migatc.local",
    queuedAt: "2026-11-06T10:00:00Z",
    startedAt: "2026-11-06T10:00:01Z",
    finishedAt: "2026-11-06T10:00:05Z",
    parallelism: 1,
    aiCalls: 0,
    aiInputTokens: 0,
    aiOutputTokens: 0,
    aiCost: 0,
    aiCallsWithoutCost: 0,
    steps: [{ code: "TEST_CONNECTION", order: 1, status: "Completed", progressDone: 0, progressTotal: 0 }],
    ...o,
  };
}
