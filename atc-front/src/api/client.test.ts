import { HttpResponse, http } from "msw";
import { API, ok, problem } from "@/test/msw/handlers";
import { server } from "@/test/msw/server";
import { apiFetch, setLanguageProvider, setTokenProvider } from "./client";

describe("apiFetch (contrato com o backend)", () => {
  it("desembrulha o envelope e manda idioma, token e traceparent", async () => {
    let headers: Headers | undefined;
    server.use(
      http.get(`${API}/v1/me`, ({ request }) => {
        headers = request.headers;
        return ok({ name: "X" });
      }),
    );
    setLanguageProvider(() => "es-ES");
    setTokenProvider(async () => "tok");
    await expect(apiFetch("/v1/me")).resolves.toEqual({ name: "X" });
    expect(headers?.get("Accept-Language")).toBe("es-ES");
    expect(headers?.get("Authorization")).toBe("Bearer tok");
    expect(headers?.get("traceparent")).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-01$/);
    setTokenProvider(async () => null);
    setLanguageProvider(() => "pt-BR");
  });

  it("transforma ProblemDetails em ApiError com o código estável e os campos em camelCase", async () => {
    server.use(
      http.post(`${API}/v1/customers`, () =>
        problem(400, "Alguns campos estão inválidos.", "Validation.Failed", { Name: ["Obrigatório."] }),
      ),
    );
    await expect(apiFetch("/v1/customers", { method: "POST" })).rejects.toEqual({
      status: 400,
      title: "Alguns campos estão inválidos.",
      code: "Validation.Failed",
      errors: { name: ["Obrigatório."] },
    });
  });

  it("204 vira null", async () => {
    server.use(http.put(`${API}/v1/sap-systems/1/password`, () => new HttpResponse(null, { status: 204 })));
    await expect(apiFetch("/v1/sap-systems/1/password", { method: "PUT" })).resolves.toBeNull();
  });
});
