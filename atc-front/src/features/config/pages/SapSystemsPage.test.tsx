import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { customer, sapSystem } from "@/test/factories/registry";
import { API, ok, paged } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { SapSystemsPage } from "./SapSystemsPage";

const base = () => [http.get(`${API}/v1/customers`, () => ok(paged([customer()])))];

describe("Ambientes", () => {
  it("lista com selos de produção e simulador", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems`, () =>
        ok(
          paged([
            sapSystem({ isProduction: true }),
            sapSystem({ id: "s2", displayName: "Sim", host: "sim://demo", isSimulator: true }),
          ]),
        ),
      ),
    );
    renderWithProviders(<SapSystemsPage />);
    expect(await screen.findByText("Cliente A DI4/400")).toBeInTheDocument();
    expect(screen.getByText("Produção")).toBeInTheDocument();
    expect(screen.getByText("Simulador")).toBeInTheDocument();
  });

  it("testa a conexão e avisa que produção é somente leitura", async () => {
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems`, () => ok(paged([sapSystem({ isProduction: true })]))),
      http.post(`${API}/v1/sap-systems/s1/test`, () =>
        ok({
          ok: true,
          release: "758",
          clientCategory: "P",
          certThumbprint: "AB12CD",
          latencyMs: 120,
          testedAt: "2026-11-06T10:00:00Z",
        }),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<SapSystemsPage />);
    await user.click(await screen.findByRole("button", { name: "Testar conexão de Cliente A DI4/400" }));
    const dialog = await screen.findByRole("dialog");
    expect(await within(dialog).findByText("120 ms")).toBeInTheDocument();
    expect(within(dialog).getByText("AB12CD")).toBeInTheDocument();
    expect(within(dialog).getByText("758")).toBeInTheDocument();
    expect(within(dialog).getByText(/escrita no SAP está bloqueada/)).toBeInTheDocument();
  });

  it("cria um ambiente (senha só na criação)", async () => {
    const created: Record<string, unknown>[] = [];
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems`, () => ok(paged([]))),
      http.post(`${API}/v1/sap-systems`, async ({ request }) => {
        created.push((await request.json()) as Record<string, unknown>);
        return ok(sapSystem());
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<SapSystemsPage />);
    await user.click(await screen.findByRole("button", { name: "Novo ambiente" }));
    const dialog = await screen.findByRole("dialog");
    await vi.waitFor(() => expect(within(dialog).getByRole("option", { name: "Cliente A" })).toBeInTheDocument());
    await user.selectOptions(within(dialog).getByLabelText("Cliente"), "c1");
    await user.type(within(dialog).getByLabelText("SID"), "di4");
    await user.type(within(dialog).getByLabelText("Mandante"), "400");
    await user.type(within(dialog).getByLabelText("Host"), "10.1.1.1");
    await user.type(within(dialog).getByLabelText("Autor (AUT-XX)"), "AUT-NT");
    await user.type(within(dialog).getByLabelText("Usuário SAP"), "CONS");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    expect(await within(dialog).findByText("Obrigatório.")).toBeInTheDocument(); // falta a senha
    await user.type(within(dialog).getByLabelText("Senha"), "segredo");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(created).toHaveLength(1));
    expect(created[0]).toMatchObject({
      customerId: "c1",
      sid: "DI4",
      client: "400",
      host: "10.1.1.1",
      adtPort: 8001,
      projectType: "Conversion",
      author: "AUT-NT",
      password: "segredo",
    });
  });

  it("na edição não há campo de senha; a troca é à parte", async () => {
    let body: unknown;
    server.use(
      ...base(),
      http.get(`${API}/v1/sap-systems`, () => ok(paged([sapSystem()]))),
      http.put(`${API}/v1/sap-systems/s1/password`, async ({ request }) => {
        body = await request.json();
        return new Response(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<SapSystemsPage />);
    await user.click(await screen.findByRole("button", { name: "Editar Cliente A DI4/400" }));
    expect(within(await screen.findByRole("dialog")).queryByLabelText("Senha")).not.toBeInTheDocument();
    await user.click(within(screen.getByRole("dialog")).getByRole("button", { name: "Cancelar" }));
    await user.click(screen.getByRole("button", { name: "Trocar senha de Cliente A DI4/400" }));
    await user.type(await screen.findByLabelText("Nova senha"), "nova");
    await user.click(within(screen.getByRole("dialog")).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(body).toEqual({ password: "nova" }));
  });
});
