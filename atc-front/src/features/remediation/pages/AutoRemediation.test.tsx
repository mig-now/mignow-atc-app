import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import type { RemediationMonitorRow, RunStatus } from "@/api/generated/atc/model";
import { customer, run } from "@/test/factories/registry";
import { API, ok, paged } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { item, session } from "../testData";
import { RemediationMonitorPage } from "./RemediationMonitorPage";
import { RemediationSessionPage } from "./RemediationSessionPage";

vi.mock("@/api/run-hub", () => ({ useRunLiveUpdates: vi.fn() }));
vi.mock("../apply", () => ({ SessionApplyTab: ({ sessionId }: { sessionId: string }) => <p>apply {sessionId}</p> }));

const summary = {
  items: 10,
  kb: 4,
  ai: 2,
  exemption: 1,
  cancelled: 1,
  pending: 2,
  activated: 6,
  applyErrors: 0,
  standard: 0,
};

describe("Remediação: monitor", () => {
  it("lista as sessões com a execução em andamento, o resumo do play e o link para a sessão", async () => {
    const rows: RemediationMonitorRow[] = [
      {
        session: session({ sapSystemName: "DI4/400" }),
        lastRun: run({
          id: "ra",
          kind: "AutoRemediation",
          status: "Running",
          sapSystemName: "DI4/400",
          finishedAt: undefined,
          steps: [
            { code: "AUTO_SOURCES_PLAN", order: 1, status: "Completed", progressDone: 1, progressTotal: 1 },
            { code: "READ_SOURCES_FETCH", order: 2, status: "Completed", progressDone: 8, progressTotal: 8 },
            { code: "PROPOSE", order: 3, status: "Processing", progressDone: 3, progressTotal: 12 },
          ],
        }),
      },
      {
        session: session({ id: "se2", sapSystemName: "EC2/100", customerName: "Cliente B" }),
        lastRun: run({ id: "rb", kind: "AutoRemediation", status: "Completed", result: { summary } }),
      },
    ];
    server.use(
      http.get(`${API}/v1/customers`, () => ok(paged([customer()]))),
      http.get(`${API}/v1/sessions/monitor`, () => ok(rows)),
    );
    renderWithProviders(<RemediationMonitorPage />, { route: "/remediacao", path: "/remediacao" });
    expect(await screen.findByText("Cliente B")).toBeInTheDocument();
    expect(screen.getByText("Etapa 3 de 3 · Propor correções")).toBeInTheDocument();
    expect(screen.getByRole("progressbar", { name: "Progresso de DI4/400" })).toHaveAttribute("aria-valuenow", "25");
    expect(screen.getByText("6 ativados no SAP · 2 pendentes")).toBeInTheDocument();
    expect(screen.getByText("Em andamento: 1")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Abrir a sessão de EC2/100" })).toHaveAttribute(
      "href",
      "/remediacao/se2?auto=rb",
    );
    expect(screen.getByRole("link", { name: "Nova remediação" })).toHaveAttribute("href", "/remediacao/nova");
    expect(screen.getByRole("button", { name: /Exportar o CSV de DI4\/400/ })).toBeDisabled();
  });
});

const triageRules = [
  {
    code: "R1",
    description: "sem código + Ambiguous → Cancelado",
    effect: "Cancelado",
    scope: "All",
    builtIn: true,
    active: true,
    refObjects: [],
    usesRefObjects: false,
  },
  {
    code: "R3",
    description: "uso não localizado → só sinaliza",
    effect: "signal",
    scope: "All",
    builtIn: true,
    active: true,
    refObjects: [],
    usesRefObjects: false,
  },
  {
    code: "R7",
    description: "objeto referenciado BSEG → Isenção",
    effect: "Isenção",
    scope: "All",
    builtIn: true,
    active: true,
    refObjects: [],
    usesRefObjects: false,
  },
];

describe("Remediação: play na sessão", () => {
  it("inicia a remediação dos itens marcados só depois da confirmação e acompanha até o fim", {
    timeout: 15000,
  }, async () => {
    const bodies: unknown[] = [];
    const ruleQueries: (string | null)[] = [];
    let status: RunStatus = "Running";
    server.use(
      http.get(`${API}/v1/sessions/se1`, () => ok(session())),
      http.get(`${API}/v1/sessions/se1/items`, () =>
        ok(paged([item(), item({ id: "i2", n: 2, objectName: "YMIG_LEGACY" })])),
      ),
      http.get(`${API}/v1/triage-rules`, ({ request }) => {
        ruleQueries.push(new URL(request.url).searchParams.get("projectType"));
        return ok(triageRules);
      }),
      http.post(`${API}/v1/sessions/se1/auto-remediation`, async ({ request }) => {
        bodies.push(await request.json());
        return ok(run({ id: "rp", kind: "AutoRemediation", status: "Queued" }), { status: 202 });
      }),
      http.get(`${API}/v1/runs/rp`, () =>
        ok(
          run({
            id: "rp",
            kind: "AutoRemediation",
            status,
            result: status === "Completed" ? { summary, reportMarkdown: "# Lote" } : null,
          }),
        ),
      ),
      http.get(`${API}/v1/runs/rp/events`, () => ok(paged([]))),
      http.get(`${API}/v1/runs/rp/ai-calls`, () => ok(paged([]))),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<RemediationSessionPage />, {
      route: "/remediacao/se1",
      path: "/remediacao/:sessionId",
    });
    await user.click(await screen.findByRole("checkbox", { name: "Selecionar item 1" }));
    await user.click(screen.getByRole("button", { name: "Iniciar (1)" }));
    const dialog = await screen.findByRole("dialog");
    const start = within(dialog).getByRole("button", { name: "Iniciar" });
    expect(start).toBeDisabled();
    // regras de triagem do tipo de projeto da sessão: só exibição, sem como desmarcar; o back aplica as mesmas
    const rules = await within(dialog).findByRole("region", { name: "Regras de triagem aplicadas" });
    expect(within(rules).getByText("Regras de triagem aplicadas (projeto de Conversão)")).toBeInTheDocument();
    for (const code of ["R1", "R7", "R3"]) expect(within(rules).getByText(code)).toBeInTheDocument();
    expect(within(rules).queryByRole("checkbox")).not.toBeInTheDocument();
    expect(ruleQueries).toContain("Conversion");
    await user.click(within(dialog).getByRole("checkbox", { name: /Confirmo a gravação no SAP/ }));
    await user.click(start);
    await vi.waitFor(() => expect(bodies).toEqual([{ itemIds: [item().id] }]));
    await vi.waitFor(() => expect(router.state.location.search).toContain("auto=rp"));
    expect(await screen.findByRole("heading", { name: "Remediação automática" })).toBeInTheDocument();

    status = "Completed"; // o painel continua consultando o run enquanto ativo (polling)
    expect(
      await screen.findByText("Concluída: 10 itens, 6 ativados no SAP.", {}, { timeout: 6000 }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Exportar CSV (atc_status.csv)" })).toBeEnabled();
    await user.click(screen.getByRole("button", { name: "Ver histórico" }));
    expect(router.state.location.search).toContain("tab=history");
  });
});
