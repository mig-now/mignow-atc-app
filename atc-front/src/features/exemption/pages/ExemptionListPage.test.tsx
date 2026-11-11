import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { sapSystem } from "@/test/factories/registry";
import { API, ok, paged, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { batch, report, session } from "../test-data";
import { ExemptionListPage } from "./ExemptionListPage";

const base = () => [
  http.get(`${API}/v1/sap-systems`, () => ok(paged([sapSystem()]))),
  http.get(`${API}/v1/sessions`, () => ok(paged([session()]))),
];
const at = { route: "/isencao", path: "/isencao" };

describe("Isenção: lista de lotes", () => {
  it("lista os lotes com totais e filtra por ambiente (na URL)", async () => {
    const seen: string[] = [];
    server.use(
      ...base(),
      http.get(`${API}/v1/exemption-batches`, ({ request }) => {
        seen.push(request.url);
        return ok(paged([batch({ status: "Simulated", approver: "APROV1", totals: { SIMULADO: 3, BLOQUEADO: 1 } })]));
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<ExemptionListPage />, at);
    expect(await screen.findByText("Simulado", { selector: "td" })).toBeInTheDocument();
    expect(screen.getByText("APROV1")).toBeInTheDocument();
    expect(screen.getByText("P1, P2, P3")).toBeInTheDocument();
    await user.selectOptions(screen.getByLabelText("Filtrar por ambiente"), "s1");
    await vi.waitFor(() => expect(seen.some((u) => u.includes("sapSystemId=s1"))).toBe(true));
    expect(router.state.location.search).toContain("sapSystemId=s1");
  });

  it("Novo lote: mostra o relatório do listar e abre o lote", async () => {
    let body: unknown;
    server.use(
      ...base(),
      http.get(`${API}/v1/exemption-batches`, () => ok(paged([]))),
      http.post(`${API}/v1/exemption-batches`, async ({ request }) => {
        body = await request.json();
        return ok({ batch: batch(), report: report() });
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<ExemptionListPage />, at);
    await user.click(await screen.findByRole("button", { name: "Novo lote" }));
    const dialog = await screen.findByRole("dialog");
    await vi.waitFor(() => expect(within(dialog).getAllByRole("option").length).toBeGreaterThan(1));
    await user.selectOptions(
      within(dialog).getByLabelText("Sessão de remediação"),
      "11111111-aaaa-bbbb-cccc-222222222222",
    );
    // as prioridades da sessão (1,2) vêm marcadas
    expect(within(dialog).getByRole("checkbox", { name: "P1" })).toBeChecked();
    expect(within(dialog).getByRole("checkbox", { name: "P3" })).not.toBeChecked();
    await user.click(within(dialog).getByRole("checkbox", { name: "P3" }));
    await user.click(within(dialog).getByRole("checkbox", { name: /Incluir itens antigos/ }));
    await user.click(within(dialog).getByRole("button", { name: "Criar lote" }));
    expect(await screen.findByText("Lote criado")).toBeInTheDocument();
    expect(body).toEqual({
      sessionId: "11111111-aaaa-bbbb-cccc-222222222222",
      priorities: [1, 2, 3],
      includeObs: true,
    });
    expect(screen.getByText("Sem vínculo com finding")).toBeInTheDocument();
    expect(screen.getByText("Prováveis sem escopo")).toBeInTheDocument();
    expect(screen.getByText(/checksum -1/, { selector: "span" })).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Abrir o lote" }));
    await vi.waitFor(() => expect(router.state.location.pathname).toBe("/isencao/b1"));
  });

  it("mostra o erro da API quando a sessão não tem itens de isenção", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/exemption-batches`, () => ok(paged([]))),
      http.post(`${API}/v1/exemption-batches`, () =>
        problem(400, "Nenhum item com status Isenção.", "Exemption.NoItems"),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<ExemptionListPage />, at);
    await user.click(await screen.findByRole("button", { name: "Novo lote" }));
    const dialog = await screen.findByRole("dialog");
    await vi.waitFor(() => expect(within(dialog).getAllByRole("option").length).toBeGreaterThan(1));
    await user.selectOptions(
      within(dialog).getByLabelText("Sessão de remediação"),
      "11111111-aaaa-bbbb-cccc-222222222222",
    );
    await user.click(within(dialog).getByRole("button", { name: "Criar lote" }));
    expect(await within(dialog).findByText("Nenhum item com status Isenção.")).toBeInTheDocument();
  });
});
