import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { customer } from "@/test/factories/registry";
import { API, ok, paged, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { CustomersPage } from "./CustomersPage";

describe("Clientes", () => {
  it("cria um cliente e fecha o formulário", async () => {
    const created: unknown[] = [];
    server.use(
      http.get(`${API}/v1/customers`, () => ok(paged([customer()]))),
      http.post(`${API}/v1/customers`, async ({ request }) => {
        created.push(await request.json());
        return ok(customer({ id: "c2", name: "Novo" }));
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<CustomersPage />);
    expect(await screen.findByText("Cliente A")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Novo cliente" }));
    const dialog = await screen.findByRole("dialog");
    await user.type(within(dialog).getByLabelText("Nome"), "Novo");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(screen.queryByRole("dialog")).not.toBeInTheDocument());
    expect(created).toEqual([{ name: "Novo", code: null }]);
  });

  it("valida o nome e mostra o conflito do backend", async () => {
    server.use(
      http.get(`${API}/v1/customers`, () => ok(paged([]))),
      http.post(`${API}/v1/customers`, () => problem(409, 'Já existe um cliente "X".', "Customer.NameAlreadyExists")),
    );
    const user = userEvent.setup();
    renderWithProviders(<CustomersPage />);
    await user.click(await screen.findByRole("button", { name: "Novo cliente" }));
    const dialog = await screen.findByRole("dialog");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    expect(await within(dialog).findByText("Obrigatório.")).toBeInTheDocument();
    await user.type(within(dialog).getByLabelText("Nome"), "X");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    expect(await within(dialog).findByText('Já existe um cliente "X".')).toBeInTheDocument();
  });

  it("edita e apaga", async () => {
    const updated: unknown[] = [];
    let deleted = "";
    server.use(
      http.get(`${API}/v1/customers`, () => ok(paged([customer()]))),
      http.put(`${API}/v1/customers/c1`, async ({ request }) => {
        updated.push(await request.json());
        return ok(customer());
      }),
      http.delete(`${API}/v1/customers/c1`, ({ request }) => {
        deleted = request.url;
        return new Response(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderWithProviders(<CustomersPage />);
    await user.click(await screen.findByRole("button", { name: "Editar Cliente A" }));
    const dialog = await screen.findByRole("dialog");
    await user.click(within(dialog).getByLabelText("Ativo"));
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));
    await vi.waitFor(() => expect(updated).toEqual([{ name: "Cliente A", code: "A", active: false }]));
    await user.click(screen.getByRole("button", { name: "Apagar Cliente A" }));
    await user.click(await within(await screen.findByRole("dialog")).findByRole("button", { name: "Apagar" }));
    await vi.waitFor(() => expect(deleted).toContain("/v1/customers/c1"));
  });
});
