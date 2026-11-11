import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { settings } from "@/test/factories/registry";
import { API, ok } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { SettingsPage } from "./SettingsPage";

describe("Parâmetros", () => {
  it("carrega, valida e salva", async () => {
    let body: unknown;
    server.use(
      http.get(`${API}/v1/settings`, () => ok(settings())),
      http.put(`${API}/v1/settings`, async ({ request }) => {
        body = await request.json();
        return ok(settings({ parallelism: 6 }));
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<SettingsPage />);
    const field = await screen.findByLabelText("Paralelismo");
    expect(field).toHaveValue(4);
    await user.clear(field);
    await user.type(field, "0");
    await user.click(screen.getByRole("button", { name: "Salvar" }));
    expect(await screen.findByText("Mínimo: 1.")).toBeInTheDocument();
    await user.clear(field);
    await user.type(field, "6");
    await user.click(screen.getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(body).toEqual(settings({ parallelism: 6 })));
  });
});
