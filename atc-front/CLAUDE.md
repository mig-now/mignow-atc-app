# atc-front — Portal MigATC (React Router v8 SPA)

Cópia adaptada do `mignow_migfix/migfix-front` (decisão 22 de `roadmap/app-atc-web.md`). Contrato com o back, rotas e telas:
`../docs/APP_ATC_DESENHO.md` (§2, §8, §9). Padrões: `../blueprint/blueprint-frontend-novo-app.md`.
Stack e versões iguais às do migfix-front (React Router 8 SPA, Vite 8, Tailwind 4, TanStack Query, RHF + Zod, i18next,
MSAL com usuário mock de DEV, Biome, Vitest + MSW, Orval).

## Desvios em relação ao migfix-front e ao blueprint

| Tema | migfix-front | Aqui | Por quê |
|---|---|---|---|
| Design System | `tokens.css` provisório | igual (`src/styles/tokens.css`, decisão 21) | Sem o PAT `read:packages`. Com acesso: trocar o import em `app.css` e apagar `tokens.css` |
| API | 2 clients (`config`, `sintaxe`) | **1 client** `atc` (`VITE_API_URL`, mutator `apiFetch`) | Uma API só (atc-back, https 7091) |
| Papéis | AdminMignow / Member, `AdminOnly`, "Ver como Membro" | **Só Consultor** (qualquer usuário autenticado); sem `AdminOnly`, sem dev-role, sem `/v1/me` obrigatório | Decisão 20 / §2 do desenho |
| Features | sintaxe, catálogo, prompt, config/{api-keys,knowledge-repo,users,projects,alerts} | removidas | Específicas do migfix |
| Componentes genéricos | em `features/sintaxe/components` | `src/features/shared/` com props de tipos locais (`shared/types.ts`), sem depender do client gerado | Reuso por todos os módulos |
| Storage | `theme`, `lang`, `sidebar-collapsed` | `migatc-theme`, `migatc-lang`, `migatc-sidebar-collapsed`, `migatc-autologin-attempts` | Evita colisão entre apps no mesmo domínio |
| Diff | jsdiff lado a lado | igual | — |
| Client gerado | versionado | **ausente até o backend publicar o Swagger** (`src/api/generated/` não existe; typecheck passa sem ele) | Backend em desenvolvimento paralelo |

## Estrutura

```
app/routes.ts + app/routes/*.tsx   rotas finas do §9 (só montam uma página de features/<módulo>/)
src/api/        client.ts (apiFetch, ApiError), unwrapped.ts, query-client.ts (401 global), run-hub.ts (SignalR), token.ts
src/auth/       MSAL + usuário mock de DEV (dev@migatc.local)
src/components/ ui/ (shadcn-like), layout/ (shell, sidebar, topbar, user-menu)
src/features/   home, config, extraction, remediation, exemption, scan, report, kb, triage, runs  (pages/ components/ hooks/ index.ts)
src/features/shared/   componentes genéricos (abaixo)
src/i18n/locales/<idioma>/<namespace>.json   um arquivo por namespace e idioma
src/test/       render.tsx (renderWithProviders), msw/{server,handlers}.ts (ok()/problem()), factories/
```

## Regras

1. **i18n (nº 1)**: nenhuma string no JSX (inclui aria-label/title/placeholder). Chaves **planas** (`keySeparator: false`)
   nos 3 idiomas (pt-BR, en, es), um arquivo por namespace: `common`, `config`, `extraction`, `remediation`, `exemption`,
   `scan`, `report`, `kb`, `triage`, `runs`, `errors`, `validation`. Gate: `src/i18n/locale-parity.test.ts` (mesmas chaves nos 3
   idiomas, todos os namespaces registrados em `i18n/config.ts`). `common` é o namespace padrão; os demais se usam com
   `useTranslation("<ns>")` ou `t("<ns>:chave")`. Mensagens do backend chegam por **código** (`errors:code.<Código>` +
   argumentos, via `CodeMessage`) ou já localizadas (`title` do ProblemDetails via `Accept-Language`).
2. **Camadas**: rota fina → página de feature → hook de dados (`features/<módulo>/hooks`) → client gerado
   (`src/api/generated/atc/<tag>/<tag>`). Página nunca chama a API direto.
3. **Client gerado não se edita.** Contrato mudou: backend rodando (`dotnet run --launch-profile https`, porta 7091) →
   `pnpm sync:api` (baixa `https://localhost:7091/swagger/v1/swagger.json` para `openapi/atc.json` e roda o Orval).
   `pnpm gen:api` só roda o Orval sobre o `openapi/atc.json` existente. Orval: `mode: tags-split`, react-query, fetch,
   mutator `apiFetch`; uma pasta por tag Swagger (`Customers`, `SapSystems`, `Runs`, `Extractions`, `Sessions`,
   `Exemptions`, `Scans`, `Reports`, `Knowledge`, `Settings`, `AiProviders`, `Me`, `Health`).
4. O mutator desembrulha o envelope `{success,data,message}`: queries usam `unwrapped(useGetX())`, mutations usam
   `payload(funcaoGerada())`; erros chegam como `ApiError` (`isApiError`). Ações que disparam Run devolvem 202 + `RunResponse`;
   conflito de ambiente = 409 (mostrar o run que bloqueia).
