import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { customer, run, sapSystem } from "@/test/factories/registry";
import { API, ok, paged, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { extraction, finding, latest } from "../test-data";
import { ExtractionPage } from "./ExtractionPage";

vi.mock("@/api/run-hub", () => ({ useRunLiveUpdates: vi.fn() }));

const at = { route: "/extracao/nova?customerId=c1&systemId=s1", path: "/extracao/nova" };

const base = () => [
  http.get(`${API}/v1/customers`, () => ok(paged([customer()]))),
  http.get(`${API}/v1/sap-systems`, () => ok(paged([sapSystem({ defaultRequest: "DI4K900001" })]))),
  http.get(`${API}/v1/runs/:id/events`, () => ok(paged([]))),
  http.get(`${API}/v1/runs/:id/ai-calls`, () => ok(paged([]))),
];

describe("Extração", () => {
  it("sem ambiente escolhido pede a seleção", async () => {
    server.use(...base());
    renderWithProviders(<ExtractionPage />, { route: "/extracao/nova", path: "/extracao/nova" });
    expect(
      await screen.findByText("Escolha o cliente e o ambiente para ver a última execução do ATC."),
    ).toBeInTheDocument();
  });

  it("mostra a última execução, extrai e acompanha o run inline", async () => {
    let started: unknown;
    let runStatus = "Running";
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems/s1/atc/latest`, () =>
        ok(latest({ inProgress: [{ displayId: "ATC-002", series: "ZMIG_SIM" }] })),
      ),
      http.get(`${API}/v1/extractions`, () => ok(paged([]))),
      http.post(`${API}/v1/extractions`, async ({ request }) => {
        started = await request.json();
        return ok(run({ id: "r9", kind: "Extraction", status: "Queued" }), { status: 202 });
      }),
      http.get(`${API}/v1/runs/r9`, () =>
        ok(
          run({
            id: "r9",
            kind: "Extraction",
            status: runStatus as "Running",
            result: { situation: "Extracted", totalCount: 13, newCount: 11 },
          }),
        ),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExtractionPage />, at);
    expect(await screen.findByText("ATC-001")).toBeInTheDocument();
    expect(screen.getByText("ZMIG_SIM")).toBeInTheDocument();
    expect(screen.getByText("P3: 9")).toBeInTheDocument();
    expect(screen.getByText("Ainda não extraída")).toBeInTheDocument();
    expect(screen.getByText("Execuções do ATC em andamento: 1")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Extrair" }));
    await vi.waitFor(() => expect(started).toEqual({ sapSystemId: "s1", force: false }));
    expect(await screen.findByText("Extração do ATC")).toBeInTheDocument(); // RunMonitorPanel
    runStatus = "Completed";
  });

  it("execução já extraída pede confirmação para forçar", async () => {
    let body: unknown;
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems/s1/atc/latest`, () => ok(latest({ alreadyExtracted: true, extractionId: "e1" }))),
      http.get(`${API}/v1/extractions`, () => ok(paged([extraction()]))),
      http.post(`${API}/v1/extractions`, async ({ request }) => {
        body = await request.json();
        return ok(run({ id: "r9", kind: "Extraction" }), { status: 202 });
      }),
      http.get(`${API}/v1/runs/r9`, () => ok(run({ id: "r9", kind: "Extraction" }))),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExtractionPage />, at);
    await user.click(await screen.findByRole("button", { name: "Extrair de novo (forçar)" }));
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText(/já foi extraída/)).toBeInTheDocument();
    expect(body).toBeUndefined();
    await user.click(within(dialog).getByRole("button", { name: "Forçar extração" }));
    await vi.waitFor(() => expect(body).toEqual({ sapSystemId: "s1", force: true }));
  });

  it("409 do ambiente ocupado mostra o run que bloqueia com link", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems/s1/atc/latest`, () => ok(latest())),
      http.get(`${API}/v1/extractions`, () => ok(paged([]))),
      http.post(`${API}/v1/extractions`, () =>
        problem(409, "Já existe uma execução em andamento neste ambiente.", "Run.EnvironmentBusy"),
      ),
      http.get(`${API}/v1/runs`, () =>
        ok(paged([run({ id: "rx", kind: "ExtractionCheck", status: "Running", startedBy: "ana@x.com" })])),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExtractionPage />, at);
    await user.click(await screen.findByRole("button", { name: "Extrair" }));
    expect(await screen.findByText("Já existe uma execução em andamento neste ambiente.")).toBeInTheDocument();
    const link = await screen.findByRole("link", { name: /Ver a execução que bloqueia: Verificação da extração/ });
    expect(link).toHaveAttribute("href", "/execucoes/rx");
  });

  it("histórico: colunas, filtro de findings e criação da sessão de remediação", async () => {
    let sessionBody: unknown;
    let findingsUrl = "";
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems/s1/atc/latest`, () => ok(latest({ alreadyExtracted: true }))),
      http.get(`${API}/v1/extractions`, () => ok(paged([extraction({ variation: 2 })]))),
      http.get(`${API}/v1/extractions/e1/findings`, ({ request }) => {
        findingsUrl = request.url;
        return ok(paged([finding()]));
      }),
      http.post(`${API}/v1/sessions`, async ({ request }) => {
        sessionBody = await request.json();
        return ok({ id: "ses1" });
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<ExtractionPage />, at);
    const row = (await screen.findByRole("button", { name: "Findings de ATC-001" })).closest("tr") as HTMLElement;
    expect(within(row).getByText("+2")).toBeInTheDocument();
    expect(within(row).getAllByText("13")[0]).toBeInTheDocument();

    await user.click(within(row).getByRole("button", { name: "Findings de ATC-001" }));
    const findings = await screen.findByRole("dialog");
    expect(await within(findings).findByText("ZMIG_FI_ITEMS")).toBeInTheDocument();
    await user.selectOptions(within(findings).getByLabelText("Prioridade"), "2");
    await vi.waitFor(() => expect(findingsUrl).toContain("priority=2"));
    await user.click(within(findings).getByRole("button", { name: "Fechar" }));

    await user.click(within(row).getByRole("button", { name: "Remediação de ATC-001" }));
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByLabelText("Request de transporte")).toHaveValue("DI4K900001");
    await user.selectOptions(within(dialog).getByLabelText("Prioridades"), "1,2");
    await user.click(within(dialog).getByRole("button", { name: "Criar sessão de remediação" }));
    await vi.waitFor(() =>
      expect(sessionBody).toEqual({ extractionId: "e1", priorities: "1,2", request: "DI4K900001" }),
    );
    await vi.waitFor(() => expect(router.state.location.pathname).toBe("/remediacao/ses1"));
  });

  it("baixa o CSV de remediação com o filtro de prioridades", async () => {
    let url = "";
    URL.createObjectURL = vi.fn(() => "blob:x");
    URL.revokeObjectURL = vi.fn();
    vi.spyOn(HTMLAnchorElement.prototype, "click").mockImplementation(() => {});
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems/s1/atc/latest`, () => ok(latest({ alreadyExtracted: true }))),
      http.get(`${API}/v1/extractions`, () => ok(paged([extraction()]))),
      http.get(`${API}/v1/extractions/e1/files/remediacao`, ({ request }) => {
        url = request.url;
        return new Response("a;b", { headers: { "content-type": "text/csv" } });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExtractionPage />, at);
    await user.click(await screen.findByRole("button", { name: "Remediação de ATC-001" }));
    await user.click(
      within(await screen.findByRole("dialog")).getByRole("button", { name: "Baixar CSV de remediação" }),
    );
    await vi.waitFor(() => expect(url).toContain("priorities=1"));
    await vi.waitFor(() => expect(URL.createObjectURL).toHaveBeenCalled());
  });
});
