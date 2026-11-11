import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { run } from "@/test/factories/registry";
import { API, ok, paged } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { RunMonitorPage } from "./RunMonitorPage";

vi.mock("@/api/run-hub", () => ({ useRunLiveUpdates: vi.fn() }));

const events = (items: unknown[] = []) => http.get(`${API}/v1/runs/:id/events`, () => ok(paged(items)));
const aiCalls = (items: unknown[] = []) => http.get(`${API}/v1/runs/:id/ai-calls`, () => ok(paged(items)));
const at = { route: "/execucoes/r1", path: "/execucoes/:runId" };

describe("Monitor da execução", () => {
  it("mostra cabeçalho, passos, log e chamadas de IA de uma execução concluída", async () => {
    server.use(
      http.get(`${API}/v1/runs/r1`, () =>
        ok(run({ aiCalls: 1, aiInputTokens: 1000, aiOutputTokens: 200, aiCost: 0.5 })),
      ),
      events([{ id: 1, at: "2026-11-06T10:00:01Z", level: "Info", code: "Run.Started", args: null }]),
      aiCalls([
        {
          id: 1,
          usage: "Review",
          model: "claude-sonnet-5-5",
          inputTokens: 1000,
          outputTokens: 200,
          cost: 0.5,
          ok: true,
          at: "2026-11-06T10:00:02Z",
        },
      ]),
    );
    const user = userEvent.setup();
    renderWithProviders(<RunMonitorPage />, at);
    expect(await screen.findByText("Cliente A · Cliente A DI4/400")).toBeInTheDocument();
    expect(screen.getByText("Concluída")).toBeInTheDocument();
    expect(screen.getByText("Testar conexão")).toBeInTheDocument(); // runs:steps.TEST_CONNECTION
    expect(screen.queryByRole("button", { name: "Cancelar" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Tentar de novo" })).not.toBeInTheDocument();
    await user.click(screen.getByRole("tab", { name: "Chamadas de IA" }));
    expect(await screen.findByText("claude-sonnet-5-5")).toBeInTheDocument();
  });

  it("execução ativa pode ser cancelada", async () => {
    let cancelled = false;
    server.use(
      http.get(`${API}/v1/runs/r1`, () =>
        ok(
          run({
            status: "Running",
            finishedAt: null,
            steps: [{ code: "TEST_CONNECTION", order: 1, status: "Processing", progressDone: 2, progressTotal: 5 }],
          }),
        ),
      ),
      events(),
      aiCalls(),
      http.post(`${API}/v1/runs/r1/cancel`, () => {
        cancelled = true;
        return ok(run({ status: "Cancelling" }));
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<RunMonitorPage />, at);
    expect(await screen.findByText("2/5")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Cancelar" }));
    await vi.waitFor(() => expect(cancelled).toBe(true));
  });

  it("falha mostra o erro e 'tentar de novo' leva ao run novo", async () => {
    server.use(
      http.get(`${API}/v1/runs/r1`, () => ok(run({ status: "Failed", failureCode: "Sap.Unreachable" }))),
      events(),
      aiCalls(),
      http.post(`${API}/v1/runs/r1/retry`, () => ok(run({ id: "r2", status: "Queued" }))),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<RunMonitorPage />, at);
    expect(await screen.findByText("A execução falhou")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Tentar de novo" }));
    await vi.waitFor(() => expect(router.state.location.pathname).toBe("/execucoes/r2"));
  });
});
