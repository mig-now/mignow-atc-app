import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { sapSystem } from "@/test/factories/registry";
import { API, ok, paged, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { session } from "../testData";
import { NewRemediationPage } from "./NewRemediationPage";

const extraction = {
  id: "x1",
  sapSystemId: "s1",
  series: "ZMIG_SIM",
  displayId: "ATC-1",
  title: null,
  executedAtUtc: null,
  extractedAt: "2026-11-05T10:00:00Z",
  totalCount: 13,
  inBaselineCount: 2,
  newCount: 11,
  total: { p1: 1, p2: 3, p3: 9 },
  new: { p1: 1, p2: 3, p3: 7 },
  situation: "Extracted",
  fileBaseName: "atc",
};
const base = () => [
  http.get(`${API}/v1/sap-systems`, () => ok(paged([sapSystem({ defaultRequest: "DI4K1" })]))),
  http.get(`${API}/v1/extractions`, () => ok(paged([extraction]))),
];
const at = { route: "/remediacao/nova", path: "/remediacao/nova" };

describe("Remediação: nova sessão", () => {
  it("cria a partir de uma extração (ambiente → extração → prioridades → request) e abre a sessão", async () => {
    const bodies: unknown[] = [];
    server.use(
      ...base(),
      http.post(`${API}/v1/sessions`, async ({ request }) => {
        bodies.push(await request.json());
        return ok(session({ id: "novo" }));
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<NewRemediationPage />, at);
    await vi.waitFor(() => expect(screen.getByRole("option", { name: "Cliente A DI4/400" })).toBeInTheDocument());
    expect(screen.getByRole("button", { name: "Criar sessão" })).toBeDisabled();
    await user.selectOptions(screen.getByLabelText("Ambiente"), "s1");
    expect(screen.getByLabelText("Request de transporte")).toHaveValue("DI4K1");
    await vi.waitFor(() => expect(screen.getByRole("option", { name: /ATC-1/ })).toBeInTheDocument());
    await user.selectOptions(screen.getByLabelText("Extração"), "x1");
    await user.selectOptions(screen.getByLabelText("Prioridades"), "1,2");
    await user.click(screen.getByRole("button", { name: "Criar sessão" }));
    await vi.waitFor(() => expect(router.state.location.pathname).toBe("/remediacao/novo"));
    expect(bodies).toEqual([{ extractionId: "x1", priorities: "1,2", request: "DI4K1" }]);
  });

  it("mostra o erro da API ao criar", async () => {
    server.use(
      ...base(),
      http.post(`${API}/v1/sessions`, () => problem(409, "Já existe uma sessão ativa", "Session.AlreadyActive")),
    );
    const user = userEvent.setup();
    renderWithProviders(<NewRemediationPage />, at);
    await vi.waitFor(() => expect(screen.getByRole("option", { name: "Cliente A DI4/400" })).toBeInTheDocument());
    await user.selectOptions(screen.getByLabelText("Ambiente"), "s1");
    await vi.waitFor(() => expect(screen.getByRole("option", { name: /ATC-1/ })).toBeInTheDocument());
    await user.selectOptions(screen.getByLabelText("Extração"), "x1");
    await user.click(screen.getByRole("button", { name: "Criar sessão" }));
    expect(await screen.findByRole("alert")).toBeInTheDocument();
  });

  it("importa o CSV e mostra o resumo (linhas, ignoradas, em escopo)", async () => {
    let form = "";
    server.use(
      ...base(),
      http.post(`${API}/v1/sessions/import-csv`, async ({ request }) => {
        form = await request.text();
        return ok(
          session({
            id: "csv1",
            import: {
              items: 7,
              byType: { PROG: 7 },
              distinctObjects: 5,
              priorities: "1,2",
              byPriorityInCsv: { "1": 3, "2": 4, "3": 9 },
              outOfScope: 9,
              shiftedColumns: 0,
              ignoredRows: 2,
            },
          }),
        );
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<NewRemediationPage />, at);
    await user.click(screen.getByRole("tab", { name: "Importar CSV" }));
    await vi.waitFor(() => expect(screen.getAllByRole("option", { name: "Cliente A DI4/400" })).toHaveLength(1));
    await user.selectOptions(screen.getByLabelText("Ambiente"), "s1");
    await user.upload(
      screen.getByLabelText(/Arquivo CSV do ATC/),
      new File(["Priority;Check\n1;x"], "atc.csv", { type: "text/csv" }),
    );
    await user.click(screen.getByRole("button", { name: "Importar CSV" }));
    expect(await screen.findByText("Resumo da importação")).toBeInTheDocument();
    expect(screen.getByText("Linhas no CSV").nextSibling).toHaveTextContent("18");
    expect(screen.getByText("Ignoradas").nextSibling).toHaveTextContent("2");
    expect(screen.getByText("Em escopo").nextSibling).toHaveTextContent("7");
    expect(form).toContain('name="sapSystemId"');
    expect(form).toContain('name="file"');
    expect(form).toContain('name="priorities"');
    expect(screen.getByRole("link", { name: "Abrir sessão" })).toHaveAttribute("href", "/remediacao/csv1");
  });
});
