import { render, screen } from "@testing-library/react";
import { ThemeProvider, useTheme } from "./theme-provider";

function Show() {
  return <p>{useTheme().theme}</p>;
}

describe("tema", () => {
  afterEach(() => localStorage.clear());

  it("é claro por padrão", () => {
    render(
      <ThemeProvider>
        <Show />
      </ThemeProvider>,
    );
    expect(screen.getByText("light")).toBeInTheDocument();
    expect(document.documentElement).toHaveClass("light");
  });

  it("respeita a escolha salva", () => {
    localStorage.setItem("migatc-theme", "dark");
    render(
      <ThemeProvider>
        <Show />
      </ThemeProvider>,
    );
    expect(document.documentElement).toHaveClass("dark");
  });
});
