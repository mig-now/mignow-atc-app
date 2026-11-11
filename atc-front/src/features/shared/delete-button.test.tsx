import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/render";
import { DeleteButton } from "./delete-button";

describe("Apagar", () => {
  it("pede confirmação e mostra a recusa do backend sem fechar", async () => {
    const onDelete = vi
      .fn()
      .mockRejectedValueOnce({
        status: 409,
        title: "O cliente tem projetos: apague-os antes.",
        code: "Customer.HasProjects",
      })
      .mockResolvedValueOnce(undefined);
    const user = userEvent.setup();
    renderWithProviders(<DeleteButton name="Cliente A" onDelete={onDelete} />);
    await user.click(screen.getByRole("button", { name: "Apagar Cliente A" }));
    const dialog = await screen.findByRole("dialog", { name: "Apagar Cliente A?" });
    expect(onDelete).not.toHaveBeenCalled();
    await user.click(within(dialog).getByRole("button", { name: "Apagar" }));
    expect(await within(dialog).findByText("O cliente tem projetos: apague-os antes.")).toBeInTheDocument();
    await user.click(within(dialog).getByRole("button", { name: "Apagar" }));
    await vi.waitFor(() => expect(screen.queryByRole("dialog")).not.toBeInTheDocument());
    expect(onDelete).toHaveBeenCalledTimes(2);
  });
});
