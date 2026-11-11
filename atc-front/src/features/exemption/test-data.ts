import type {
  ExemptionBatchResponse,
  ExemptionListingReport,
  ExemptionRequestResponse,
  SessionResponse,
} from "@/api/generated/atc/model";

/** Fábricas de DTO para os testes da isenção. */
export const quantities = {
  findings: 4,
  simulated: 0,
  requested: 0,
  info: 0,
  blocked: 0,
  noScope: 0,
  errors: 0,
  alreadyRequested: 0,
  pending: 4,
};

export const discoveryOk = {
  ok: true,
  policy: { text: "qualquer resultado", value: "A", display: "A – qualquer resultado", allowsLocal: true },
  reasons: [
    { id: "FPOS", title: "False Positive", justificationMandatory: false },
    { id: "OTHR", title: "Other Reason", justificationMandatory: true },
  ],
  validities: ["U = Unlimited", "D = Until date"],
  approvers: 1,
  approvalAreas: 0,
  adtApply: true,
  blockers: [],
  warnings: [],
  userExemptions: 0,
};

export function batch(o: Partial<ExemptionBatchResponse> = {}): ExemptionBatchResponse {
  return {
    id: "b1",
    sessionId: "11111111-aaaa-bbbb-cccc-222222222222",
    sapSystemId: "s1",
    priorities: "1,2,3",
    approver: null,
    reason: "FPOS",
    validity: "U",
    justificationPrefix: null,
    effectiveJustificationPrefix: "FPOS - POS - ATC - MIGNOW | ",
    discovery: null,
    status: "Created",
    requests: 4,
    totals: { PENDENTE: 4 },
    quantities,
    createdAt: "2026-11-08T10:00:00Z",
    createdBy: "dev@migatc.local",
    ...o,
  };
}

export function request(o: Partial<ExemptionRequestResponse> = {}): ExemptionRequestResponse {
  return {
    id: "q1",
    seq: 1,
    itemNumber: 1,
    objectName: "ZMIG_FI_ITEMS",
    objectType: "PROG",
    refObject: "BSEG",
    check: "S/4HANA Readiness",
    message: "BSEG is replaced by ACDOCA",
    markerId: "MK1",
    reason: "FPOS",
    justification: "FPOS - POS - ATC - MIGNOW | S/4 MIGRATION MIG-NOW 08.11.2026 AUT-DEV: ok",
    status: "SIMULADO",
    probablyNoScope: false,
    sapMessage: null,
    requestXml: null,
    responseXml: null,
    ...o,
  };
}

export function report(o: Partial<ExemptionListingReport> = {}): ExemptionListingReport {
  return {
    selectedItems: 4,
    findings: 3,
    priorities: ["1", "2", "3"],
    outOfScope: 0,
    withoutPriority: [],
    divergentGroups: [],
    byOrigin: { finding_id: 3, cruzamento: 0 },
    withoutLink: [{ itemNumber: 4, objectName: "ZMIG_MM_DOCS" }],
    alreadyExempt: [],
    alreadyRequested: [],
    probablyNoScope: [{ itemNumber: 2, objectName: "ZMIG_FI_ITEMS", markerId: "MK3", detail: "checksum -1" }],
    sample: [
      {
        seq: 1,
        itemNumber: 1,
        objectName: "ZMIG_SD_PRICING",
        include: "ZMIG_SD_PRICING",
        line: 3,
        refObject: "FROM",
        markerId: "MK1",
        origin: "finding_id",
      },
    ],
    ...o,
  };
}

export function session(o: Partial<SessionResponse> = {}): SessionResponse {
  return {
    id: "11111111-aaaa-bbbb-cccc-222222222222",
    customerId: "c1",
    customerName: "Cliente A",
    sapSystemId: "s1",
    sapSystemName: "DI4/400",
    source: "Extraction",
    sourceRef: "ATC-001",
    priorities: "1,2",
    request: null,
    author: "AUT-NT",
    projectType: "Conversion",
    cycle: 1,
    archived: false,
    archivedAt: null,
    autoMode: false,
    csvEncoding: "utf-8-sig",
    csvDelimiter: ";",
    columns: [],
    createdAt: "2026-11-08T09:00:00Z",
    counts: {
      kb: 0,
      ai: 0,
      exemption: 4,
      cancelled: 0,
      pending: 0,
      objects: 3,
      pendingObjects: 0,
      applyErrors: 0,
      awaitingApproval: 0,
      total: 4,
    },
    carriedOver: 0,
    ...o,
  } as SessionResponse;
}
