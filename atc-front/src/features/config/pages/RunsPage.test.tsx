import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { run, sapSystem } from "@/test/factories/registry";
import { API, ok, paged } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { RunsPage } from "./RunsPage";

describe("Execuções (config)", () => {
  it("mostra ativos com cancelar e o histórico com link e filtros na URL", async () => {
    let cancelled = "";
    const seen: string[] = [];
    server.use(
      http.get(`${API}/v1/sap-systems`, () => ok(paged([sapSystem()]))),
      http.get(`${API}/v1/runs`, ({ request }) => {
        const u = new URL(request.url);
        seen.push(u.search);
        return u.searchParams.get("activeOnly") === "true"
          ? ok(paged([run({ id: "ra", status: "Running", kind: "Extraction" })]))
          : ok(paged([run({ id: "rh" })]));
      }),
      http.post(`${API}/v1/runs/ra/cancel`, () => {
        cancelled = "ra";
        return ok(run({ id: "ra", status: "Cancelling" }));
      }),
    );
    const user = userEvent.setup();
    const { router } = renderWithProviders(<RunsPage />, { route: "/config/execucoes", path: "/config/execucoes" });
    await user.click(await screen.findByRole("button", { name: "Cancelar Extração do ATC" }));
    await vi.waitFor(() => expect(cancelled).toBe("ra"));
    expect(await screen.findByRole("link", { name: "Teste de conexão" })).toHaveAttribute("href", "/execucoes/rh");
    await user.selectOptions(screen.getByLabelText("Filtrar por tipo"), "ConnectionTest");
    await vi.waitFor(() => expect(router.state.location.search).toContain("kind=ConnectionTest"));
    await vi.waitFor(() => expect(seen.some((s) => s.includes("kind=ConnectionTest"))).toBe(true));
  });
});
