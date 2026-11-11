import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { aiProvider } from "@/test/factories/registry";
import { API, ok } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { AiProvidersPage } from "./AiProvidersPage";

describe("Provedores de IA", () => {
  it("lista sem expor a chave e cria um provedor com a chave", async () => {
    const created: unknown[] = [];
    server.use(
      http.get(`${API}/v1/ai-providers`, () => ok([aiProvider()])),
      http.post(`${API}/v1/ai-providers`, async ({ request }) => {
        created.push(await request.json());
        return ok(aiProvider({ id: "p2" }));
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<AiProvidersPage />);
    expect(await screen.findByText("Claude Opus")).toBeInTheDocument();
    expect(screen.getByText("Definida")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Novo provedor" }));
    const dialog = await screen.findByRole("dialog");
    await user.type(within(dialog).getByLabelText("Nome"), "Mecânico");
    await user.type(within(dialog).getByLabelText("Modelo"), "claude-haiku-4-5");
    await user.type(within(dialog).getByLabelText("Chave de API"), "sk-123");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(created).toHaveLength(1));
    expect(created[0]).toEqual({
      name: "Mecânico",
      kind: "Anthropic",
      model: "claude-haiku-4-5",
      apiKey: "sk-123",
      isDefault: false,
    });
  });

  it("edição sem nova chave não envia a chave", async () => {
    const updated: Record<string, unknown>[] = [];
    server.use(
      http.get(`${API}/v1/ai-providers`, () => ok([aiProvider()])),
      http.put(`${API}/v1/ai-providers/p1`, async ({ request }) => {
        updated.push((await request.json()) as Record<string, unknown>);
        return ok(aiProvider());
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<AiProvidersPage />);
    await user.click(await screen.findByRole("button", { name: "Editar Claude Opus" }));
    await user.click(within(await screen.findByRole("dialog")).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(updated).toHaveLength(1));
    expect(updated[0]?.apiKey).toBeNull();
  });
});
