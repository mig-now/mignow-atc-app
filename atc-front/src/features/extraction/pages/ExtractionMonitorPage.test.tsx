import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import type { ExtractionMonitorRow } from "@/api/generated/atc/model";
import { customer, run } from "@/test/factories/registry";
import { API, ok, paged, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { extraction } from "../test-data";
import { ExtractionMonitorPage } from "./ExtractionMonitorPage";

const at = { route: "/extracao", path: "/extracao" };

function row(o: Partial<ExtractionMonitorRow>): ExtractionMonitorRow {
  return {
    sapSystemId: "s1",
    sapSystemName: "DI4/400",
    customerId: "c1",
    customerName: "Cliente A",
    isProduction: false,
    isSimulator: false,
    ...o,
  };
}

const rows = [
  row({ sapSystemId: "s1", sapSystemName: "DI4/400" }),
  row({
    sapSystemId: "s2",
    sapSystemName: "EC2/100",
    customerId: "c2",
    customerName: "Cliente B",
    lastRun: run({
      id: "r2",
      kind: "Extraction",
      status: "Completed",
      sapSystemId: "s2",
      result: { situation: "Extracted" },
    }),
    lastExtraction: extraction({ sapSystemId: "s2", displayId: "ATC-777", totalCount: 120, newCount: 30 }),
  }),
  row({
    sapSystemId: "s4",
    sapSystemName: "QAS/300",
    customerId: "c4",
    customerName: "Cliente D",
    lastRun: run({
      id: "r4",
      kind: "Extraction",
      status: "Failed",
      sapSystemId: "s4",
      failureCode: "Sap.Upstream",
      failureArgs: { message: "O SAP não respondeu como esperado (timeout)." },
    }),
  }),
  row({
    sapSystemId: "s3",
    sapSystemName: "MI9/500",
    customerId: "c3",
    customerName: "Cliente C",
    lastRun: run({
      id: "r3",
      kind: "Extraction",
      status: "Running",
      sapSystemId: "s3",
      sapSystemName: "MI9/500",
      finishedAt: undefined,
      steps: [
        { code: "EXTRACTION_FIND_LATEST", order: 1, status: "Completed", progressDone: 1, progressTotal: 1 },
        { code: "EXTRACTION_EXTRACT", order: 2, status: "Processing", progressDone: 40, progressTotal: 80 },
      ],
    }),
  }),
];

describe("Monitor de extrações", () => {
  it("lista os ambientes com status, andamento e última extração", async () => {
    server.use(
      http.get(`${API}/v1/customers`, () => ok(paged([customer()]))),
      http.get(`${API}/v1/extractions/monitor`, () => ok(rows)),
    );
    renderWithProviders(<ExtractionMonitorPage />, at);
    expect(await screen.findByText("DI4/400")).toBeInTheDocument();
    expect(screen.getByText("Nunca extraído")).toBeInTheDocument();
    expect(screen.getByText("O SAP não respondeu como esperado (timeout).")).toBeInTheDocument();
    expect(screen.getByText("ATC-777")).toBeInTheDocument();
    expect(screen.getByText("120 findings · 30 sem baseline")).toBeInTheDocument();
    expect(screen.getByText("Extrair findings")).toBeInTheDocument();
    expect(screen.getByRole("progressbar", { name: "Progresso de MI9/500" })).toHaveAttribute("aria-valuenow", "50");
    expect(screen.getByText("Em andamento: 1")).toBeInTheDocument();
    expect(screen.getByRole("checkbox", { name: "Selecionar Cliente C · MI9/500" })).toBeDisabled();
    expect(screen.getByRole("link", { name: "Nova extração" })).toHaveAttribute("href", "/extracao/nova");
    expect(screen.getByRole("link", { name: "Abrir a extração de DI4/400" })).toHaveAttribute(
      "href",
      "/extracao/nova?customerId=c1&systemId=s1",
    );
  });

  it("dispara vários ambientes de uma vez e mostra o erro só do que falhou", async () => {
    const started: unknown[] = [];
    server.use(
      http.get(`${API}/v1/customers`, () => ok(paged([customer()]))),
      http.get(`${API}/v1/extractions/monitor`, () => ok(rows)),
      http.post(`${API}/v1/extractions`, async ({ request }) => {
        const body = (await request.json()) as { sapSystemId: string };
        started.push(body);
        return body.sapSystemId === "s2"
          ? problem(409, "O ambiente já tem uma execução ativa.", "Run.EnvironmentBusy")
          : ok(run({ id: "r9", kind: "Extraction", status: "Queued" }), { status: 202 });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExtractionMonitorPage />, at);
    await user.click(await screen.findByRole("checkbox", { name: "Selecionar todos os ambientes livres" }));
    await user.click(screen.getByRole("button", { name: "Extrair selecionados (3)" }));
    expect(await screen.findByText("Disparadas: 2 · com erro: 1")).toBeInTheDocument();
    expect(started).toEqual(
      expect.arrayContaining([
        { sapSystemId: "s1", force: false },
        { sapSystemId: "s2", force: false },
      ]),
    );
    expect(started).toHaveLength(3);
    expect(screen.getByText("O ambiente já tem uma execução ativa.")).toBeInTheDocument();
  });
});
