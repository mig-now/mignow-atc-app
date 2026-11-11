import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { run } from "@/test/factories/registry";
import { API, ok, paged } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { counts, groups, history, item, itemDetail, session } from "../testData";
import { RemediationSessionPage } from "./RemediationSessionPage";

vi.mock("@/api/run-hub", () => ({ useRunLiveUpdates: vi.fn() }));
vi.mock("../apply", () => ({ SessionApplyTab: ({ sessionId }: { sessionId: string }) => <p>apply {sessionId}</p> }));

const at = (search = "") => ({ route: `/remediacao/se1${search}`, path: "/remediacao/:sessionId" });
const base = () => [
  http.get(`${API}/v1/sessions/se1`, () => ok(session())),
  http.get(`${API}/v1/sessions/se1/items`, () =>
    ok(paged([item(), item({ id: "i2", n: 2, objectName: "YMIG_LEGACY", status: "Isenção", triageFlags: ["R7"] })])),
  ),
];

describe("Remediação: sessão", () => {
  it("mostra o cabeçalho com contagens e a tabela de itens", async () => {
    server.use(...base());
    renderWithProviders(<RemediationSessionPage />, at());
    expect(await screen.findByText("ZMIG_FI_ITEMS")).toBeInTheDocument();
    expect(screen.getByText("Conversão")).toBeInTheDocument();
    expect(screen.getByText("P1 + P2")).toBeInTheDocument();
    expect(screen.getByText("AUT-NT")).toBeInTheDocument();
    expect(screen.getByRole("img", { name: /Pendente: 2/ })).toBeInTheDocument();
    expect(screen.getByRole("cell", { name: "R7" })).toBeInTheDocument();
    for (const tab of [
      "Itens",
      "Fontes",
      "Triagem",
      "Grupos e decisões",
      "Propostas e aplicação",
      "Exportar",
      "Histórico",
    ])
      expect(screen.getByRole("tab", { name: tab })).toBeInTheDocument();
  });

  it("confirmado com falha na gravação mostra o erro do SAP na lista, não só o status", async () => {
    const queries: string[] = [];
    server.use(
      http.get(`${API}/v1/sessions/se1`, () => ok(session({ counts: counts({ kb: 3, applyErrors: 2 }) }))),
      http.get(`${API}/v1/sessions/se1/items`, ({ request }) => {
        queries.push(new URL(request.url).search);
        return ok(
          paged([
            item({
              status: "Confirmado KB",
              applicationResult: "ERRO_REQUEST",
              applicationDetail: "objeto ja esta na request MI9K902668 (sessao usa MI9K902708)",
            }),
            item({
              id: "i2",
              n: 2,
              status: "Confirmado KB",
              applicationResult: "ERRO",
              applicationDetail: "Sap.WriteBlockedProductionClient",
            }),
            item({ id: "i3", n: 3, status: "Confirmado KB", applicationResult: "ATIVADO", applicationDetail: null }),
            item({ id: "i4", n: 4 }),
          ]),
        );
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<RemediationSessionPage />, at());
    expect(await screen.findByText("Erro de request")).toBeInTheDocument();
    expect(screen.getByText("objeto ja esta na request MI9K902668 (sessao usa MI9K902708)")).toBeInTheDocument();
    expect(screen.getByText("Escrita bloqueada: mandante de produção (T000-CCCATEGORY = P).")).toBeInTheDocument();
    expect(screen.getByText("Ativado")).toBeInTheDocument();
    expect(screen.getByText("Não aplicado")).toBeInTheDocument();
    // contagem na barra da sessão e filtro próprio
    expect(screen.getByText("Erro no SAP (confirmado, não gravado)")).toBeInTheDocument();
    await user.selectOptions(screen.getByLabelText("Filtrar por situação"), "applyError");
    await vi.waitFor(() => expect(queries.at(-1)).toContain("status=applyError"));
  });

  it("proposta esperando o consultor aparece como pendente de aprovação, com a sugestão, filtro e contagem próprios", async () => {
    const queries: string[] = [];
    const bodies: unknown[] = [];
    server.use(
      http.get(`${API}/v1/sessions/se1`, () => ok(session({ counts: counts({ pending: 2, awaitingApproval: 1 }) }))),
      http.get(`${API}/v1/sessions/se1/items`, ({ request }) => {
        queries.push(new URL(request.url).search);
        return ok(
          paged([
            item({
              awaitingApproval: true,
              proposalAction: "isencao",
              proposalNote: "O código já usa a API do S/4: isentar o finding.",
            }),
            item({ id: "i2", n: 2 }),
          ]),
        );
      }),
      http.post(`${API}/v1/sessions/se1/items/decision`, async ({ request }) => {
        bodies.push(await request.json());
        return ok({ items: [item({ status: "Isenção" })], pending: 1, skipped: 0 });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<RemediationSessionPage />, at());
    expect((await screen.findAllByText("Pendente de aprovação")).length).toBeGreaterThan(0);
    expect(await screen.findByText("Sugestão: isenção")).toBeInTheDocument();
    // a justificativa fica no detalhe do item, não na lista
    expect(screen.queryByText("O código já usa a API do S/4: isentar o finding.")).not.toBeInTheDocument();
    expect(screen.getByRole("img", { name: /Pendente: 1, .*Pendente de aprovação: 1/ })).toBeInTheDocument();

    // aceitar a sugestão é a decisão padrão; sem observação, o back usa a justificativa da proposta
    await user.click(screen.getByRole("checkbox", { name: "Selecionar item 1" }));
    await user.click(screen.getByRole("button", { name: /Decidir/ }));
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByLabelText("Decisão")).toHaveValue("AcceptSuggestion");
    // o diálogo mostra a sugestão e a justificativa de cada item escolhido
    expect(within(dialog).getByText("Sugestão: isenção")).toBeInTheDocument();
    expect(within(dialog).getByText("O código já usa a API do S/4: isentar o finding.")).toBeInTheDocument();
    await user.click(within(dialog).getByRole("button", { name: "Aplicar decisão" }));
    await vi.waitFor(() => expect(bodies).toEqual([{ itemIds: ["i1"], decision: "AcceptSuggestion", obs: null }]));

    await user.selectOptions(screen.getByLabelText("Filtrar por situação"), "approval");
    await vi.waitFor(() => expect(queries.at(-1)).toContain("status=approval"));
    await user.selectOptions(screen.getByLabelText("Filtrar por situação"), "unprocessed");
    await vi.waitFor(() => expect(queries.at(-1)).toContain("status=unprocessed"));
  });

  it("sem pendentes, o play continua disponível para reprocessar os itens com erro no SAP", async () => {
    server.use(
      http.get(`${API}/v1/sessions/se1`, () => ok(session({ counts: counts({ pending: 0, kb: 3, applyErrors: 2 }) }))),
      http.get(`${API}/v1/sessions/se1/items`, () =>
        ok(paged([item({ status: "Confirmado KB", applicationResult: "ERRO_REQUEST" })])),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<RemediationSessionPage />, at());
    const play = await screen.findByRole("button", { name: "Iniciar" });
    expect(play).toBeEnabled();
    // sem item marcado: confirma antes que vale para todos os pendentes e com erro
    await user.click(play);
    const confirm = await screen.findByRole("dialog", { name: "Processar todos os itens?" });
    expect(
      within(confirm).getByText("Nenhum item marcado: serão processados os 0 itens pendentes e os 2 com erro no SAP."),
    ).toBeInTheDocument();
    await user.click(within(confirm).getByRole("button", { name: "Cancelar" }));
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    await user.click(play);
    await user.click(
      within(await screen.findByRole("dialog", { name: "Processar todos os itens?" })).getByRole("button", {
        name: "Processar todos",
      }),
    );
    expect(
      await screen.findByText("Os 0 itens pendentes e os 2 com erro no SAP passam pelas etapas sem parada."),
    ).toBeInTheDocument();
  });

  it("filtros dos itens vão para a URL e para a API", async () => {
    const queries: string[] = [];
    server.use(
      http.get(`${API}/v1/sessions/se1`, () => ok(session())),
      http.get(`${API}/v1/sessions/se1/items`, ({ request }) => {
        queries.push(new URL(request.url).search);
        return ok(paged([item()]));
      }),
      // o filtro por regra lista os códigos do cadastro de triagem
      http.get(`${API}/v1/triage-rules`, () =>
        ok([
          { code: "R7", key: "R7" },
          { code: "ZR9", key: "R9" },
        ]),
      ),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<RemediationSessionPage />, at());
    await screen.findByText("ZMIG_FI_ITEMS");
    expect(await screen.findByRole("option", { name: "ZR9" })).toBeInTheDocument();
    await user.selectOptions(screen.getByLabelText("Filtrar por situação"), "exemption");
    await user.selectOptions(screen.getByLabelText("Filtrar por regra de triagem"), "R7");
    await vi.waitFor(() => expect(queries.at(-1)).toContain("triage=R7"));
    expect(queries.at(-1)).toContain("status=exemption");
    expect(router.state.location.search).toContain("status=exemption");
    expect(router.state.location.search).toContain("triage=R7");
  });

  it("abre o diálogo do item com ficha, histórico e proposta", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/sessions/se1/items/i1`, () =>
        ok(
          itemDetail({
            proposal: {
              origin: "KB",
              kbCardId: "S02753",
              blocks: [],
              alerts: ["conferir"],
              syntaxOk: true,
              action: "corrigir",
            },
          }),
        ),
      ),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<RemediationSessionPage />, at());
    await user.click(await screen.findByRole("button", { name: "Abrir item 1" }));
    const dialog = await screen.findByRole("dialog");
    expect(await within(dialog).findByText("Trocar BSEG por ACDOCA.")).toBeInTheDocument();
    expect(within(dialog).getByText("S02753")).toBeInTheDocument();
    expect(within(dialog).getByText("Ainda não aplicado.")).toBeInTheDocument();
    expect(router.state.location.search).toContain("item=i1");
  });

  it("decide itens marcados: isenção exige observação", async () => {
    const bodies: unknown[] = [];
    server.use(
      ...base(),
      http.post(`${API}/v1/sessions/se1/items/decision`, async ({ request }) => {
        bodies.push(await request.json());
        return ok({ items: [item()], pending: 1 });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<RemediationSessionPage />, at());
    await user.click(await screen.findByRole("checkbox", { name: "Selecionar item 1" }));
    await user.click(screen.getByRole("button", { name: "Decidir (1)" }));
    const dialog = await screen.findByRole("dialog");
    await user.selectOptions(within(dialog).getByLabelText("Decisão"), "Exemption");
    await user.click(within(dialog).getByRole("button", { name: "Aplicar decisão" }));
    expect(await within(dialog).findByText("Informe a observação da isenção.")).toBeInTheDocument();
    expect(bodies).toHaveLength(0);
    await user.type(within(dialog).getByLabelText("Observação (obrigatória)"), "BAPI já corrigida");
    await user.click(within(dialog).getByRole("button", { name: "Aplicar decisão" }));
    await vi.waitFor(() => expect(bodies).toHaveLength(1));
    expect(bodies[0]).toEqual({ itemIds: ["i1"], decision: "Exemption", obs: "BAPI já corrigida" });
    expect(await screen.findByText("1 itens atualizados; 1 pendentes.")).toBeInTheDocument();
  });

  it("sessão recém-criada acompanha a leitura automática dos fontes, ou diz por que não começou", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/runs/r9`, () => ok(run({ id: "r9", kind: "ReadSources" }))),
      http.get(`${API}/v1/runs/r9/events`, () => ok(paged([]))),
      http.get(`${API}/v1/runs/r9/ai-calls`, () => ok(paged([]))),
    );
    const user = userEvent.setup();
    const { router, unmount } = renderWithProviders(<RemediationSessionPage />, at("?sources=r9"));
    expect(await screen.findByRole("heading", { name: "Leitura dos fontes" })).toBeInTheDocument();
    expect(await screen.findByText("Leitura de fontes")).toBeInTheDocument(); // monitor do run
    await user.click(screen.getByRole("button", { name: "Fechar o painel da leitura dos fontes" }));
    expect(router.state.location.search).not.toContain("sources=");
    unmount();

    renderWithProviders(<RemediationSessionPage />, at("?sourcesNotStarted=Run.EnvironmentBusy"));
    expect(await screen.findByText("A leitura dos fontes não começou")).toBeInTheDocument();
    expect(screen.getByText("O play lê os fontes que faltarem antes de propor as correções.")).toBeInTheDocument();
  });

  it("fontes: lista os objetos com fonte lido, sem disparo manual, e abre o fonte num popup", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/sessions/se1/sources`, () =>
        ok({
          objects: 3,
          read: 1,
          items: [
            {
              objectType: "PROG",
              objectName: "ZMIG_FI_ITEMS",
              itemCount: 2,
              pending: 1,
              includes: [{ id: "c1", includeName: "ZMIG_FI_ITEMS_F01", lines: 2, readAt: "2026-11-11T10:00:00Z" }],
            },
          ],
        }),
      ),
      http.get(`${API}/v1/sessions/se1/sources/c1`, () =>
        ok({
          id: "c1",
          objectName: "ZMIG_FI_ITEMS",
          includeName: "ZMIG_FI_ITEMS_F01",
          adtUri: "/sap/bc/adt/programs/includes/zmig_fi_items_f01",
          readAt: "2026-11-11T10:00:00Z",
          source: "SELECT * FROM konv INTO TABLE lt.\nWRITE lt.\n",
        }),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<RemediationSessionPage />, at("?tab=sources"));
    expect(await screen.findByText(/1 de 3 objetos com fonte lido/)).toBeInTheDocument();
    expect(screen.getByText("2 (1 pendentes)")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Ler fontes/ })).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Ver o fonte ZMIG_FI_ITEMS_F01" }));
    const dialog = await screen.findByRole("dialog", { name: "ZMIG_FI_ITEMS_F01" });
    expect(await within(dialog).findByText("SELECT * FROM konv INTO TABLE lt.")).toBeInTheDocument();
    expect(within(dialog).getByText("WRITE lt.")).toBeInTheDocument();
    expect(within(dialog).getByText("2")).toBeInTheDocument(); // número da linha
  });

  it("triagem: só exibe as regras do tipo de projeto da sessão, sem aplicar nem conferir R1", async () => {
    const queries: (string | null)[] = [];
    server.use(
      ...base(),
      http.get(`${API}/v1/triage-rules`, ({ request }) => {
        queries.push(new URL(request.url).searchParams.get("projectType"));
        return ok([
          {
            key: "R1",
            code: "R1",
            description: "sem código + Ambiguous → Cancelado",
            effect: "Cancelado",
            scope: "All",
            order: 6,
            condition: "O tipo do objeto está em: TABL (sem código ABAP) e a classificação do finding é AMBIGUOUS.",
            builtIn: true,
            active: true,
            refObjects: [],
            parameters: {},
            usesRefObjects: false,
          },
          {
            key: "R7",
            code: "R7",
            description: "objeto referenciado BSEG → Isenção",
            effect: "Isenção",
            scope: "All",
            order: 3,
            condition: "O objeto referenciado está em: BSEG.",
            builtIn: true,
            active: true,
            refObjects: ["BSEG"],
            parameters: {},
            usesRefObjects: true,
          },
        ]);
      }),
    );
    renderWithProviders(<RemediationSessionPage />, at("?tab=triage"));
    expect(
      await screen.findByText(/Regras que a triagem considera nesta sessão \(projeto de Conversão\)/),
    ).toBeInTheDocument();
    expect(await screen.findByText("O objeto referenciado está em: BSEG.")).toBeInTheDocument();
    const rows = screen.getAllByRole("row").slice(1);
    expect(within(rows[0] as HTMLElement).getByText("R7")).toBeInTheDocument(); // ordem de avaliação
    expect(queries).toEqual(["Conversion"]);
    expect(screen.queryByRole("checkbox")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Aplicar/ })).not.toBeInTheDocument();
    expect(screen.queryByText(/Conferir R1/)).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: /cadastro/ })).not.toBeInTheDocument();
  });

  it("grupos: decide por grupo resolvendo os ids dos itens", async () => {
    const bodies: unknown[] = [];
    server.use(
      http.get(`${API}/v1/sessions/se1/groups`, () => ok(groups())),
      http.get(`${API}/v1/sessions/se1/items`, ({ request }) => {
        const url = new URL(request.url);
        if (url.searchParams.get("group") !== "BSEG") return ok(paged([]));
        return ok(paged([item({ id: "a", n: 2 }), item({ id: "b", n: 9 }), item({ id: "c", n: 30 })]));
      }),
      http.post(`${API}/v1/sessions/se1/items/decision`, async ({ request }) => {
        bodies.push(await request.json());
        return ok({ items: [item(), item()], pending: 0 });
      }),
      ...base(),
    );
    const user = userEvent.setup();
    renderWithProviders(<RemediationSessionPage />, at("?tab=groups"));
    expect(await screen.findByText("S02753")).toBeInTheDocument();
    expect(screen.getByText("5 soluções")).toBeInTheDocument();
    expect(screen.queryByText("haiku")).not.toBeInTheDocument();
    await user.click(screen.getByRole("checkbox", { name: "Marcar grupo BSEG" }));
    await user.click(screen.getByRole("button", { name: "Decidir grupos (1)" }));
    const dialog = await screen.findByRole("dialog");
    await user.selectOptions(within(dialog).getByLabelText("Decisão"), "Cancel");
    await user.click(within(dialog).getByRole("button", { name: "Aplicar decisão" }));
    await vi.waitFor(() => expect(bodies).toHaveLength(1));
    expect(bodies[0]).toEqual({ itemIds: ["a", "b"], decision: "Cancel", obs: null }); // item 30 é de outro grupo
  });

  it("exportar baixa o CSV e reaproveitar mostra os candidatos", async () => {
    const createObjectURL = vi.fn(() => "blob:x");
    Object.assign(URL, { createObjectURL, revokeObjectURL: vi.fn() });
    const click = vi.spyOn(HTMLAnchorElement.prototype, "click").mockImplementation(() => {});
    const reuseBodies: unknown[] = [];
    server.use(
      ...base(),
      http.get(
        `${API}/v1/sessions/se1/export`,
        () => new HttpResponse("a;b\n1;2\n", { headers: { "Content-Type": "text/csv" } }),
      ),
      http.get(`${API}/v1/sessions`, () => ok(paged([session(), session({ id: "old", cycle: 0, request: "OLD" })]))),
      http.post(`${API}/v1/sessions/se1/reuse`, async ({ request }) => {
        reuseBodies.push(await request.json());
        return ok({
          summary: { igual: 1 },
          items: [
            {
              n: 3,
              objectName: "ZMIG_X",
              ref: "BSEG",
              situation: "igual",
              originN: 7,
              originStatus: "Confirmado KB",
              linesBefore: 2,
              linesAfter: 3,
            },
          ],
        });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<RemediationSessionPage />, at("?tab=export"));
    await user.click(await screen.findByRole("button", { name: "Baixar atc_status.csv" }));
    await vi.waitFor(() => expect(click).toHaveBeenCalled());
    expect(createObjectURL).toHaveBeenCalled();
    await vi.waitFor(() => expect(screen.getByRole("option", { name: /Ciclo 0/ })).toBeInTheDocument());
    expect(screen.getAllByRole("option", { name: /Ciclo/ })).toHaveLength(1); // a própria sessão fica de fora
    await user.selectOptions(screen.getByLabelText("Sessão de origem"), "old");
    await user.click(screen.getByRole("button", { name: "Buscar candidatos" }));
    expect(await screen.findByText("ZMIG_X")).toBeInTheDocument();
    expect(screen.getByText("2 → 3")).toBeInTheDocument();
    expect(reuseBodies).toEqual([{ fromSessionId: "old" }]);
    click.mockRestore();
  });

  it("histórico: antes e depois, transições, tempo líquido e runs", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/sessions/se1/history`, () => ok(history())),
    );
    renderWithProviders(<RemediationSessionPage />, at("?tab=history"));
    expect(await screen.findByText("Antes e depois por situação")).toBeInTheDocument();
    expect(screen.getByText("→ Isenção")).toBeInTheDocument();
    expect(screen.getByText("10min 0s")).toBeInTheDocument();
    expect(screen.getByText("1h 1min 40s")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Leitura de fontes" })).toHaveAttribute("href", "/execucoes/r1");
  });

  it("a aba Propostas e aplicação monta o componente do slot", async () => {
    server.use(...base());
    renderWithProviders(<RemediationSessionPage />, at("?tab=apply"));
    expect(await screen.findByText("apply se1")).toBeInTheDocument();
  });
});
