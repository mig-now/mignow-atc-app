import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { API, ok } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { TriageRulesPage } from "./TriageRulesPage";

const base = {
  condition: "",
  order: 0,
  builtIn: true,
  active: true,
  refObjects: [],
  objectPattern: null,
  messagePattern: null,
  obs: null,
  usesRefObjects: false,
};
const R1 = {
  ...base,
  key: "R1",
  code: "R1",
  description: "sem código + Ambiguous → Cancelado",
  effect: "Cancelado",
  scope: "All",
  order: 5,
  condition: "O tipo do objeto não tem código ABAP (TABL, VIEW) e a classificação do finding é Ambiguous.",
};
const R9 = {
  ...base,
  key: "R9",
  code: "R9",
  description: "upgrade: objeto referenciado BSEG/KONV → Isenção",
  effect: "Isenção",
  scope: "Upgrade",
  refObjects: ["BSEG", "KONV"],
  usesRefObjects: true,
  order: 2,
  condition: "O objeto referenciado está em: BSEG, KONV.",
};
const ZR10 = {
  ...base,
  key: "ZR10",
  code: "ZR10",
  description: "Legado YMIG",
  effect: "Cancelado",
  scope: "Conversion",
  builtIn: false,
  objectPattern: "YMIG*",
};

describe("Regras de triagem", () => {
  it("lista o cadastro com o escopo e filtra pelo tipo de projeto", async () => {
    const queries: (string | null)[] = [];
    server.use(
      http.get(`${API}/v1/triage-rules`, ({ request }) => {
        const type = new URL(request.url).searchParams.get("projectType");
        queries.push(type);
        return ok(type === "Conversion" ? [R1] : [R1, R9]);
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<TriageRulesPage />);
    const r9 = (await screen.findByText("R9")).closest("tr") as HTMLElement;
    expect(within(r9).getByText("Só upgrade")).toBeInTheDocument();
    expect(within(r9).getByText("O objeto referenciado está em: BSEG, KONV.")).toBeInTheDocument();
    // ordem de avaliação: R9 (2) vem antes da R1 (5)
    const rows = screen.getAllByRole("row").slice(1);
    expect(within(rows[0] as HTMLElement).getByText("R9")).toBeInTheDocument();
    expect(within(rows[1] as HTMLElement).getByText("R1")).toBeInTheDocument();
    const r1 = screen.getByText("R1").closest("tr") as HTMLElement;
    expect(within(r1).getByText("Todos")).toBeInTheDocument();
    expect(within(r1).getByText("Padrão")).toBeInTheDocument();
    // regra padrão não se exclui
    expect(within(r1).queryByRole("button", { name: "Apagar R1" })).not.toBeInTheDocument();

    await user.selectOptions(screen.getByRole("combobox", { name: "Filtrar por tipo de projeto" }), "Conversion");
    await vi.waitFor(() => expect(screen.queryByText("R9")).not.toBeInTheDocument());
    expect(queries).toEqual([null, "Conversion"]);
  });

  it("edita a regra padrão (escopo e objetos) sem mexer no efeito", async () => {
    const puts: unknown[] = [];
    server.use(
      http.get(`${API}/v1/triage-rules`, () => ok([R1, R9])),
      http.put(`${API}/v1/triage-rules/R9`, async ({ request }) => {
        puts.push(await request.json());
        return ok({ ...R9, scope: "All" });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<TriageRulesPage />);
    await user.click(await screen.findByRole("button", { name: "Editar R9" }));
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText("Regra no código · avaliada em 2º lugar")).toBeInTheDocument();
    expect(within(dialog).getByText("O objeto referenciado está em: BSEG, KONV.")).toBeInTheDocument();
    // código editável (renomear); a chave R9 continua na obs gravada
    expect(within(dialog).getByLabelText("Código")).toBeEnabled();
    expect(within(dialog).getByText(/A chave R9 não muda/)).toBeInTheDocument();
    expect(within(dialog).getByLabelText("Efeito")).toBeDisabled();
    // filtro adicional (opcional) também na regra padrão
    expect(within(dialog).getByRole("group", { name: "Filtro adicional" })).toBeInTheDocument();
    const code = within(dialog).getByLabelText("Código");
    await user.clear(code);
    await user.type(code, "zr9");
    await user.selectOptions(within(dialog).getByLabelText("Escopo"), "All");
    const refs = within(dialog).getByLabelText("Objetos referenciados");
    await user.clear(refs);
    await user.type(refs, "bseg, mara");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(puts).toHaveLength(1));
    expect(puts[0]).toMatchObject({
      code: "ZR9",
      scope: "All",
      effect: "Isenção",
      refObjects: ["BSEG", "MARA"],
      active: true,
    });
  });

  it("cria regra nova por critério e exclui só a nova", async () => {
    const posts: unknown[] = [];
    let deleted = "";
    server.use(
      http.get(`${API}/v1/triage-rules`, () => ok([R1, ZR10])),
      http.post(`${API}/v1/triage-rules`, async ({ request }) => {
        posts.push(await request.json());
        return ok(ZR10);
      }),
      http.delete(`${API}/v1/triage-rules/:code`, ({ params }) => {
        deleted = String(params.code);
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<TriageRulesPage />);
    await screen.findByText("ZR10");
    await user.click(screen.getByRole("button", { name: "Nova regra" }));
    const dialog = await screen.findByRole("dialog");
    await user.type(within(dialog).getByLabelText("Código"), "zr11");
    await user.type(within(dialog).getByLabelText("Descrição"), "Teste");
    // sem critério: não envia
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    expect(await within(dialog).findByText(/Informe ao menos um critério/)).toBeInTheDocument();
    expect(posts).toHaveLength(0);
    await user.selectOptions(within(dialog).getByLabelText("Efeito"), "Cancelado");
    await user.selectOptions(within(dialog).getByLabelText("Escopo"), "Conversion");
    await user.type(within(dialog).getByLabelText("Nome do objeto"), "YMIG*");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(posts).toHaveLength(1));
    expect(posts[0]).toMatchObject({
      code: "ZR11",
      description: "Teste",
      effect: "Cancelado",
      scope: "Conversion",
      objectPattern: "YMIG*",
      refObjects: [],
    });

    const row = screen.getByText("ZR10").closest("tr") as HTMLElement;
    expect(within(row).getByText("Nova")).toBeInTheDocument();
    await user.click(within(row).getByRole("button", { name: "Apagar ZR10" }));
    await user.click(await within(await screen.findByRole("dialog")).findByRole("button", { name: "Apagar" }));
    await vi.waitFor(() => expect(deleted).toBe("ZR10"));
  });
});
