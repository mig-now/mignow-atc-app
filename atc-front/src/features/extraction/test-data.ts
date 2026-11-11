import type { AtcLatestResponse, ExtractionResponse, FindingResponse } from "@/api/generated/atc/model";

/** Fábricas de DTO para os testes da extração. */
export function latest(o: Partial<AtcLatestResponse> = {}): AtcLatestResponse {
  return {
    sapSystemId: "s1",
    displayId: "ATC-001",
    series: "ZMIG_SIM",
    title: "ATC S/4HANA Readiness",
    executedAtUtc: "2026-10-20T12:00:00Z",
    scheduledBy: "MIGATC",
    checkProfile: "SIM_VARIANT",
    configuration: "SIM",
    prio1: 1,
    prio2: 3,
    prio3: 9,
    prio4: 0,
    planningErrors: 0,
    inProgress: [],
    alreadyExtracted: false,
    extractionId: null,
    fileBaseName: "atc_DI4_400_ZMIG_SIM_ATC-001",
    ...o,
  };
}

export function extraction(o: Partial<ExtractionResponse> = {}): ExtractionResponse {
  return {
    id: "e1",
    sapSystemId: "s1",
    series: "ZMIG_SIM",
    displayId: "ATC-001",
    title: "ATC S/4HANA Readiness",
    executedAtUtc: "2026-10-20T12:00:00Z",
    extractedAt: "2026-11-08T10:00:00Z",
    totalCount: 13,
    inBaselineCount: 2,
    newCount: 11,
    total: { p1: 1, p2: 3, p3: 9 },
    new: { p1: 1, p2: 3, p3: 7 },
    variation: null,
    execution: null,
    situation: "Extracted",
    fileBaseName: "atc_DI4_400_ZMIG_SIM_ATC-001",
    ...o,
  };
}

export function finding(o: Partial<FindingResponse> = {}): FindingResponse {
  return {
    id: "f1",
    seq: 1,
    priority: 2,
    checkTitle: "S/4HANA Readiness",
    messageTitle: "BSEG is replaced by ACDOCA",
    objectName: "ZMIG_FI_ITEMS",
    objectType: "PROG",
    package: "ZMIG",
    refObjectName: "BSEG",
    refObjectType: "TABL",
    note: "0002000000",
    findingId: "MK1",
    inBaseline: false,
    allColumns: null,
    ...o,
  };
}
