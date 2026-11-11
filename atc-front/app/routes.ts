import { index, layout, type RouteConfig, route } from "@react-router/dev/routes";

/** Rotas do §9 do desenho (docs/APP_ATC_DESENHO.md). Rotas finas: cada uma monta uma página de `src/features/<módulo>/`. */
export default [
  // Tela de login — full-screen, fora do shell. ÚNICA rota pública.
  route("login", "routes/login.tsx"),
  // Guard PRIMEIRO: nada (nem o shell) renderiza sem sessão.
  layout("routes/_protected.tsx", [
    layout("routes/_app-shell.tsx", [
      index("routes/_index.tsx"),
      // Engrenagem ⚙: cadastros e parâmetros.
      route("config", "routes/config.tsx", [
        index("routes/config._index.tsx"),
        route("clientes", "routes/config.clientes.tsx"),
        route("ambientes", "routes/config.ambientes.tsx"),
        route("ia", "routes/config.ia.tsx"),
        route("parametros", "routes/config.parametros.tsx"),
        route("execucoes", "routes/config.execucoes.tsx"),
      ]),
      route("extracao", "routes/extracao.tsx"),
      route("extracao/nova", "routes/extracao.nova.tsx"),
      route("remediacao", "routes/remediacao._index.tsx"),
      route("remediacao/nova", "routes/remediacao.nova.tsx"),
      route("remediacao/:sessionId", "routes/remediacao.$sessionId.tsx"),
      route("isencao", "routes/isencao._index.tsx"),
      route("isencao/:batchId", "routes/isencao.$batchId.tsx"),
      route("varredura", "routes/varredura._index.tsx"),
      route("varredura/:scanId", "routes/varredura.$scanId.tsx"),
      route("relatorio", "routes/relatorio.tsx"),
      route("regras-triagem", "routes/regras-triagem.tsx"),
      route("kb", "routes/kb._index.tsx"),
      route("kb/:category/:element", "routes/kb.$category.$element.tsx"),
      route("execucoes/:runId", "routes/execucoes.$runId.tsx"),
    ]),
  ]),
] satisfies RouteConfig;
