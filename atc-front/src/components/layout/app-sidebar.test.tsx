import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/render";
import { AppSidebar } from "./app-sidebar";

vi.mock("@/auth/use-auth", () => ({
  useAuth: () => ({
    name: "Usuário Dev",
    email: "dev@migatc.local",
    isAuthenticated: true,
    isMock: true,
    logout: () => {},
  }),
}));

describe("menu lateral", () => {
  it("lista os módulos do §9 com as rotas certas", () => {
    renderWithProviders(<AppSidebar />);
    const expected: [string, string][] = [
      ["Início", "/"],
      ["Extração", "/extracao"],
      ["Remediação", "/remediacao"],
      ["Isenção", "/isencao"],
      ["Varredura MIGNOW", "/varredura"],
      ["Relatório final", "/relatorio"],
      ["Regras de triagem", "/regras-triagem"],
      ["Base de conhecimento", "/kb"],
    ];
    for (const [name, href] of expected) expect(screen.getByRole("link", { name })).toHaveAttribute("href", href);
    expect(screen.getByText("ATC")).toBeInTheDocument();
  });

  it("a engrenagem do rodapé abre Clientes, Ambientes, IA, Parâmetros e Execuções", async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppSidebar />);
    await user.click(screen.getByRole("button", { name: "Configuração" }));
    const expected: [string, string][] = [
      ["Clientes", "/config/clientes"],
      ["Ambientes", "/config/ambientes"],
      ["IA", "/config/ia"],
      ["Parâmetros", "/config/parametros"],
      ["Execuções", "/config/execucoes"],
    ];
    for (const [name, href] of expected)
      expect(await screen.findByRole("menuitem", { name })).toHaveAttribute("href", href);
  });

  it("não tem papéis: só Consultor, sem 'ver como' nem área de admin", async () => {
    const user = userEvent.setup();
    renderWithProviders(<AppSidebar />);
    await user.click(screen.getByRole("button", { name: "Menu do usuário" }));
    expect(await screen.findByText("dev@migatc.local")).toBeInTheDocument();
    expect(screen.getAllByText("Consultor").length).toBeGreaterThan(0);
    expect(screen.queryByText(/Ver como/)).not.toBeInTheDocument();
    expect(screen.queryByText(/Admin/)).not.toBeInTheDocument();
  });
});