5. Ações de botão com mutation: `attempt(...)` ou `mutate(..., { onSuccess })` — nunca `await mutateAsync` sem tratar.
6. Execuções (Runs): progresso ao vivo com `useRunLiveUpdates(runId, active, queryKeys)` (`api/run-hub.ts`, SignalR
   `/hubs/runs`, `RunChanged`/`StepChanged`, `JoinRun`) **mais polling** enquanto ativa (`isRunActive`).
7. Tema por classe no `<html>`; **claro é o padrão**. Só tokens semânticos, nunca cor literal.
8. Sem papéis: não criar guardas por papel. O backend recusa o que não pode (401/403/409).
9. **Tipografia (escala única, a dos exemplos do blueprint/MigLoad)** – só estes tamanhos:
   - `text-xl font-semibold`: título da página (`PageHeader`, login).
   - `text-base font-semibold`: título de seção, card (`CardTitle`) e diálogo.
   - `text-sm font-semibold`: subtítulo dentro de uma seção (`h4`).
   - `text-lg font-semibold tabular-nums`: número de destaque (indicador em `<dd>`).
   - `text-sm` (**padrão**): conteúdo – célula de tabela, item de lista, parágrafo, formulário, botão, aba, alerta, corpo de diálogo,
     resumo, mensagem de erro exibida como conteúdo.
   - `text-xs` + `text-muted-foreground`: só o **auxiliar** – metadado abaixo de um valor (data, ciclo, id), dica/erro de campo, legenda,
     rodapé ("atualizado…"), selo e cabeçalho de tabela (já nos componentes). Conteúdo principal nunca em `text-xs`.
   Nada de `text-[..px]`, `text-2xl+` (só o wordmark da sidebar) nem tamanho novo sem combinar.

## Como os módulos trabalham (agentes em paralelo)

- Cada módulo edita **somente** `src/features/<módulo>/` e os **seus** arquivos de i18n
  (`src/i18n/locales/{pt-BR,en,es}/<módulo>.json`). Mapa módulo → pasta → namespace → rotas:
  `home` (common) `/` · `config` (config) `/config/*` · `extraction` (extraction) `/extracao` ·
  `remediation` (remediation) `/remediacao*` · `exemption` (exemption) `/isencao*` · `scan` (scan) `/varredura*` ·
  `report` (report) `/relatorio` · `kb` (kb) `/kb*` · `triage` (triage) `/regras-triagem` · `runs` (runs) `/execucoes/:runId`.
- Não editar `app/routes.ts`, `components/`, `features/shared/`, `api/`, nem os namespaces `common`/`errors`/`validation`
  sem combinar; se precisar de um componente genérico novo, avisar o responsável pela base. Texto de status/ações que já
  existe em `common` (runStatus, stepStatus, actions, pagination…) se reutiliza com `t("common:...")`/namespace padrão.
- Hooks de dados em `features/<módulo>/hooks/*.ts` usando o client gerado `@/api/generated/atc/<tag>/<tag>`
  (ex.: `import { useGetV1Runs } from "@/api/generated/atc/runs/runs"`). Cada módulo cria os hooks que precisa.
- Mensagens por código: o módulo que gera o código acrescenta `code.<Código>` em `errors.json` (3 idiomas) e os rótulos de
  passos em `runs:steps.<CÓDIGO_DO_PASSO>`.
- Testes ao lado do código (`*.test.tsx`) com `renderWithProviders` + `server.use(http.get(`${API}/v1/...`, () => ok(...)))`.
  Request sem handler falha o teste (`onUnhandledRequest: "error"`). Factories de DTO gerado ficam em `src/test/factories`.
- Antes de entregar: `pnpm typecheck && pnpm lint && pnpm test` verdes (`pnpm exec biome check --write .` formata).

## Componentes shared (`src/features/shared`, `@/features/shared`)

`StepsTimeline({steps: RunStep[]})` · `EventsLog({events: RunEvent[], loading?, error?, paging?})` ·
`SourceDiff({before, after})` · `AiCallsTable({calls: AiCall[]})` · `Markdown({text})` ·
`CodeMessage({code, args?})` / `useCodeMessage()` · `RunStatusBadge({status})` / `StepStatusBadge` / `ActiveBadge` /
`isRunActive` (`status.tsx`) · `PageHeader({title, description?, actions?})` · `Pagination({page, pageSize, total, onPage})` ·
`EmptyState({text})` · `ApiErrorAlert({error})` / `errorTitle` · `attempt`, `applyServerErrors`, `orNull` (`form.ts`) ·
`DeleteButton({name, description?, onDelete})`. Tipos em `shared/types.ts` (`RunStep`, `RunEvent`, `AiCall`, `MessageArgs`).

## Rodar

```bash
cp .env.example .env    # VITE_API_URL=https://localhost:7091; GUIDs 00000000-… = usuário mock de DEV (sem login)
pnpm install
pnpm dev                # http://localhost:5173
pnpm typecheck && pnpm lint && pnpm test
pnpm build
```
