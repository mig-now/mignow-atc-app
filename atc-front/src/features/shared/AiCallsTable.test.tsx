import { screen } from "@testing-library/react";
import { aiCall } from "@/test/factories";
import { renderWithProviders } from "@/test/render";
import { AiCallsTable } from "./AiCallsTable";

describe("AiCallsTable", () => {
  it("mostra cada chamada e o total", async () => {
    renderWithProviders(
      <AiCallsTable
        calls={[
          aiCall({ attempt: 1, inputTokens: 7000, outputTokens: 4000, cost: 0.01 }),
          aiCall({ attempt: 2, inputTokens: 8000, outputTokens: 5000, cost: 0.02, usage: "Analysis" }),
        ]}
      />,
    );
    expect(await screen.findByText("Total")).toBeInTheDocument();
    expect(screen.getByText("Revisão")).toBeInTheDocument();
    expect(screen.getByText("Análise")).toBeInTheDocument();
    expect(screen.getByText("15.000")).toBeInTheDocument();
    expect(screen.getByText("9.000")).toBeInTheDocument();
  });

  it("sem chamadas avisa", async () => {
    renderWithProviders(<AiCallsTable calls={[]} />);
    expect(await screen.findByText("Nenhuma chamada de IA.")).toBeInTheDocument();
  });

  it("custo que o serviço não informa aparece como não informado", async () => {
    renderWithProviders(<AiCallsTable calls={[aiCall({ cost: null })]} />);
    expect(await screen.findByText("não informado")).toBeInTheDocument();
  });
});
