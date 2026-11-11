import type { RouteConfigEntry } from "@react-router/dev/routes";
import routes from "./routes";

function paths(entries: RouteConfigEntry[], prefix = ""): string[] {
  return entries.flatMap((e) => {
    const own = e.path ? `${prefix}/${e.path}`.replace(/\/+/g, "/") : prefix || "/";
    const children = e.children ? paths(e.children, e.path ? own : prefix) : [];
    return [...(e.index ? [prefix || "/"] : e.path && !e.children ? [own] : e.path ? [own] : []), ...children];
  });
}

describe("rotas (§9 do desenho)", () => {
  it("expõem exatamente as rotas do contrato", async () => {
    const list = new Set(paths(await routes));
    const expected = [
      "/",
      "/config",
      "/config/clientes",
      "/config/ambientes",
      "/config/ia",
      "/config/parametros",
      "/config/execucoes",
      "/extracao",
      "/extracao/nova",
      "/remediacao",
      "/remediacao/nova",
      "/remediacao/:sessionId",
      "/isencao",
      "/isencao/:batchId",
      "/varredura",
      "/varredura/:scanId",
      "/relatorio",
      "/regras-triagem",
      "/kb",
      "/kb/:category/:element",
      "/execucoes/:runId",
      "/login",
    ];
    expect([...list].sort()).toEqual([...new Set(expected)].sort());
  });
});
