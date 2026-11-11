import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import type { ItemProposal } from "@/api/generated/atc/model";
import { run } from "@/test/factories/registry";
import { API, ok, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { item, proposal, SESSION_ID, session, useApplyApi } from "./fixtures";
import { SessionApplyTab } from "./SessionApplyTab";

vi.mock("@/api/run-hub", () => ({ useRunLiveUpdates: vi.fn() }));

const base = `${API}/v1/sessions/${SESSION_ID}`;
const renderTab = () => renderWithProviders(<SessionApplyTab sessionId={SESSION_ID} />);
const openStep = (user: ReturnType<typeof userEvent.setup>, name: string) =>
  user.click(screen.getByRole("tab", { name }));

describe("Aba Propor, revisar e aplicar", () => {
  it("propõe por grupo no modo KB e dispara o run Propose", async () => {
    useApplyApi();
    let body: unknown;
    server.use(
      http.post(`${base}/propose`, async ({ request }) => {
        body = await request.json();
        return ok(run({ id: "r1", kind: "Propose", status: "Completed" }), { status: 202 });
      }),
    );
    const user = userEvent.setup();
    renderTab();
    await user.click(await screen.findByRole("checkbox", { name: /^BSEG \(TABL\)/ }));
    expect(screen.getByRole("button", { name: "Propor" })).toBeEnabled();
    await user.click(screen.getByRole("button", { name: "Propor" }));
    await waitFor(() =>
      expect(body).toEqual({ groupKeys: ["C|BSEG|TABL|SQL"], itemIds: null, mode: "kb", autoConfirm: false }),
    );
    expect(await screen.findByText("Proposta de correção")).toBeInTheDocument(); // runs:kind.Propose
  });

  it("modo automático mostra o aviso das condições de parada e o relatório do lote em Markdown", async () => {
    useApplyApi({ session: session({ autoMode: true }) });
    let body: { autoConfirm?: boolean } | undefined;
    server.use(
      http.post(`${base}/propose`, async ({ request }) => {
        body = (await request.json()) as { autoConfirm?: boolean };
        return ok(run({ id: "r1", kind: "Propose" }), { status: 202 });
      }),
      http.get(`${API}/v1/runs/r1`, () =>
        ok(
          run({ id: "r1", kind: "Propose", result: { reportMarkdown: "# Relatório do lote X\n\n- item 1 aplicado" } }),
        ),
      ),
    );
    const user = userEvent.setup();
    renderTab();
    await user.click(await screen.findByRole("checkbox", { name: /^BSEG \(TABL\)/ }));
    await user.click(screen.getByRole("checkbox", { name: /Modo automático/ }));
    expect(screen.getByText(/cópia Z de objeto standard/)).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Propor" }));
    expect(await screen.findByRole("heading", { name: "Relatório do lote X" })).toBeInTheDocument();
    expect(body?.autoConfirm).toBe(true);
  });

  it("409 ao propor mostra a mensagem do back", async () => {
    useApplyApi();
    server.use(
      http.post(`${base}/propose`, () =>
        problem(409, "O ambiente está ocupado por outra execução.", "Run.EnvironmentBusy"),
      ),
    );
    const user = userEvent.setup();
    renderTab();
    await user.click(await screen.findByRole("checkbox", { name: /^BSEG \(TABL\)/ }));
    await user.click(screen.getByRole("button", { name: "Propor" }));
    expect(await screen.findByText("O ambiente está ocupado por outra execução.")).toBeInTheDocument();
  });

  it("revisão: mostra a proposta da KB com diff lado a lado e confirma KB", async () => {
    useApplyApi();
    let body: unknown;
    server.use(
      http.post(`${base}/proposals/decision`, async ({ request }) => {
        body = await request.json();
        return ok({ items: [{ itemId: "i1", n: 1, status: "Confirmado KB", obs: "" }], kbCardId: null, factIds: [] });
      }),
    );
    const user = userEvent.setup();
    renderTab();
    await openStep(user, "Revisar e decidir");
    await user.click(await screen.findByRole("button", { name: "Abrir proposta do item 1" }));
    const art = await screen.findByRole("article", { name: "Proposta do item 1" });
    expect(within(art).getByText("Antes")).toBeInTheDocument();
    expect(within(art).getByText("Depois")).toBeInTheDocument();
    expect(within(art).getByText("SELECT * FROM bseg.")).toBeInTheDocument();
    expect(within(art).getByText("SELECT * FROM acdoca.")).toBeInTheDocument();
    expect(within(art).getByText("Confirmar o campo DMBTR")).toBeInTheDocument(); // alerta da KB
    expect(within(art).getByRole("link", { name: "S02753" })).toHaveAttribute("href", "/kb/tabelas/BSEG");
    expect(within(art).getByText("Sem erros")).toBeInTheDocument();
    await user.click(within(art).getByRole("button", { name: "Confirmar KB" }));
    await waitFor(() => expect(body).toEqual({ itemIds: ["i1"], decision: "ConfirmKb" }));
  });

  it("revisão: confirmar IA abre o diálogo e envia observação e fatos candidatos", async () => {
    useApplyApi({
      proposal: proposal({ proposal: { ...(proposal().proposal as ItemProposal), origin: "IA", kbCardId: null } }),
    });
    let body: unknown;
    server.use(
      http.post(`${base}/proposals/decision`, async ({ request }) => {
        body = await request.json();
        return ok({ items: [], kbCardId: "C-BSEG-001", factIds: [] });
      }),
    );
    const user = userEvent.setup();
    renderTab();
    await openStep(user, "Revisar e decidir");
    await user.click(await screen.findByRole("button", { name: "Abrir proposta do item 1" }));
    await user.click(await screen.findByRole("button", { name: "Confirmar IA" }));
    const dialog = await screen.findByRole("dialog");
    await user.type(within(dialog).getByLabelText("Observação"), "ok pela IA");
    await user.click(within(dialog).getByRole("button", { name: "Confirmar IA" }));
    await waitFor(() => expect(body).toMatchObject({ itemIds: ["i1"], decision: "ConfirmIa", obs: "ok pela IA" }));
  });

  it("simular, confirmar explicitamente e aplicar", async () => {
    useApplyApi({
      items: [item({ status: "Confirmado KB" })],
      proposal: proposal({
        status: "Confirmado KB",
        review: "**Veredito:** aprovar\n\nSem riscos.",
        reviewVerdict: "aprovar",
        application: { result: "SIMULADO", backupSource: "DATA x.", history: [] },
      }),
    });
    const bodies: Record<string, unknown> = {};
    server.use(
      http.post(`${base}/apply-simulate`, async ({ request }) => {
        bodies.simulate = await request.json();
        return ok(run({ id: "r1", kind: "ApplySimulate" }), { status: 202 });
      }),
      http.post(`${base}/apply`, async ({ request }) => {
        bodies.apply = await request.json();
        return ok(run({ id: "r1", kind: "Apply" }), { status: 202 });
      }),
    );
    const user = userEvent.setup();
    renderTab();
    await openStep(user, "Simular e aplicar");
    const apply = await screen.findByRole("button", { name: "Aplicar no SAP (0)" });
    expect(apply).toBeDisabled();
    await user.click(await screen.findByRole("checkbox", { name: /ZMIG_FI_ITEMS/ }));
    expect(screen.getByRole("button", { name: "Aplicar no SAP (1)" })).toBeDisabled(); // falta simular
    await user.click(screen.getByRole("button", { name: "Simular (1)" }));
    await waitFor(() =>
      expect(bodies.simulate).toEqual({ itemIds: ["i1"], allowTableStructure: false, ddicCreate: [] }),
    );
    expect(await screen.findByText("Backup feito")).toBeInTheDocument();
    expect(screen.getAllByText("aprovar").length).toBeGreaterThan(0);
    const applyBtn = await screen.findByRole("button", { name: "Aplicar no SAP (1)" });
    await waitFor(() => expect(applyBtn).toBeEnabled());
    await user.click(applyBtn);
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText("Vai gravar no SAP Cliente A DI4/400, na request DI4K900001.")).toBeInTheDocument();
    expect(within(dialog).getByText(/ZMIG_FI_ITEMS/)).toBeInTheDocument();
    const confirm = within(dialog).getByRole("button", { name: "Gravar no SAP" });
    expect(confirm).toBeDisabled();
    await user.click(within(dialog).getByRole("checkbox"));
    await user.click(confirm);
    await waitFor(() => expect(bodies.apply).toEqual({ itemIds: ["i1"], allowTableStructure: false, ddicCreate: [] }));
  });

  it("erro de ativação: escolher Rollback chama a decisão e mostra o run de rollback", async () => {
    useApplyApi({
      items: [item({ status: "Confirmado KB", applicationResult: "ERRO_ATIVACAO" })],
      proposal: proposal({
        status: "Confirmado KB",
        application: {
          result: "ERRO_ATIVACAO",
          messages: ["Erro de sintaxe na linha 10"],
          backupSource: "DATA x.",
          history: [],
        },
      }),
    });
    let body: unknown;
    server.use(
      http.post(`${base}/items/i1/activation-decision`, async ({ request }) => {
        body = await request.json();
        return ok({ decision: "rollback", run: run({ id: "r2", kind: "Rollback" }) }, { status: 202 });
      }),
      http.get(`${API}/v1/runs/r2`, () => ok(run({ id: "r2", kind: "Rollback" }))),
      http.get(`${API}/v1/runs/r2/events`, () => ok({ items: [], page: 1, pageSize: 50, total: 0 })),
      http.get(`${API}/v1/runs/r2/ai-calls`, () => ok({ items: [], page: 1, pageSize: 50, total: 0 })),
    );
    const user = userEvent.setup();
    renderTab();
    await openStep(user, "Simular e aplicar");
    expect(await screen.findByText("Erro de sintaxe na linha 10")).toBeInTheDocument();
    expect(screen.getAllByText("Erro de ativação").length).toBeGreaterThan(0);
    await user.click(screen.getByRole("button", { name: "Rollback" }));
    await waitFor(() => expect(body).toEqual({ decision: "rollback" }));
    expect(await screen.findByRole("heading", { name: "Rollback" })).toBeInTheDocument(); // card do run (runs:kind.Rollback)
  });

  it("ambiente de produção: gravação bloqueada", async () => {
    useApplyApi({ production: true, items: [item({ status: "Confirmado KB" })] });
    let simulated = false;
    server.use(
      http.post(`${base}/apply-simulate`, () => {
        simulated = true;
        return ok(run({ id: "r1", kind: "ApplySimulate" }), { status: 202 });
      }),
    );
    const user = userEvent.setup();
    renderTab();
    await openStep(user, "Simular e aplicar");
    expect(await screen.findByText("Este ambiente é de produção: a gravação está bloqueada.")).toBeInTheDocument();
    await user.click(await screen.findByRole("checkbox", { name: /ZMIG_FI_ITEMS/ }));
    await user.click(screen.getByRole("button", { name: "Simular (1)" }));
    await waitFor(() => expect(simulated).toBe(true));
    expect(await screen.findByRole("button", { name: "Aplicar no SAP (1)" })).toBeDisabled();
  });
});
