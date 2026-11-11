import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { run } from "@/test/factories/registry";
import { API, ok, paged, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { ScanDetailPage } from "./ScanDetailPage";
import { ScanListPage } from "./ScanListPage";

vi.mock("@/api/run-hub", () => ({ useRunLiveUpdates: vi.fn() }));

const system = { id: "s1", customerName: "Cliente A", displayName: "SBX/100" };
const batch = (o: Record<string, unknown> = {}) => ({
  number: 1,
  weight: 100,
  objectCount: 40,
  className: "ZCL_MIG_SCAN_1",
  status: "OK",
  classRemoved: true,
  stats: { occurrences: 7 },
  ...o,
});
const scan = (o: Record<string, unknown> = {}) => ({
  id: "sc1",
  sapSystemId: "s1",
  sapSystemName: "Cliente A SBX/100",
  extractionId: "e1",
  scope: {
    escopo: 120,
    lotes: 2,
    peso_por_lote: 100,
    tadir_lidos: 300,
    por_tipo: { PROG: 80, CLAS: 40 },
    excluidos: { tmp: 1, marcados_exclusao: 2, gerados: 3 },
    incluir_gerados: false,
  },
  batches: [batch(), batch({ number: 2, classRemoved: false, className: "ZCL_MIG_SCAN_2" })],
  activeRuleIds: ["REGEX_001"],
  ruleCount: 2,
  summary: {
    ocorrencias: { total: 10, pendente: 6, ja_remediado: 3, informativo: 1 },
    cruzamento_pendentes: {
      ATC_PEGOU: 3,
      ATC_OUTRA_LINHA: 1,
      MIGNOW_FONTE_COM_FINDING: 1,
      MIGNOW_FONTE_SEM_FINDING: 1,
    },
    mignow: { ocorrencias: 2, objetos: 2 },
    csv_remediacao: { arquivo: "p1.csv", linhas: 2 },
  },
  createdAt: "2026-11-06T10:00:00Z",
  createdBy: "dev",
  ...o,
});

const common = () => [
  http.get(`${API}/v1/runs`, () => ok(paged([]))),
  http.get(`${API}/v1/scans/rules`, () =>
    ok({
      version: "1.0",
      date: "2026-10-27",
      trigger: "",
      rules: [
        {
          id: "REGEX_001",
          active: true,
          priority: "1",
          category: "TABELA",
          severity: "ALTO",
          description: "Acesso a VBUK",
          originalPcre: "",
          pcre: "",
          text: "C",
          fixedElement: "",
          requires: "",
          suppressedBy: "",
          robt: "TABL",
        },
        {
          id: "REGEX_002",
          active: true,
          priority: "1",
          category: "TABELA",
          severity: "MEDIO",
          description: "Acesso a KONV",
          originalPcre: "",
          pcre: "",
          text: "C",
          fixedElement: "",
          requires: "",
          suppressedBy: "",
          robt: "TABL",
        },
      ],
    }),
  ),
  http.get(`${API}/v1/scans/sc1/occurrences`, () =>
    ok(
      paged([
        {
          id: "o1",
          ruleId: "REGEX_001",
          objectType: "PROG",
          objectName: "ZPROG1",
          include: null,
          adtSource: null,
          line: 10,
          lineEnd: null,
          element: "VBUK",
          snippet: "SELECT * FROM vbuk.",
          status: "pendente",
          cross: "MIGNOW_FONTE_SEM_FINDING",
          batch: 1,
        },
      ]),
    ),
  ),
];

describe("Varredura MIGNOW", () => {
  it("lista as varreduras do ambiente, avisa que é opcional e cria uma nova com a extração mais recente", async () => {
    let body: unknown = null;
    server.use(
      http.get(`${API}/v1/sap-systems`, () => ok(paged([system]))),
      http.get(`${API}/v1/scans`, () => ok(paged([scan()]))),
      http.get(`${API}/v1/extractions`, () =>
        ok(
          paged([
            { id: "e-old", displayId: "ATC-1", extractedAt: "2026-10-01T00:00:00Z", executedAtUtc: null },
            { id: "e-new", displayId: "ATC-2", extractedAt: "2026-11-01T00:00:00Z", executedAtUtc: null },
          ]),
        ),
      ),
      http.post(`${API}/v1/scans`, async ({ request }) => {
        body = await request.json();
        return ok({ scan: scan({ id: "novo" }), run: run({ id: "r9", kind: "ScanInventory" }) }, { status: 202 });
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<ScanListPage />, { route: "/varredura", path: "/varredura" });
    expect(await screen.findByText("Etapa opcional, só extrai")).toBeInTheDocument();
    expect(await screen.findByRole("link", { name: /06\/11\/2026|11\/6\/26|2026/ })).toHaveAttribute(
      "href",
      "/varredura/sc1",
    );
    await vi.waitFor(() => expect(screen.getByLabelText("Extração base do ATC")).toHaveValue("e-new"));
    await user.click(screen.getByRole("button", { name: "Criar varredura" }));
    await vi.waitFor(() => expect(router.state.location.pathname).toBe("/varredura/novo"));
    expect(body).toEqual({ sapSystemId: "s1", extractionId: "e-new", includeGenerated: false });
    expect(router.state.location.search).toBe("?run=r9");
  });

  it("detalhe: escopo, alerta de classe não removida com limpeza, cruzamento e ocorrências", async () => {
    let cleaned = false;
    server.use(
      ...common(),
      http.get(`${API}/v1/scans/sc1`, () => ok(scan())),
      http.post(`${API}/v1/scans/sc1/cleanup`, () => {
        cleaned = true;
        return ok(run({ id: "r-clean", kind: "ScanCleanup" }), { status: 202 });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<ScanDetailPage />, { route: "/varredura/sc1", path: "/varredura/:scanId" });
    expect(await screen.findByText("1 classe(s) temporária(s) não removida(s) do SAP")).toBeInTheDocument();
    expect(screen.getByText(/ZCL_MIG_SCAN_2/, { selector: "p" })).toBeInTheDocument();
    expect(screen.getByText("Objetos no escopo").nextSibling).toHaveTextContent("120");
    expect(screen.getByRole("list", { name: "Pendentes por classe de cruzamento" })).toBeInTheDocument();
    expect(await screen.findByText("SELECT * FROM vbuk.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Limpar resíduos" }));
    await vi.waitFor(() => expect(cleaned).toBe(true));
  });

  it("salva a seleção de regras em PUT scope", async () => {
    let ids: string[] = [];
    server.use(
      ...common(),
      http.get(`${API}/v1/scans/sc1`, () => ok(scan())),
      http.put(`${API}/v1/scans/sc1/scope`, async ({ request }) => {
        ids = ((await request.json()) as { ruleIds: string[] }).ruleIds;
        return ok(scan());
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<ScanDetailPage />, { route: "/varredura/sc1", path: "/varredura/:scanId" });
    await user.click(await screen.findByRole("checkbox", { name: "Usar a regra REGEX_002" }));
    await user.click(screen.getByRole("button", { name: "Salvar escopo" }));
    await vi.waitFor(() => expect(ids.sort()).toEqual(["REGEX_001", "REGEX_002"]));
  });

  it("importar na remediação exige confirmação e navega para a sessão", async () => {
    let imported = false;
    server.use(
      ...common(),
      http.get(`${API}/v1/scans/sc1`, () => ok(scan({ batches: [batch()] }))),
      http.post(`${API}/v1/scans/sc1/import`, () => {
        imported = true;
        return ok({ id: "sess-9" });
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<ScanDetailPage />, { route: "/varredura/sc1", path: "/varredura/:scanId" });
    await user.click(await screen.findByRole("button", { name: "Importar na remediação" }));
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText(/será arquivada/)).toBeInTheDocument();
    expect(imported).toBe(false);
    await user.click(within(dialog).getByRole("button", { name: "Arquivar a sessão atual e importar" }));
    await vi.waitFor(() => expect(router.state.location.pathname).toBe("/remediacao/sess-9"));
  });

  it("mostra o conflito de ambiente (409) ao simular", async () => {
    server.use(
      ...common(),
      http.get(`${API}/v1/scans/sc1`, () => ok(scan({ batches: [batch()] }))),
      http.post(`${API}/v1/scans/sc1/simulate`, () =>
        problem(409, "Há uma execução em andamento neste ambiente.", "Run.EnvironmentBusy"),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<ScanDetailPage />, { route: "/varredura/sc1", path: "/varredura/:scanId" });
    await user.click(await screen.findByRole("button", { name: "Simular" }));
    expect(await screen.findByText("Há uma execução em andamento neste ambiente.")).toBeInTheDocument();
  });
});
