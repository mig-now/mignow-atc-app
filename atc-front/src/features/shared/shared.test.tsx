import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http } from "msw";
import { runEvent, runStep } from "@/test/factories";
import { API, ok } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { renderWithProviders } from "@/test/render";
import { ApiErrorAlert } from "./api-error-alert";
import { CodeMessage } from "./CodeMessage";
import { EventsLog } from "./EventsLog";
import { EmptyState } from "./empty-state";
import { applyServerErrors, attempt, orNull } from "./form";
import { Markdown } from "./Markdown";
import { PageHeader } from "./page-header";
import { Pagination } from "./pagination";
import { SourceDiff } from "./SourceDiff";
import { StepsTimeline } from "./StepsTimeline";
import { isRunActive, RunStatusBadge } from "./status";

describe("StepsTimeline", () => {
  it("ordena por order, mostra status acessível, progresso e cai no código sem tradução", () => {
    renderWithProviders(
      <StepsTimeline
        steps={[
          runStep({ code: "B_STEP", order: 2, status: "Processing", progress: { done: 3, total: 10 } }),
          runStep({ code: "A_STEP", order: 1, status: "Completed" }),
          runStep({ code: "C_STEP", order: 3, status: "Error", messageCode: "Sap.Timeout" }),
        ]}
      />,
    );
    const items = within(screen.getByRole("list", { name: "Passos da execução" })).getAllByRole("listitem");
    expect(items.map((i) => i.textContent)).toEqual([
      expect.stringContaining("A_STEP"),
      expect.stringContaining("B_STEP"),
      expect.stringContaining("C_STEP"),
    ]);
    expect(items[1]).toHaveTextContent("3/10");
    expect(items[1]).toHaveTextContent("Em processamento");
    expect(items[2]).toHaveTextContent("Sap.Timeout"); // código sem tradução aparece cru
  });
});

describe("EventsLog", () => {
  it("lista eventos com nível e código; vazio mostra aviso", () => {
    const { unmount } = renderWithProviders(
      <EventsLog events={[runEvent({ level: "Warning", code: "X.Y", itemRef: "ZPROG" })]} />,
    );
    expect(screen.getByText("aviso")).toBeInTheDocument();
    expect(screen.getByText("ZPROG")).toBeInTheDocument();
    expect(screen.getByText("X.Y")).toBeInTheDocument();
    unmount();
    renderWithProviders(<EventsLog events={[]} />);
    expect(screen.getByText("Sem eventos ainda.")).toBeInTheDocument();
  });

  it("pagina quando recebe paging", async () => {
    const onPage = vi.fn();
    const user = userEvent.setup();
    renderWithProviders(<EventsLog events={[runEvent()]} paging={{ page: 1, pageSize: 50, total: 120, onPage }} />);
    expect(screen.getByText("120 registros · página 1 de 3")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Próxima página" }));
    expect(onPage).toHaveBeenCalledWith(2);
  });
});

describe("SourceDiff", () => {
  it("destaca linhas removidas e adicionadas lado a lado", () => {
    renderWithProviders(<SourceDiff before={"a\nb\nc"} after={"a\nB\nc\nd"} />);
    expect(screen.getByText("Antes")).toBeInTheDocument();
    expect(screen.getByText("Depois")).toBeInTheDocument();
    expect(screen.getByText("b").className).toContain("bg-destructive");
    expect(screen.getByText("B").className).toContain("bg-success");
    expect(screen.getByText("d").className).toContain("bg-success");
  });
});

describe("Markdown", () => {
  it("renderiza markdown e não executa HTML cru", () => {
    renderWithProviders(<Markdown text={"# Título\n\n- item\n\n<script>alert(1)</script>"} />);
    expect(screen.getByRole("heading", { name: "Título" })).toBeInTheDocument();
    expect(screen.getByText("item")).toBeInTheDocument();
    expect(document.querySelector("script")).toBeNull();
  });
});

describe("CodeMessage", () => {
  it("sem tradução mostra o código; com tradução interpola", async () => {
    const { default: i18n } = await import("@/i18n/config");
    i18n.addResource("pt-BR", "errors", "code.Test.Hello", "Olá {{who}}");
    renderWithProviders(
      <>
        <CodeMessage code="Test.Hello" args={{ who: "mundo" }} />|<CodeMessage code="Unknown.Code" />
      </>,
    );
    expect(screen.getByText("Olá mundo|Unknown.Code")).toBeInTheDocument();
  });
});

describe("RunStatusBadge", () => {
  it("traduz o status e identifica os ativos", () => {
    renderWithProviders(<RunStatusBadge status="Completed" />);
    expect(screen.getByText("Concluída")).toBeInTheDocument();
    expect(isRunActive("Running")).toBe(true);
    expect(isRunActive("Completed")).toBe(false);
  });
});

describe("PageHeader / EmptyState / Pagination / ApiErrorAlert", () => {
  it("PageHeader mostra título, descrição e ações", () => {
    renderWithProviders(<PageHeader title="T" description="D" actions={<button type="button">Ação</button>} />);
    expect(screen.getByRole("heading", { name: "T" })).toBeInTheDocument();
    expect(screen.getByText("D")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Ação" })).toBeInTheDocument();
  });

  it("EmptyState mostra o texto", () => {
    renderWithProviders(<EmptyState text="Nada aqui" />);
    expect(screen.getByText("Nada aqui")).toBeInTheDocument();
  });

  it("Pagination desabilita os extremos", () => {
    renderWithProviders(<Pagination page={1} pageSize={10} total={10} onPage={() => {}} />);
    expect(screen.getByRole("button", { name: "Página anterior" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Próxima página" })).toBeDisabled();
  });

  it("ApiErrorAlert usa o título do ApiError ou a mensagem de rede", () => {
    const { rerender } = renderWithProviders(
      <ApiErrorAlert error={{ status: 409, title: "Ambiente ocupado.", code: "Run.Conflict" }} />,
    );
    expect(screen.getByText("Ambiente ocupado.")).toBeInTheDocument();
    rerender(<ApiErrorAlert error={new Error("x")} />);
  });
});

describe("form", () => {
  it("orNull e attempt (sem rejeição solta)", async () => {
    expect(orNull("  ")).toBeNull();
    expect(orNull(" a ")).toBe("a");
    await expect(attempt(() => Promise.reject(new Error("falha")))).resolves.toBeUndefined();
  });

  it("applyServerErrors aplica só os campos conhecidos", () => {
    const setError = vi.fn();
    const applied = applyServerErrors(
      { status: 400, title: "x", code: "Validation.Failed", errors: { name: ["Obrigatório."], other: ["?"] } },
      setError,
      ["name"],
    );
    expect(applied).toBe(true);
    expect(setError).toHaveBeenCalledTimes(1);
    expect(setError).toHaveBeenCalledWith("name", { type: "server", message: "Obrigatório." });
    expect(applyServerErrors(new Error("x"), setError, ["name"])).toBe(false);
  });
});

describe("MSW + envelope", () => {
  it("o servidor de teste responde com o envelope ok()", async () => {
    server.use(http.get(`${API}/v1/ping`, () => ok({ pong: true })));
    const { apiFetch } = await import("@/api/client");
    await expect(apiFetch("/v1/ping")).resolves.toEqual({ pong: true });
  });
});
