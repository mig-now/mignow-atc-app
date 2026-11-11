import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { run, sapSystem } from "@/test/factories/registry";
import { API, ok, paged, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { batch, discoveryOk, request } from "../test-data";
import { ExemptionBatchPage } from "./ExemptionBatchPage";

vi.mock("@/api/run-hub", () => ({ useRunLiveUpdates: vi.fn() }));

const at = (query = "") => ({ route: `/isencao/b1${query}`, path: "/isencao/:batchId" });

function mount(
  b: ReturnType<typeof batch>,
  opts: { runs?: unknown[]; system?: Partial<Parameters<typeof sapSystem>[0]> } = {},
) {
  server.use(
    http.get(`${API}/v1/exemption-batches/b1`, () => ok(b)),
    http.get(`${API}/v1/sap-systems/s1`, () => ok(sapSystem({ username: "CONSULTOR", ...opts.system }))),
    http.get(`${API}/v1/runs`, () => ok(paged(opts.runs ?? []))),
    http.get(`${API}/v1/runs/:id/events`, () => ok(paged([]))),
    http.get(`${API}/v1/runs/:id/ai-calls`, () => ok(paged([]))),
  );
}

const simulatedBatch = (over: Partial<ReturnType<typeof batch>> = {}) =>
  batch({
    discovery: discoveryOk,
    approver: "APROV1",
    status: "Simulated",
    totals: { SIMULADO: 3, BLOQUEADO: 1 },
    ...over,
  });

const requests = [
  request({ id: "q1", seq: 1 }),
  request({ id: "q2", seq: 2, objectName: "ZMIG_MM_DOCS" }),
  request({ id: "q3", seq: 3, objectName: "ZMIG_SD_PRICING" }),
  request({ id: "q4", seq: 4, status: "BLOQUEADO", sapMessage: "Isenção já existe" }),
];

describe("Isenção: lote (assistente)", () => {
  it("política diferente de A bloqueia as etapas seguintes e mostra a mensagem", async () => {
    mount(
      batch({
        status: "Discovered",
        discovery: {
          ...discoveryOk,
          ok: false,
          policy: { value: "C", display: "C – somente centrais" },
          blockers: ["Política de isenção não permite pedido local"],
        },
      }),
    );
    renderWithProviders(<ExemptionBatchPage />, at());
    expect(await screen.findByText("Lote bloqueado pela política de isenção")).toBeInTheDocument();
    expect(screen.getByText(/C – somente centrais/, { selector: "p" })).toBeInTheDocument();
    expect(screen.getByText("Política de isenção não permite pedido local")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /2 · Parâmetros/ })).toBeDisabled();
    expect(screen.getByRole("button", { name: /4 · Enviar/ })).toBeDisabled();
  });

  it("dispara a descoberta e acompanha o run inline", async () => {
    let called = false;
    mount(batch());
    server.use(
      http.post(`${API}/v1/exemption-batches/b1/discover`, () => {
        called = true;
        return ok(run({ id: "r5", kind: "ExemptionDiscover", status: "Running" }), { status: 202 });
      }),
      http.get(`${API}/v1/runs/r5`, () => ok(run({ id: "r5", kind: "ExemptionDiscover", status: "Running" }))),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExemptionBatchPage />, at());
    await user.click(await screen.findByRole("button", { name: "Executar descoberta" }));
    await vi.waitFor(() => expect(called).toBe(true));
    expect(await screen.findByText("Descoberta de isenção")).toBeInTheDocument();
  });

  it("parâmetros: recusa aprovador igual ao usuário SAP e salva o padrão FPOS", async () => {
    let put: unknown;
    mount(batch({ status: "Discovered", discovery: discoveryOk }));
    server.use(
      http.put(`${API}/v1/exemption-batches/b1/params`, async ({ request }) => {
        put = await request.json();
        return ok(batch({ status: "Discovered", discovery: discoveryOk, approver: "APROV1" }));
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<ExemptionBatchPage />, at("?step=2"));
    const approver = await screen.findByLabelText("Aprovador");
    expect(screen.getByLabelText("Motivo")).toHaveValue("FPOS");
    await user.type(approver, "consultor");
    await user.click(screen.getByRole("button", { name: "Salvar parâmetros" }));
    expect(await screen.findByText(/não pode ser o próprio usuário SAP/)).toBeInTheDocument();
    expect(put).toBeUndefined();
    await user.clear(approver);
    await user.type(approver, "APROV1");
    await user.click(screen.getByRole("button", { name: "Salvar parâmetros" }));
    await vi.waitFor(() =>
      expect(put).toEqual({ approver: "APROV1", reason: "FPOS", validity: "U", justificationPrefix: null }),
    );
    await vi.waitFor(() => expect(router.state.location.search).toContain("step=3"));
  });

  it("simulação: tabela por status com filtro", async () => {
    const seen: string[] = [];
    mount(simulatedBatch());
    server.use(
      http.get(`${API}/v1/exemption-batches/b1/requests`, ({ request: r }) => {
        seen.push(r.url);
        return ok(paged(requests));
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExemptionBatchPage />, at("?step=3"));
    expect(await screen.findByText("Isenção já existe")).toBeInTheDocument();
    expect(screen.getAllByText("Simulado", { selector: "span" }).length).toBeGreaterThan(0);
    expect(screen.getByText("Bloqueado", { selector: "td span" })).toBeInTheDocument();
    await user.selectOptions(screen.getByLabelText("Filtrar por status"), "BLOQUEADO");
    await vi.waitFor(() => expect(seen.some((u) => u.includes("status=BLOQUEADO"))).toBe(true));
  });

  it("envio exige confirmação explícita com a contagem e só então grava", async () => {
    const sent: unknown[] = [];
    mount(simulatedBatch());
    server.use(
      http.get(`${API}/v1/exemption-batches/b1/requests`, () => ok(paged(requests))),
      http.post(`${API}/v1/exemption-batches/b1/send`, async ({ request: r }) => {
        sent.push(await r.json());
        return ok(run({ id: "r7", kind: "ExemptionSend", status: "Running" }), { status: 202 });
      }),
      http.get(`${API}/v1/runs/r7`, () => ok(run({ id: "r7", kind: "ExemptionSend", status: "Running" }))),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExemptionBatchPage />, at("?step=4"));
    await user.click(await screen.findByRole("button", { name: "Enviar o lote ao SAP (3)" }));
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText("Criar 3 pedido(s) de isenção no SAP?")).toBeInTheDocument();
    expect(within(dialog).getByText(/Serão criados 3 pedido\(s\).*APROV1/)).toBeInTheDocument();
    expect(within(dialog).getByText("Isto grava no SAP")).toBeInTheDocument();
    await user.click(within(dialog).getByRole("button", { name: "Cancelar" }));
    expect(sent).toHaveLength(0); // cancelar não envia
    await user.click(screen.getByRole("button", { name: "Enviar o lote ao SAP (3)" }));
    await user.click(
      within(await screen.findByRole("dialog")).getByRole("button", { name: "Criar 3 pedido(s) no SAP" }),
    );
    await vi.waitFor(() => expect(sent).toEqual([{}]));
    expect(await screen.findByText("Envio de isenção")).toBeInTheDocument();
  });

  it("envio por seq: só os marcados", async () => {
    const sent: unknown[] = [];
    mount(simulatedBatch());
    server.use(
      http.get(`${API}/v1/exemption-batches/b1/requests`, () => ok(paged(requests))),
      http.post(`${API}/v1/exemption-batches/b1/send`, async ({ request: r }) => {
        sent.push(await r.json());
        return ok(run({ id: "r7", kind: "ExemptionSend", status: "Running" }), { status: 202 });
      }),
      http.get(`${API}/v1/runs/r7`, () => ok(run({ id: "r7", kind: "ExemptionSend", status: "Running" }))),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExemptionBatchPage />, at("?step=4"));
    expect(await screen.findByRole("checkbox", { name: "Selecionar o pedido 4" })).toBeDisabled(); // bloqueado
    expect(screen.getByRole("button", { name: "Enviar só os marcados (0)" })).toBeDisabled();
    await user.click(screen.getByRole("checkbox", { name: "Selecionar o pedido 2" }));
    await user.click(screen.getByRole("checkbox", { name: "Selecionar o pedido 3" }));
    await user.click(screen.getByRole("button", { name: "Enviar só os marcados (2)" }));
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText("Criar 2 pedido(s) de isenção no SAP?")).toBeInTheDocument();
    await user.click(within(dialog).getByRole("button", { name: "Criar 2 pedido(s) no SAP" }));
    await vi.waitFor(() => expect(sent).toEqual([{ seq: [2, 3] }]));
  });

  it("409 no envio mostra o run que bloqueia com link; produção desabilita o envio", async () => {
    mount(simulatedBatch(), {
      runs: [run({ id: "rx", kind: "ExtractionCheck", status: "Running", startedBy: "ana@x.com", subjectId: "outro" })],
    });
    server.use(
      http.get(`${API}/v1/exemption-batches/b1/requests`, () => ok(paged(requests))),
      http.post(`${API}/v1/exemption-batches/b1/send`, () =>
        problem(409, "Já existe uma execução em andamento neste ambiente.", "Run.EnvironmentBusy"),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExemptionBatchPage />, at("?step=4"));
    await user.click(await screen.findByRole("button", { name: "Enviar o lote ao SAP (3)" }));
    await user.click(
      within(await screen.findByRole("dialog")).getByRole("button", { name: "Criar 3 pedido(s) no SAP" }),
    );
    expect(await screen.findByText("Já existe uma execução em andamento neste ambiente.")).toBeInTheDocument();
    expect(
      await screen.findByRole("link", { name: /Ver a execução que bloqueia: Verificação da extração/ }),
    ).toHaveAttribute("href", "/execucoes/rx");
  });

  it("em produção o envio fica desabilitado", async () => {
    mount(simulatedBatch(), { system: { isProduction: true } });
    server.use(http.get(`${API}/v1/exemption-batches/b1/requests`, () => ok(paged(requests))));
    renderWithProviders(<ExemptionBatchPage />, at("?step=4"));
    expect(await screen.findByText("Ambiente de produção")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Enviar o lote ao SAP (3)" })).toBeDisabled();
  });

  it("conferência: mostra a situação de cada finding e o histórico do lote", async () => {
    mount(batch({ discovery: discoveryOk, approver: "APROV1", status: "Checked", totals: { PEDIDO: 2 } }), {
      runs: [
        run({
          id: "r9",
          kind: "ExemptionCheck",
          subjectId: "b1",
          result: {
            history: { totals: { PEDIDO: 2 } },
            situations: [
              {
                seq: 1,
                markerId: "MK1",
                itemNumber: 1,
                objectName: "ZMIG_FI_ITEMS",
                sapMessage: "Exemption already requested",
              },
            ],
          },
        }),
      ],
    });
    server.use(http.get(`${API}/v1/runs/r9`, () => ok(run({ id: "r9", kind: "ExemptionCheck" }))));
    renderWithProviders(<ExemptionBatchPage />, at());
    expect(await screen.findByText("Exemption already requested")).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Histórico do lote" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /\d{2}\/\d{2}\/\d{4}/ })).toHaveAttribute("href", "/execucoes/r9");
  });
});
