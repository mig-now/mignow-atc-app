import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { API, ok, paged } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { ReportPage } from "./ReportPage";

const point = (o: Record<string, unknown> = {}) => ({
  t: 1,
  rot: "08.11 19:58",
  KB: 2,
  Análise: 1,
  Isenção: 1,
  Cancelado: 0,
  Pendente: 7,
  objetos_pendentes: 5,
  ciclo: 1,
  total: 11,
  total_objetos: 8,
  ...o,
});

const report = {
  gerado_em: "2026-11-08T23:00:00Z",
  cliente: {
    nome: "Cliente Demonstração",
    sid: "SBX",
    mandante: "100",
    release: "S/4",
    papel: "Teste",
    autor: "AUT",
    request: "SBXK1",
  },
  extracao: {
    serie: "ZMIG_SIM",
    display_id: "ATC-1",
    executada: "20.10.2026",
    total: 13,
    em_baseline: 2,
    com_baseline: 11,
    p1: 1,
    p2: 3,
    p3: 7,
    fonte: "extractions",
  },
  escopo: ["P1", "P2"],
  ciclos: [
    {
      ciclo: 1,
      fonte: "s",
      inicio: null,
      escopo: ["P1"],
      findings: 11,
      objetos: 8,
      por_solucao: { Pendente: 7, KB: 4 },
      ja_tratados_antes: { qtde: 0, por_solucao_anterior: {} },
    },
  ],
  itens: [
    {
      n: 1,
      ciclo: 1,
      obj: "ZA",
      tipo: "PROG",
      check: "Performance",
      msg: "m",
      ref: "FROM",
      sol: "Pendente",
      sis: "",
      req: "",
      obs: "",
      data: "",
      hist: [],
    },
    {
      n: 2,
      ciclo: 1,
      obj: "ZB",
      tipo: "PROG",
      check: "Performance",
      msg: "m",
      ref: "FROM",
      sol: "KB",
      sis: "",
      req: "",
      obs: "",
      data: "",
      hist: [],
    },
  ],
  objetos_total: 8,
  evolucao: { pontos: [point({ rot: "início", KB: 0, Pendente: 11 }), point()], fonte: ["status_history"] },
  aplicacao: { por_request: {}, fontes_ativadas: 0, manuais: [], request_sessao: null },
  isencao: null,
  kb: { fatos_novos: 0, cartoes_novos: 0, elementos_com_cartoes: null, relatorios_lote: 0 },
  versoes: {},
  custos: {
    fases: [],
    total: { sessoes: 3, liquido_s: 393, total: 0, saida: 0, cache_leitura: 0, entrada_escrita: 0 },
    remediacao_liquido_s: 141,
    fonte: "x",
    relatorio_incluido: false,
    relogio_no_lugar_do_liquido: [],
  },
  comparacao_manual: {
    premissas: { objetos_dia: 64, horas_dia: 8, fonte: "p" },
    ciclos: [],
    total: {
      objetos_tratados: 3,
      objetos_pendentes: 5,
      manual_s: 1350,
      ia_s: 393,
      ia_remediacao_s: 141,
      manual_dias: 0.05,
      ganho_s: 957,
      reducao_pct: 70.9,
      fator: 3.4,
      fator_remediacao: 9.6,
    },
    fonte: "f",
  },
  execucao_processo: {
    execucoes: [],
    sem_registro: [],
    totais: {
      agentes: 0,
      agentes_por_tipo: {},
      agentes_por_modelo: {},
      tokens_por_modelo: {},
      ativo_agentes_s: 0,
      ganho_paralelismo_s: 0,
      execucoes_com_registro: 0,
      execucoes_sem_registro: 0,
    },
    fonte: "r",
  },
  complementos: { destaques: [], pendencias: [] },
  validacao: { ok: false, avisos: ["Soma dos ciclos difere da extração"] },
};

const base = () => [
  http.get(`${API}/v1/customers`, () => ok(paged([{ id: "c1", name: "Cliente Demonstração" }]))),
  http.get(`${API}/v1/sap-systems`, () => ok(paged([{ id: "s1", displayName: "SBX/100" }]))),
  http.get(`${API}/v1/customers/c1/report`, () => ok(report)),
];

describe("Relatório final", () => {
  it("renderiza resumo, avisos de validação, burndown e check × solução", async () => {
    server.use(...base());
    const user = userEvent.setup();
    renderWithProviders(<ReportPage />, { route: "/relatorio", path: "/relatorio" });
    expect(await screen.findByText("Avisos de validação")).toBeInTheDocument();
    expect(screen.getByText("Soma dos ciclos difere da extração")).toBeInTheDocument();
    await user.click(screen.getByRole("tab", { name: "Evolução" }));
    expect(await screen.findByRole("img", { name: "Burndown por findings" })).toBeInTheDocument();
    expect(screen.getByRole("img", { name: "Burndown por objetos" })).toBeInTheDocument();
    await user.click(screen.getByRole("tab", { name: "Check × solução" }));
    expect(await screen.findByText("Performance")).toBeInTheDocument();
    await user.click(screen.getByRole("tab", { name: "Manual × IA" }));
    expect(await screen.findByText("70,9%")).toBeInTheDocument();
  });

  it("histórico do processo usa o filtro de módulo", async () => {
    let module: string | null = "x";
    server.use(
      ...base(),
      http.get(`${API}/v1/history`, ({ request }) => {
        module = new URL(request.url).searchParams.get("module");
        return ok([
          {
            module: "Extraction",
            stage: "Extração do ATC",
            sapSystemId: 1,
            subjectId: null,
            start: "2026-11-08T22:55:00Z",
            end: "2026-11-08T22:59:00Z",
            clockSeconds: 234,
            netSeconds: 200,
            runs: 3,
            runsCompleted: 3,
            runsFailed: 0,
            inputTokens: 0,
            outputTokens: 0,
            totalTokens: 0,
            cost: 0,
            callsWithoutCost: 0,
            aiCalls: 0,
            maxParallelism: 1,
            runsByKind: {},
            byModel: [],
            parallelPeriods: [],
          },
        ]);
      }),
    );
    renderWithProviders(<ReportPage />, { route: "/relatorio?tab=history&module=extraction", path: "/relatorio" });
    expect(await screen.findByText("Extração do ATC")).toBeInTheDocument();
    expect(module).toBe("extraction");
    expect(screen.getByText("3min 54s")).toBeInTheDocument();
  });

  it("destaques manuais: adiciona e salva por PUT", async () => {
    let saved: unknown = null;
    server.use(
      ...base(),
      http.get(`${API}/v1/customers/c1/report/highlights`, () => ok({ highlights: [], pending: [], premise: null })),
      http.put(`${API}/v1/customers/c1/report/highlights`, async ({ request }) => {
        saved = await request.json();
        return ok({ highlights: [{ texto: "Ganho de 3x", fonte: null }], pending: [], premise: null });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<ReportPage />, { route: "/relatorio?tab=highlights", path: "/relatorio" });
    const addButtons = await screen.findAllByRole("button", { name: "Adicionar" });
    await user.click(addButtons[0] as HTMLElement);
    await user.type(screen.getByLabelText("Texto"), "Ganho de 3x");
    await user.click(screen.getByRole("button", { name: "Salvar destaques" }));
    await vi.waitFor(() => expect(saved).toMatchObject({ highlights: [{ texto: "Ganho de 3x" }], pending: [] }));
    expect(await screen.findByText("Destaques salvos.")).toBeInTheDocument();
  });
});
