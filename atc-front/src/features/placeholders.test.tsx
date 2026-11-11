import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/render";
import { ConfigLayout } from "./config";

vi.mock("@/auth/use-auth", () => ({ useAuth: () => ({ name: "Ana", isAuthenticated: true, isMock: true }) }));

const cases: [string, () => React.JSX.Element, string][] = [];

describe("páginas placeholder", () => {
  it.each(cases)("%s mostra título e 'em construção'", async (_n, Page, title) => {
    renderWithProviders(<Page />);
    expect(await screen.findByRole("heading", { level: 1, name: title })).toBeInTheDocument();
    expect(screen.getByText("Em construção.")).toBeInTheDocument();
  });

  it("a engrenagem tem as 5 abas", async () => {
    renderWithProviders(<ConfigLayout />);
    expect(await screen.findByRole("navigation", { name: "Seções da configuração" })).toBeInTheDocument();
    expect(screen.getAllByRole("link")).toHaveLength(5);
  });
});
