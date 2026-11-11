import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/render";
import { HomePage } from "./HomePage";

vi.mock("@/auth/use-auth", () => ({ useAuth: () => ({ name: "Ana", isAuthenticated: true, isMock: true }) }));

describe("Início", () => {
  it("mostra a saudação e os atalhos dos módulos, sem os ambientes", async () => {
    renderWithProviders(<HomePage />);
    expect(await screen.findByRole("heading", { level: 1, name: "Olá, Ana" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Extração/ })).toHaveAttribute("href", "/extracao");
    expect(screen.getByRole("link", { name: /Remediação/ })).toHaveAttribute("href", "/remediacao");
    expect(screen.getByRole("link", { name: /Regras de triagem/ })).toHaveAttribute("href", "/regras-triagem");
    expect(screen.queryByText("Ambientes")).not.toBeInTheDocument();
  });
});
