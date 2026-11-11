import { http } from "msw";
import type { ItemResponse, ProposalResponse, RemediationGroup, SessionResponse } from "@/api/generated/atc/model";
import { run, sapSystem } from "@/test/factories/registry";
import { API, ok, paged } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";

export const SESSION_ID = "ses1";

export function session(o: Partial<SessionResponse> = {}): SessionResponse {
  return {
    id: SESSION_ID,
    customerId: "c1",
    customerName: "Cliente A",
    sapSystemId: "s1",
    sapSystemName: "Cliente A DI4/400",
    source: "Extraction",
    sourceRef: "ATC-1",
    priorities: "1,2",
    request: "DI4K900001",
    author: "AUT-NT",
    projectType: "Conversion",
    cycle: 1,
    archived: false,
    autoMode: false,
    csvEncoding: "utf-8",
    csvDelimiter: ";",
    columns: [],
    createdAt: "2026-11-06T10:00:00Z",
    counts: {
      kb: 0,
      ai: 0,
      exemption: 0,
      cancelled: 0,
      pending: 1,
      objects: 1,
      pendingObjects: 1,
      applyErrors: 0,
      awaitingApproval: 0,
      total: 1,
    },
    carriedOver: 0,
    ...o,
  } as SessionResponse;
}

export function item(o: Partial<ItemResponse> = {}): ItemResponse {
  return {
    id: "i1",
    n: 1,
    objectName: "ZMIG_FI_ITEMS",
    objectType: "PROG",
    refObject: "BSEG",
    refType: "TABL",
    check: "S/4HANA Readiness",
    message: "BSEG",
    origin: "ATC",
    status: "",
    obs: "",
    triageFlags: [],
    hasProposal: true,
    hasPreparation: true,
    awaitingApproval: false,
    ...o,
  };
}

export const group: RemediationGroup = {
  key: "C|BSEG|TABL|SQL",
  ref: "BSEG",
  refType: "TABL",
  callForm: "SQL",
  items: [1],
  objects: ["PROG ZMIG_FI_ITEMS"],
  kbTop: { id: "S02753", occurrences: 1, alerts: [] },
  kbSolutions: 5,
  complexity: "simples",
  standard: false,
  suggestedModel: "haiku",
};

export function proposal(o: Partial<ProposalResponse> = {}): ProposalResponse {
  return {
    itemId: "i1",
    n: 1,
    status: "",
    proposal: {
      origin: "KB",
      kbCardId: "S02753",
      blocks: [],
      markedBlock: "*>>> S/4 MIGRATION MIG-NOW - START OF INSERTION",
      alerts: ["Confirmar o campo DMBTR"],
      syntaxOk: true,
      syntaxMessages: [],
      model: "haiku",
      tokens: 321,
      justification: "Campo substituído pelo da ACDOCA",
      action: "corrigir",
    },
    alerts: ["Confirmar o campo DMBTR"],
    syntaxOk: true,
    preparation: { source: "DATA x.\nSELECT * FROM bseg.\n", newSource: "DATA x.\nSELECT * FROM acdoca.\n" },
    ...o,
  } as ProposalResponse;
}

type Opts = { session?: SessionResponse; items?: ItemResponse[]; production?: boolean; proposal?: ProposalResponse };

/** Handlers comuns da aba; cada teste acrescenta os POSTs que quer observar. */
export function useApplyApi(o: Opts = {}) {
  const finished = run({ id: "r1", kind: "Propose", status: "Completed" });
  server.use(
    http.get(`${API}/v1/sessions/${SESSION_ID}`, () => ok(o.session ?? session())),
    http.get(`${API}/v1/sap-systems/s1`, () => ok(sapSystem({ isProduction: o.production ?? false }))),
    http.get(`${API}/v1/sessions/${SESSION_ID}/items`, () => ok(paged(o.items ?? [item()]))),
    http.get(`${API}/v1/sessions/${SESSION_ID}/groups`, () =>
      ok({ pending: 1, batch: 40, objectsInBatch: 1, itemsInBatch: 1, groups: [group] }),
    ),
    http.get(`${API}/v1/sessions/${SESSION_ID}/items/:itemId/proposal`, () => ok(o.proposal ?? proposal())),
    http.get(`${API}/v1/runs/:id`, () => ok(finished)),
    http.get(`${API}/v1/runs/:id/events`, () => ok(paged([]))),
    http.get(`${API}/v1/runs/:id/ai-calls`, () => ok(paged([]))),
  );
}
