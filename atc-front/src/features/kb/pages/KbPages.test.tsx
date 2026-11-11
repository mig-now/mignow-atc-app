import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { API, ok } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { KbElementPage } from "./KbElementPage";
import { KbIndexPage } from "./KbIndexPage";

const card = (o: Record<string, unknown> = {}) => ({
  id: "S00831",
  occurrences: 2,
  pattern: "Tabela/view removida sem substituta",
  original: "matnr TYPE mara-matnr,",
  new: "matnr TYPE matnr18,",
  kbSource: "elementos/tabelas/MARA.md",
  isContribution: false,
  alerts: [],
  complexity: "simples",
  replacedBy: null,
  ...o,
});

describe("Base de conhecimento", () => {
  it("lista elementos e padrões e filtra pela busca (estado na URL)", async () => {
    let lastSearch: string | null = null;
    server.use(
      http.get(`${API}/v1/kb/index`, ({ request }) => {
        lastSearch = new URL(request.url).searchParams.get("search");
        return ok({
          elements: [{ category: "tabelas", element: "MARA", slug: "MARA", cards: 14, contributions: 0, facts: 0 }],
          patterns: [{ id: "P02", description: "Tabela/view substituída", solutions: 738, occurrences: 2862 }],
        });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<KbIndexPage />, { route: "/kb", path: "/kb" });
    expect(await screen.findByRole("link", { name: "MARA" })).toHaveAttribute("href", "/kb/tabelas/MARA");
    expect(screen.getByText("Tabela/view substituída")).toBeInTheDocument();
    await user.type(screen.getByLabelText("Buscar elemento"), "mara");
    await user.click(screen.getByRole("button", { name: "Buscar" }));
    await vi.waitFor(() => expect(lastSearch).toBe("mara"));
  });

  it("lookup mostra as soluções lado a lado", async () => {
    server.use(
      http.get(`${API}/v1/kb/index`, () => ok({ elements: [], patterns: [] })),
      http.get(`${API}/v1/kb/lookup`, ({ request }) => {
        const q = new URL(request.url).searchParams;
        expect(q.get("ref")).toBe("MARA");
        return ok({ cards: [card()], total: 5 });
      }),
    );
    renderWithProviders(<KbIndexPage />, { route: "/kb?ref=MARA&robt=TABL", path: "/kb" });
    expect(await screen.findByText("Problema (ABAP original)")).toBeInTheDocument();
    expect(screen.getByText("matnr TYPE matnr18,")).toBeInTheDocument();
    expect(screen.getByText("Mostrando 1 de 5 soluções.")).toBeInTheDocument();
  });

  it("página do elemento mostra soluções, alertas, contribuições, fatos e substituição", async () => {
    server.use(
      http.get(`${API}/v1/kb/elements/tabelas/MARA`, () =>
        ok({
          category: "tabelas",
          element: "MARA",
          cards: [card({ alerts: ["Validar o MATNR de 40 posições"], replacedBy: "C-MARA-001" })],
          contributions: [card({ id: "C-MARA-001", isContribution: true })],
          facts: [{ id: "F-MARA-001", date: "2026-10-01", text: "MARA mudou" }],
          replacements: [
            { cardId: "S00831", replacedBy: "C-MARA-001", element: "MARA", date: "2026-10-02", reason: "melhor" },
          ],
        }),
      ),
    );
    renderWithProviders(<KbElementPage />, { route: "/kb/tabelas/MARA", path: "/kb/:category/:element" });
    expect(await screen.findByRole("heading", { level: 1, name: "MARA" })).toBeInTheDocument();
    expect(screen.getByText("Validar o MATNR de 40 posições")).toBeInTheDocument();
    expect(screen.getByText("Substituída por C-MARA-001")).toBeInTheDocument();
    expect(screen.getByText("Contribuições C-* (1)")).toBeInTheDocument();
    expect(screen.getByText("MARA mudou")).toBeInTheDocument();
    expect(screen.getByText(/S00831 substituída por C-MARA-001/)).toBeInTheDocument();
  });
});
