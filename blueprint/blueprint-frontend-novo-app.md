# Blueprint de Frontend — novo app nos moldes do MigLoad

> **Documento par do [Blueprint de Backend](./blueprint-backend-novo-app.md).** Juntos formam o **kit de
> handoff** para iniciar um novo app da Mignow com **Claude Code**, reaproveitando stack, arquitetura,
> padrões e contratos do **MigLoad**. O **contrato back↔front** (envelope, ProblemDetails,
> `Accept-Language`, geração do client Orval) está em **§17 do blueprint de backend** e é referenciado
> pela camada de API aqui (§9).
>
> **Para quem é isto:** o time que vai iniciar o **frontend de um novo app** da Mignow usando
> **Claude Code**, reaproveitando a mesma stack, arquitetura, design system e estrutura de tela do
> **MigLoad**. É ao mesmo tempo (1) o **onboarding** de tudo que o dev precisa instalar/configurar no
> PC e (2) o **spec** que o Claude Code segue para andaimar (scaffold) o projeto.
>
> **Idioma do time:** português (Brasil). Commits, comentários e documentação em **pt-BR**; nomes de
> código (componentes, hooks, tipos) em inglês técnico do ecossistema React/TS. **Nenhuma string
> voltada ao usuário é escrita direto no JSX** — vai sempre pelo i18n (regra nº 1, ver §7).
>
> **Fonte:** todos os trechos de código abaixo são **fiéis ao MigLoad em produção** (extraídos do
> repo `mig-now/mignow-load`, pasta `mignow-load-front/`). Onde aparece o nome do app, use os
> placeholders da seção §0.

---

## 0. Como usar este documento

### Placeholders (troque em todo o projeto)

| Placeholder | Significa | Exemplo |
|---|---|---|
| `<APP>` | Nome de exibição do app (wordmark, `<title>`) | `MigFoo` |
| `<app>` | Slug em kebab-case (nome do pacote, chaves de storage, assets) | `migfoo` |
| `<ORG>` | Org do GitHub (registro privado do DS) | `mig-now` |

O **Design System é reaproveitado como está**: o pacote `@mig-now/ds` (GitHub Packages da org
`mig-now`) continua o mesmo — mesmas cores, tokens, raios e fontes (Geometria/Phonk) e o degradê de
marca. **Só muda a "marca de aplicação"**: o wordmark (`<APP>`), o símbolo/logo e o `<title>`. Não se
cria DS novo, não se edita `node_modules` (§6).

### Trilhas de decisão que este doc cobre

- **Camada de API/backend:** ainda **indefinida** para o novo app. O doc traz **duas trilhas** (§9):
  - **Trilha A** — API real desde o dia 1 (client Orval + token MSAL + 401 global, igual ao MigLoad).
  - **Trilha B** — front primeiro, com dados mockados (MSW); a camada de API entra na fase 2.
  - O **shell, tema, i18n e auth** (§5–§8) são iguais nas duas trilhas.

### Como conduzir com o Claude Code

1. Faça o **onboarding do PC** (§1) — uma vez por dev.
2. Deixe o Claude Code **andar em fases** (§14): scaffold → shell → tema → i18n → auth → (API) → 1ª
   feature. A cada fase, rode os **gates** (`pnpm typecheck && pnpm lint && pnpm test`).
3. **Adote os `CLAUDE.md` por pasta** (§15): eles são o que mantém o Claude Code (e o dev) dentro das
   regras. Copie a disciplina, adaptando o texto ao novo domínio.

---

## 1. Onboarding do dev — instalar/configurar no PC (uma vez)

Ambiente de referência: **Windows 11** (o time trabalha em win32). Onde o comando diverge, há a
variante macOS/Linux.

### 1.1 Node (versão fixada) + pnpm

O projeto **fixa a versão do Node** no `.nvmrc` (hoje **`22.22.0`**) e o **pnpm** no campo
`packageManager` do `package.json` (hoje **`pnpm@11.4.0`**). Use um gerenciador de versão de Node e o
**Corepack** para o pnpm — não instale pnpm global "solto".

```powershell
# Windows — instale um gerenciador de versão de Node (escolha um):
#   - nvm-windows:  https://github.com/coreybutler/nvm-windows
#   - fnm (rápido): winget install Schniz.fnm
# Depois, a versão do projeto:
nvm install 22.22.0 ; nvm use 22.22.0      # (nvm-windows)
# ou:  fnm install && fnm use               # (fnm lê o .nvmrc)

# pnpm via Corepack (vem com o Node) — pega a versão pinada no package.json:
corepack enable
corepack prepare pnpm@11.4.0 --activate
node -v   # deve casar com o .nvmrc
pnpm -v   # 11.4.0
```

```bash
# macOS / Linux (nvm)
nvm install && nvm use        # lê o .nvmrc
corepack enable && corepack prepare pnpm@11.4.0 --activate
```

### 1.2 PAT do GitHub (`read:packages`) + `~/.npmrc` — **obrigatório**

O `@mig-now/ds` é privado no **GitHub Packages**; sem autenticação, `pnpm install` falha com **401/403**.
O roteamento do escopo já vem commitado no `.npmrc` **do projeto** (sem segredo); o **token** fica no
`~/.npmrc` de **cada dev**, nunca no repo.

1. Gere um **PAT classic** em <https://github.com/settings/tokens/new> — marque **somente
   `read:packages`**. Se a org exigir SSO, clique em **Configure SSO → Authorize** no token.
   *(O token do `gh` CLI `gho_…` **não serve** — não tem o escopo.)*
2. Grave no `~/.npmrc` do seu perfil:

```powershell
# Windows (PowerShell)
Add-Content "$env:USERPROFILE\.npmrc" "//npm.pkg.github.com/:_authToken=ghp_SEU_TOKEN_AQUI"
```

```bash
# macOS / Linux / Git Bash
echo "//npm.pkg.github.com/:_authToken=ghp_SEU_TOKEN_AQUI" >> ~/.npmrc
```

| Sintoma no install | Causa | Correção |
|---|---|---|
| `401 Unauthorized` | Token ausente/expirado no `~/.npmrc` | Refazer passos 1–2 |
| `403 Forbidden` | Token sem `read:packages`, sem SSO autorizado, ou sem acesso à org | Conferir escopo/SSO/membership |
| `Can't resolve '@mig-now/ds/...'` | Install não rodou | `pnpm install` |

### 1.3 VS Code + extensões

Comitar `.vscode/extensions.json` para padronizar o time:

```json
{
  "recommendations": [
    "biomejs.biome",
    "bradlc.vscode-tailwindcss",
    "lokalise.i18n-ally"
  ],
  "unwantedRecommendations": [
    "esbenp.prettier-vscode",
    "dbaeumer.vscode-eslint"
  ]
}
```

> **Biome substitui Prettier + ESLint.** Não instale nenhum dos dois — o formatador/linter é o Biome
> (§10). O `i18n-ally` ajuda a ver chaves faltando nos 3 idiomas.

### 1.4 Git (identidade e commits)

- **Branch por tarefa** a partir da `main` (`feat/<resumo>`, `fix/<resumo>`, `docs/<resumo>`). Não
  commite direto na `main`.
- **Conventional Commits em pt-BR** com escopo (`feat(front): …`, `fix(auth): …`). **Sem** coautoria
  de ferramentas nas mensagens.
- Configure a identidade do git para a org (e-mail corporativo). Em máquinas com **mais de uma conta
  GitHub**, garanta que o repo do novo app use a identidade certa (via `includeIf` no `~/.gitconfig`
  por diretório).

### 1.5 Docker (só na **Trilha A**, se houver backend local)

Necessário apenas se o novo app tiver **backend próprio rodando local** (ex.: PostgreSQL de dev).
Na Trilha B (front-first com MSW) **não é preciso**.

### 1.6 Pré-requisito de CI do DS (uma vez, na UI do pacote)

Para o CI do **novo repo** restaurar o `@mig-now/ds`, o pacote precisa **conceder acesso** ao repo em
**Package settings → Manage Actions access** (feito uma vez pela UI do pacote `ds`). Sem isso, o job de
build do CI falha no install mesmo com o `GITHUB_TOKEN`.

---

## 2. Stack (versões exatas) e porquês

Copie as versões abaixo (é o `package.json` do MigLoad; ver §10 para o arquivo inteiro).

| Camada | Libs | Por quê |
|---|---|---|
| **Core** | React **19** · React Router **v8** (framework mode, `ssr:false`, **sem prerender**) · TypeScript **~6** · Vite **8** | SPA protegida por auth; roteamento por arquivos com typegen. |
| **Dados** | TanStack Query **v5** · React Hook Form **7** + Zod **v4** · Orval **8** (client gerado) | Cache/estado de servidor em hooks; formulários tipados; client HTTP gerado do OpenAPI. |
| **UI** | Tailwind CSS **v4** (`@tailwindcss/vite`) · **`@mig-now/ds`** (tokens/fonts) · shadcn local + **Radix** · `lucide-react` · `class-variance-authority` · `clsx` + `tailwind-merge` · `recharts` | DS é a fonte de cores/raios/fontes; primitivos shadcn locais espelham o DS. |
| **i18n** | `i18next` + `react-i18next` (**pt-BR / en / es**) | Regra nº 1: nada de string hardcoded. |
| **Auth** | `@azure/msal-browser` + `@azure/msal-react` (Entra ID) | Auto-login por redirect; toda rota protegida. |
| **Telemetria** | `@microsoft/applicationinsights-web` (plugável, no-op sem config) | Observabilidade opcional. |
| **Testes/Qualidade** | Vitest **4** + Testing Library + **MSW** · Biome **2.5** | Teste de componente contra a rede mockada; lint+format num só tool. |

---

## 3. Estrutura de pastas (blueprint)

```
<app>-front/
  app/                         # React Router (framework mode)
    root.tsx                   # <html>, providers globais, script anti-FOUC, 401 global
    routes.ts                  # mapa de rotas (hierarquia protegida)
    routes/                    # arquivos de rota FINOS (montam páginas de feature)
      login.tsx                # ÚNICA rota pública
      _protected.tsx           # guard (RequireAuth) — nada renderiza sem sessão
      _app-shell.tsx           # sidebar + topbar
      _index.tsx               # / (Início)
  src/
    api/                       # client HTTP (Orval + customFetch)  ← Trilha A/§9
      generated/               #   gerado pelo Orval — NÃO EDITAR
      client.ts                #   mutator: token/idioma/envelope/erro
      query-client.ts          #   makeQueryClient({ onUnauthorized })
      unwrapped.ts             #   reconciliação de tipos do envelope
    auth/                      # MSAL/Entra: msal.ts, auth-provider, require-auth, use-auth
    components/
      ui/                      # primitivos shadcn locais (Button, Dialog, Table, Toast…)
      layout/                  # shell: app-sidebar, topbar, user-menu, theme-toggle, language-switcher
      theme-provider.tsx       # fonte única da classe de tema
    features/<área>/           # domínio autocontido (ver §11 organização)
      components/  hooks/  pages/  schemas/  query-keys.ts  index.ts
    i18n/                      # config.ts, locales/{pt-BR,en,es}/{common,validation,errors}.json, zod-error-map.ts
    lib/                       # env.ts (Zod), cn.ts, format.ts, use-typed-search-params.ts…
    styles/                    # app.css (base + gotchas) + theme.css (ponte DS↔Tailwind)
    telemetry/                 # app-insights (plugável)
    test/                      # renderWithProviders, MSW, factories, MSAL fake
    assets/brand/              # <app>-symbol.png etc.
```

**Convenções de nome de arquivo:** kebab-case por padrão (`use-create-foo.ts`, `app-sidebar.tsx`);
**PascalCase** só para componentes de feature e páginas (`FooForm.tsx`, `FoosListPage.tsx`). Testes
`<alvo>.test.ts(x)` **ao lado** do alvo. Imports via alias `@/` (nunca caminho relativo longo).

---

## 4. Regras obrigatórias (valem para dev humano e IA)

1. **i18n (nº 1):** nenhuma string voltada ao usuário no JSX — **inclui `aria-label`, `title`,
   `placeholder`, `alt`**. Toda feature nasce nos **3 idiomas**. Detalhe em §7.
2. **Teste é definição de pronto:** nenhuma feature/bugfix entra sem teste (componente + MSW para
   fluxo; unidade só para lógica pura). Detalhe em §11.
3. **Segredos:** `.env` é gitignored; **nunca** logar tokens; variáveis validadas por Zod no boot
   (`src/lib/env.ts`) — falta de variável quebra cedo.
4. **Acessibilidade é contrato:** `role`/`aria-*` corretos — os testes selecionam por eles
   (`getByRole`/`getByLabelText`), não por `data-testid`.
5. **Design System é a fonte de cor/tema:** sempre **token semântico** (`bg-muted`, `text-primary`),
   **nunca** cor literal. **Nunca** editar `node_modules`.
6. **Camadas:** rota fina → página de feature → hook de dados → client. **Nada de lógica de negócio na
   página** nem chamada de API direta fora dos hooks.
7. **Clareza > esperteza · explícito > mágico.**

---

## 5. Estrutura de tela (shell) — sidebar, topbar, tema, idioma, usuário+logout

O shell é **idêntico ao MigLoad**. Hierarquia de rotas (o guard vem **primeiro**, o shell aninhado
dentro — nada do sistema renderiza sem sessão):

```ts
// app/routes.ts
import { index, layout, type RouteConfig, route } from "@react-router/dev/routes";

export default [
  // Tela de login — full-screen, FORA do app-shell. ÚNICA rota pública.
  route("login", "routes/login.tsx"),

  // Guard PRIMEIRO: nada (nem o shell/menu) renderiza sem sessão.
  layout("routes/_protected.tsx", [
    layout("routes/_app-shell.tsx", [
      index("routes/_index.tsx"),           // /  (Início)
      // route("foos", "routes/foos._index.tsx"),   // features novas nascem aqui
    ]),
  ]),
] satisfies RouteConfig;
```

### 5.1 `root.tsx` — providers globais + anti-FOUC + 401 global

Pontos críticos: **`<html>` NÃO leva `className` fixo** (o `ThemeProvider` é a fonte única; um script
inline pinta o tema antes do 1º paint, evitando FOUC). O **401 global** é ligado aqui.

```tsx
// app/root.tsx  (essência — adapte o <title> para <APP>)
import "../src/styles/app.css";
import "@/i18n/zod-error-map"; // registra i18n + error map do Zod antes do render
import { QueryClientProvider } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { I18nextProvider } from "react-i18next";
import { Links, Meta, type MetaDescriptor, Outlet, Scripts, ScrollRestoration, useNavigate, useRouteError } from "react-router";
import { makeQueryClient } from "@/api/query-client";
import { AuthProvider } from "@/auth/auth-provider";
import { ThemeProvider } from "@/components/theme-provider";
import { ToastProvider } from "@/components/ui/toast";
import i18n from "@/i18n/config";

// BLOQUEANTE (antes de <Links/>): aplica .dark/.light no <html> antes do 1º paint. Dark é o default.
// NÃO use import de módulo ES aqui — módulos são assíncronos e exibem o flash antes de executar.
const THEME_INIT_SCRIPT =
  "(function(){try{var t=localStorage.getItem('theme')==='light'?'light':'dark';var r=document.documentElement;r.classList.remove('light','dark');r.classList.add(t)}catch(e){}})()";

export function meta(): MetaDescriptor[] { return [{ title: "<APP>" }]; }

export default function Root() {
  const navigate = useNavigate();
  const navigateRef = useRef(navigate);
  useEffect(() => { navigateRef.current = navigate; }, [navigate]);

  // QueryClient criado UMA vez; callback de 401 sempre usa o navigate atual.
  const [queryClient] = useState(() =>
    makeQueryClient(() => {
      if (window.location.pathname === "/login") return;              // já em /login → evita loop
      const from = `${window.location.pathname}${window.location.search}`;
      navigateRef.current("/login", { state: { from }, replace: true }); // SPA nav preserva sessão MSAL
    }),
  );

  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" href="/favicon.ico" sizes="any" />
        {/* biome-ignore lint/security/noDangerouslySetInnerHtml: tema init — string literal constante */}
        <script dangerouslySetInnerHTML={{ __html: THEME_INIT_SCRIPT }} />
        <Meta /><Links />
      </head>
      <body suppressHydrationWarning>
        <I18nextProvider i18n={i18n}>
          <AuthProvider>
            <QueryClientProvider client={queryClient}>
              <ThemeProvider>
                <ToastProvider>
                  <Outlet />
                </ToastProvider>
              </ThemeProvider>
            </QueryClientProvider>
          </AuthProvider>
        </I18nextProvider>
        <ScrollRestoration /><Scripts />
      </body>
    </html>
  );
}
```

> **Também exporte `HydrateFallback` e `ErrorBoundary`** espelhando o mesmo shell (`<html lang="pt-BR">`
> + `DocumentHead`) — sem o `HydrateFallback`, o React Router usa `<html lang="en">` e gera warning de
> hidratação. Ver o `root.tsx` do MigLoad como referência.

### 5.2 Topbar — busca + toggle de tema + seletor de idioma

```tsx
// src/components/layout/topbar.tsx
import { Menu, PanelLeft, Search } from "lucide-react";
import { useTranslation } from "react-i18next";
import { LanguageSwitcher } from "./language-switcher";
import { ThemeToggle } from "./theme-toggle";

interface TopbarProps { onToggleSidebar: () => void; onToggleCollapse: () => void; }

export function Topbar({ onToggleSidebar, onToggleCollapse }: TopbarProps) {
  const { t } = useTranslation();
  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-2 border-b border-border bg-background/80 px-3 backdrop-blur sm:gap-3 sm:px-4">
      <button type="button" onClick={onToggleSidebar} aria-label={t("nav.openMenu")}
        className="inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-foreground md:hidden">
        <Menu className="size-5" />
      </button>
      <button type="button" onClick={onToggleCollapse} aria-label={t("nav.toggleMenu")} title={t("nav.toggleMenu")}
        className="hidden size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-foreground md:inline-flex">
        <PanelLeft className="size-5" />
      </button>
      <div className="relative hidden min-w-0 flex-1 sm:block sm:max-w-md">
        <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
        <input type="search" placeholder={t("search.placeholder")}
          className="field-focus h-9 w-full rounded-md border border-input bg-card/50 pr-3 pl-9 text-sm text-foreground outline-none transition-colors placeholder:text-muted-foreground" />
      </div>
      <div className="ml-auto flex items-center gap-2 sm:gap-3">
        <ThemeToggle />
        <LanguageSwitcher />
      </div>
    </header>
  );
}
```

### 5.3 Sidebar — marca no topo, navegação no meio, usuário no rodapé

Recolhe para um rail de ícones (`collapsed`), vira drawer no mobile. Usa os tokens `--sidebar-*` do DS.
**Adapte** o wordmark `<APP>` e o asset do símbolo.

```tsx
// src/components/layout/app-sidebar.tsx  (essência)
import { Database, LayoutGrid, Users } from "lucide-react";
import { useTranslation } from "react-i18next";
import { NavLink } from "react-router";
import symbolUrl from "@/assets/brand/<app>-symbol.png";
import { cn } from "@/lib/cn";
import { UserMenu } from "./user-menu";

const NAV_ITEMS = [
  { to: "/", key: "nav.home", end: true, Icon: LayoutGrid },
  // { to: "/foos", key: "nav.foos", end: false, Icon: Users },   // extensível
] as const;

export function AppSidebar({ className, collapsed = false, onNavigate }:
  { className?: string; collapsed?: boolean; onNavigate?: () => void }) {
  const { t } = useTranslation();
  return (
    <div className={cn("flex h-dvh flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground transition-[width] duration-200",
      collapsed ? "w-16" : "w-64", className)}>
      <div className={cn("flex h-16 shrink-0 items-center", collapsed ? "justify-center px-0" : "px-5")}>
        <NavLink to="/" onClick={onNavigate} className="flex items-center gap-2" aria-label={t("nav.brandHome")}>
          {collapsed ? (
            <img src={symbolUrl} alt="<APP>" className="h-8 w-auto" />
          ) : (
            <>
              <img src={symbolUrl} alt="" className="h-8 w-auto shrink-0" />
              <span className="font-sans text-2xl font-bold tracking-tight">
                <span className="text-sidebar-foreground">Mig</span>{/* ← parte 1 do wordmark */}
                <span className="text-sidebar-primary">Foo</span>{/* ← parte 2 destacada */}
              </span>
            </>
          )}
        </NavLink>
      </div>
      <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-2">
        {NAV_ITEMS.map(({ to, key, end, Icon }) => (
          <NavLink key={to} to={to} end={end} onClick={onNavigate} title={collapsed ? t(key) : undefined}
            className={({ isActive }) => cn("flex items-center gap-3 rounded-md py-2 text-sm transition-colors",
              collapsed ? "justify-center px-0" : "px-3",
              isActive ? "bg-sidebar-accent font-medium text-sidebar-accent-foreground"
                       : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground")}>
            <Icon className="size-4 shrink-0" />
            {!collapsed && t(key)}
          </NavLink>
        ))}
      </nav>
      <div className={cn("shrink-0 border-t border-sidebar-border", collapsed ? "p-2" : "p-3")}>
        <UserMenu collapsed={collapsed} />
      </div>
    </div>
  );
}
```

### 5.4 Menu do usuário (nome + e-mail) + **Sair**

Autenticado → avatar com iniciais que abre dropdown com nome, e-mail e **Sair** (`logout()`). Não
autenticado → botão **Entrar** (→ `/login`).

```tsx
// src/components/layout/user-menu.tsx  (essência)
import { ChevronsUpDown, LogIn, LogOut } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { useAuth } from "@/auth/use-auth";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";

function initials(name?: string) { /* "Anselmo Otero" → "AO" */ if (!name) return "?";
  const p = name.trim().split(/\s+/); return ((p[0]?.[0] ?? "") + (p.length > 1 ? p.at(-1)?.[0] ?? "" : "")).toUpperCase() || "?"; }

export function UserMenu({ collapsed = false }: { collapsed?: boolean }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { isAuthenticated, name, email, logout } = useAuth();

  if (!isAuthenticated) {
    return <Button type="button" variant="outline" size={collapsed ? "icon" : "sm"} className={collapsed ? "" : "w-full"}
      aria-label={collapsed ? t("user.signIn") : undefined} onClick={() => navigate("/login")}>
      <LogIn />{!collapsed && t("user.signIn")}
    </Button>;
  }
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button type="button" aria-label={t("user.menuLabel")} data-collapsed={collapsed}
          className="flex w-full items-center gap-2 rounded-md p-2 text-left outline-none transition-colors hover:bg-sidebar-accent/60 focus-visible:ring-2 focus-visible:ring-ring data-[collapsed=true]:justify-center">
          <Avatar className="size-8"><AvatarFallback className="bg-primary text-xs text-primary-foreground">{initials(name)}</AvatarFallback></Avatar>
          {!collapsed && (<>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-sidebar-foreground">{name ?? "—"}</p>
              <p className="truncate text-xs text-sidebar-foreground/60">{email ?? "—"}</p>
            </div>
            <ChevronsUpDown className="size-4 shrink-0 text-sidebar-foreground/60" />
          </>)}
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuContent side={collapsed ? "right" : "top"} align={collapsed ? "end" : "start"} className="min-w-56">
        <div className="px-2 py-1.5">
          <p className="truncate text-sm font-medium text-foreground">{name ?? "—"}</p>
          <p className="truncate text-xs text-muted-foreground">{email ?? "—"}</p>
        </div>
        <DropdownMenuSeparator />
        <DropdownMenuItem onSelect={() => logout()}><LogOut />{t("user.signOut")}</DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

### 5.5 Toggle de tema (um botão, mostra o ícone do tema-alvo)

```tsx
// src/components/layout/theme-toggle.tsx
import { Moon, Sun } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useTheme } from "@/components/theme-provider";

export function ThemeToggle() {
  const { t } = useTranslation();
  const { theme, toggle } = useTheme();
  const isDark = theme === "dark";
  const label = isDark ? t("theme.light") : t("theme.dark");
  return (
    <button type="button" onClick={toggle} aria-label={label} title={label}
      className="inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-foreground">
      {isDark ? <Sun className="size-[18px]" /> : <Moon className="size-[18px]" />}
    </button>
  );
}
```

### 5.6 Seletor de idioma (PT / EN / ES — pílula)

```tsx
// src/components/layout/language-switcher.tsx
import { useTranslation } from "react-i18next";
import { cn } from "@/lib/cn";

const LANGUAGES = [{ code: "pt-BR", label: "PT" }, { code: "en", label: "EN" }, { code: "es", label: "ES" }] as const;
const base = (code: string) => code.split("-")[0];

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation();
  const current = i18n.language;
  return (
    // biome-ignore lint/a11y/useSemanticElements: grupo de botões toggle — role="group" + aria-label é o ARIA adequado.
    <div role="group" aria-label={t("a11y.language")} className="flex items-center gap-0.5 rounded-full bg-muted p-0.5">
      {LANGUAGES.map((lang) => {
        const active = base(current) === base(lang.code);
        return (
          <button key={lang.code} type="button" aria-pressed={active}
            onClick={() => { if (!active) void i18n.changeLanguage(lang.code); }}
            className={cn("rounded-full px-2.5 py-1 text-xs font-medium transition-colors",
              active ? "bg-background text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground")}>
            {lang.label}
          </button>
        );
      })}
    </div>
  );
}
```

> **Persistência (localStorage):** o shell persiste `theme`, `lang` e `sidebar-collapsed`. Padrão: ler
> no init com guarda `typeof window !== "undefined"` e persistir via `useEffect`/listener, **tudo em
> `try/catch`** (o storage pode estar bloqueado). Ver §6 (theme-provider) e §7 (i18n config).

---

## 6. Tema + Design System (a ponte DS ↔ Tailwind)

**Reaproveitar o `@mig-now/ds` como está.** Consumimos apenas `tokens/app.css` (tokens shadcn-native
sob `.light`/`.dark`) e `tokens/fonts.css` (Geometria/Phonk). Os componentes `.ui-*` do DS **não** são
importados — nós os **espelhamos** em shadcn local (`src/components/ui/`).

### 6.1 `src/styles/app.css` — imports, base e **gotchas**

```css
@import "tailwindcss";
@import "@mig-now/ds/tokens/fonts.css";  /* @font-face Geometria + Phonk — NEVER edit the DS */
@import "@mig-now/ds/tokens/app.css";    /* tema shadcn-native (.light / .dark) — NEVER edit the DS */
@import "./theme.css";

/* Dark por CLASSE (convenção shadcn): <html class="dark"> é o default do produto. */
@custom-variant dark (&:is(.dark *));

:root { --grad-brand: linear-gradient(118deg, #42b6fe 0%, #7a74ff 52%, #a245ff 100%); }

/* Controles NATIVOS (popup do <select>, scrollbars, autofill) respeitam o tema. */
.light { color-scheme: light; }
.dark  { color-scheme: dark;  }

@layer base {
  *, ::before, ::after { border-color: var(--border); }
  body { background-color: var(--background); color: var(--foreground); font-family: var(--font-sans); -webkit-font-smoothing: antialiased; }
}

/* ⚠️ GOTCHA: foco de campo em degradê azul→roxo — DEVE ficar SEM @layer.
   As utilitárias do Tailwind vivem em @layer utilities e venceriam qualquer regra dentro de
   @layer components (por ORDEM DE CASCADE LAYER), derrotando `border-color: transparent`.
   Estilos SEM layer têm prioridade sobre TODAS as @layer. NÃO mover para dentro de layer. */
.field-focus:focus, .field-focus:focus-visible {
  outline: none; border-color: transparent;
  background: linear-gradient(var(--background), var(--background)) padding-box, var(--grad-brand) border-box;
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--ring) 18%, transparent);
}
.field-focus[aria-invalid="true"]:focus, .field-focus[aria-invalid="true"]:focus-visible {
  border-color: var(--destructive); background: var(--background);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--destructive) 22%, transparent);
}

/* Scrollbars no roxo da marca (--primary) — theme-aware (Firefox + Webkit). */
html { scrollbar-width: thin; scrollbar-color: var(--primary) transparent; }
::-webkit-scrollbar { width: 12px; height: 12px; }
::-webkit-scrollbar-thumb { background-color: var(--primary); border: 3px solid transparent; border-radius: 9999px; background-clip: padding-box; }
```

### 6.2 `src/styles/theme.css` — expõe os tokens do DS no namespace do Tailwind v4

**Não redefina valores** aqui — apenas mapeie `var(--token)` do DS para `--color-*` do Tailwind, para
gerar as utilitárias (`bg-background`, `text-primary`, `border-border`…).

```css
/* Ponte Tailwind v4 ↔ tokens shadcn-native do @mig-now/ds. O DS é a fonte única. */
@theme inline {
  --color-background: var(--background);   --color-foreground: var(--foreground);
  --color-card: var(--card);               --color-card-foreground: var(--card-foreground);
  --color-primary: var(--primary);         --color-primary-foreground: var(--primary-foreground);
  --color-muted: var(--muted);             --color-muted-foreground: var(--muted-foreground);
  --color-accent: var(--accent);           --color-accent-foreground: var(--accent-foreground);
  --color-destructive: var(--destructive); --color-success: var(--success); --color-warning: var(--warning); --color-info: var(--info);
  --color-border: var(--border);           --color-input: var(--input);     --color-ring: var(--ring);
  /* Sidebar (app shell) */
  --color-sidebar: var(--sidebar);         --color-sidebar-foreground: var(--sidebar-foreground);
  --color-sidebar-primary: var(--sidebar-primary);  --color-sidebar-accent: var(--sidebar-accent);
  --color-sidebar-accent-foreground: var(--sidebar-accent-foreground);  --color-sidebar-border: var(--sidebar-border);
  /* Data-viz (Recharts) */
  --color-chart-1: var(--chart-1); /* …2..5 */
  /* Raios e fontes */
  --radius-sm: var(--ui-radius-sm); --radius-md: var(--ui-radius-md); --radius-lg: var(--ui-radius-lg);
  --font-sans: "Geometria", ui-sans-serif, system-ui, sans-serif;
  --font-display: "Phonk", "Geometria", ui-sans-serif, system-ui, sans-serif;
}
```

*(No MigLoad o `theme.css` lista todos os `*-foreground` e os 5 charts — copie o arquivo inteiro do
repo para não faltar token.)*

### 6.3 `src/components/theme-provider.tsx` — fonte única da classe de tema

```tsx
import { createContext, type ReactNode, useContext, useEffect, useState } from "react";
type Theme = "dark" | "light";
const ThemeContext = createContext<{ theme: Theme; toggle: () => void; setTheme: (t: Theme) => void } | null>(null);

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<Theme>(() => {
    if (typeof window === "undefined") return "dark";
    try { return localStorage.getItem("theme") === "light" ? "light" : "dark"; } catch { return "dark"; }
  });
  const setTheme = (next: Theme) => { setThemeState(next); try { localStorage.setItem("theme", next); } catch {} };
  const toggle = () => setTheme(theme === "light" ? "dark" : "light");
  useEffect(() => { const r = document.documentElement; r.classList.remove("light", "dark"); r.classList.add(theme); }, [theme]);
  return <ThemeContext.Provider value={{ theme, toggle, setTheme }}>{children}</ThemeContext.Provider>;
}
export function useTheme() { const c = useContext(ThemeContext); if (!c) throw new Error("useTheme fora do ThemeProvider"); return c; }
```

**Ao alterar estilo:** confira **os dois temas** (light/dark) e o **responsivo** (sidebar vira drawer
no mobile) antes de concluir.

---

## 7. i18n (regra nº 1) — pt-BR / en / es

- **Idiomas:** `pt-BR` (padrão + fallback), `en`, `es`. **Namespaces:** `common` (UI), `validation`
  (Zod), `errors` (API/rede). Recursos em `src/i18n/locales/{pt-BR,en,es}/{common,validation,errors}.json`.
- **Chaves planas** com pontos literais (`foos.toast.created.title`) — não aninhe em objetos.
  Interpolação `{{var}}` (valores no 2º arg do `t()`).
- **Validação traduz sozinha:** `zod-error-map.ts` mapeia os códigos do Zod para o namespace
  `validation` no idioma ativo — basta manter os 3 `validation.json`. **Não** escreva `message:` literal
  no schema.
- **Troca + persistência já existem** no `LanguageSwitcher` (§5.6) + `config.ts`. Não recrie.

### 7.1 `src/i18n/config.ts` (essência)

```ts
import i18next from "i18next";
import { initReactI18next } from "react-i18next";
import { setLanguageProvider } from "@/api/client"; // ← só na Trilha A; na Trilha B, remova esta linha
import ptBRCommon from "./locales/pt-BR/common.json"; /* …demais imports dos 3×3 arquivos… */

const STORAGE_KEY = "lang";
const SUPPORTED = ["pt-BR", "en", "es"];
function initialLanguage(): string {
  if (typeof window === "undefined") return "pt-BR";
  try { const s = window.localStorage.getItem(STORAGE_KEY); return s && SUPPORTED.includes(s) ? s : "pt-BR"; }
  catch { return "pt-BR"; }
}

const i18n = i18next.createInstance();
i18n.use(initReactI18next).init({
  lng: initialLanguage(), fallbackLng: "pt-BR", defaultNS: "common",
  ns: ["common", "validation", "errors"],
  resources: { "pt-BR": { common: ptBRCommon, /* validation, errors */ }, en: { /* … */ }, es: { /* … */ } },
  interpolation: { escapeValue: false }, // React já faz escape
});

// Trilha A: Accept-Language do backend segue o locale ativo (na cultura que o back suporta).
const BACKEND_CULTURES: Record<string, string> = { "pt-BR": "pt-BR", en: "en-US", es: "es-ES" };
setLanguageProvider(() => BACKEND_CULTURES[i18n.language] ?? i18n.language);

i18n.on("languageChanged", (lng) => { if (typeof window === "undefined") return;
  try { window.localStorage.setItem(STORAGE_KEY, lng); } catch {} });
export default i18n;
```

### 7.2 Gate de paridade (obrigatório)

Crie `locale-parity.test.ts` que compara as chaves dos **3** idiomas em **cada** namespace e falha se
alguma faltar. É a regra nº 1 virando verificação mecânica — se ficar vermelho, **falta chave**, não
"ajuste o teste".

**Checklist ao criar/alterar UI:** ☐ toda string via `t()` (inclui aria/title/placeholder) · ☐ chave
nos **3** arquivos do namespace · ☐ namespace correto · ☐ interpolação conferida · ☐ `pnpm test` verde.

---

## 8. Autenticação — MSAL / Entra ID (auto-login por redirect)

Fluxo: **login automático por redirect** (sem clique/popup), toda rota protegida, `/login` como única
tela pública de pouso. Precisa de um **App Registration (SPA)** novo no Entra da org (e, na Trilha A,
outro para a **API** expondo o scope `access_as_user`).

### 8.1 App Registration (Entra) — checklist

- Criar App Registration **SPA** para `<APP> Front`. Anotar **Client ID** e **Tenant ID**.
- **Redirect URIs (tipo SPA):** `http://localhost:5173` (dev) e a URL de produção.
- **Post-logout redirect URI:** registrar `<origem>/login` (o `postLogoutRedirectUri` é `/login`).
- **Trilha A:** App Registration da **API** expõe um scope (ex.: `access_as_user`); a SPA pede consent
  a `api://<client-id-da-api>/access_as_user` (vai em `VITE_ENTRA_API_SCOPE`).

### 8.2 `.env` e o **modo mock de DEV**

O app roda **sem tenant** enquanto os GUIDs forem os **placeholders** `00000000-…` (usuário mock de
DEV). Com GUIDs reais, o fluxo é 100% MSAL. Ver `src/lib/env.ts` (§10) e:

```ts
// src/auth/msal.ts — Entra "configurado" = GUIDs reais (não os placeholders)
const PLACEHOLDER_GUID = /^00000000-/;
export const isEntraConfigured =
  !PLACEHOLDER_GUID.test(env.VITE_ENTRA_CLIENT_ID) && !PLACEHOLDER_GUID.test(env.VITE_ENTRA_TENANT_ID);
```

### 8.3 Instância MSAL — **`sessionStorage` + `loginRedirect`** (obrigatório)

```ts
// src/auth/msal.ts (essência)
import type { AuthenticationResult } from "@azure/msal-browser";
import { EventType, PublicClientApplication } from "@azure/msal-browser";
import { env } from "@/lib/env";

export function createMsalInstance(): PublicClientApplication {
  const instance = new PublicClientApplication({
    auth: {
      clientId: env.VITE_ENTRA_CLIENT_ID,
      authority: `https://login.microsoftonline.com/${env.VITE_ENTRA_TENANT_ID}`,
      redirectUri: typeof window !== "undefined" ? window.location.origin : undefined,
      postLogoutRedirectUri: "/login", // tela estável de pouso pós-logout (registrar no App Registration)
    },
    cache: { cacheLocation: "sessionStorage", storeAuthStateInCookie: false },
    // sessionStorage: o redirect é navegação full-page e o cache PRECISA sobreviver a ela
    // (memoryStorage não sobrevive). NÃO usar localStorage (amplia janela de XSS).
  });
  instance.addEventCallback((e) => { // fixa a conta ativa ao voltar do redirect
    if (e.eventType === EventType.LOGIN_SUCCESS && e.payload) {
      const acc = (e.payload as AuthenticationResult).account; if (acc) instance.setActiveAccount(acc);
    }
  });
  return instance;
}
export const loginRequest = { scopes: [env.VITE_ENTRA_API_SCOPE] };
```

### 8.4 ⚠️ Guard anti-loop (`require-auth.tsx`) — **a regra mais delicada**

O login dispara **automaticamente via `useMsalAuthentication`** (mecanismo oficial do MSAL) **dentro do
`RequireAuth`**. **NUNCA adicione um segundo ponto de disparo** (ex.: `useEffect` com `loginRedirect`
em página/rota) — dois disparos + a volta do Entra em rota protegida causaram um **loop de
re-autorização** (app ⇄ Entra piscando até o Entra bloquear). Há ainda um **circuit breaker** de última
linha (registra tentativas em `sessionStorage`; corta após 2 num intervalo curto e cai no `/login`
manual).

```tsx
// src/auth/require-auth.tsx (essência — ordem de decisão importa)
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { useMsal, useMsalAuthentication } from "@azure/msal-react";
import { useEffect } from "react";
import { Navigate, Outlet, useLocation } from "react-router";
import { isEntraConfigured, loginRequest } from "@/auth/msal";
import { useAuth } from "@/auth/use-auth";
import { Loading } from "@/components/ui/loading";

const AUTO_LOGIN_KEY = "<app>.autologin.attempts"; // ← namespace do app
const AUTO_LOGIN_WINDOW_MS = 60_000, AUTO_LOGIN_MAX_ATTEMPTS = 2;
// …recentAttempts/canAutoLogin/recordAutoLoginAttempt/clearAutoLoginAttempts em sessionStorage (try/catch)…

function AutoLogin() {
  const location = useLocation();
  const { error } = useMsalAuthentication(InteractionType.Redirect, {
    ...loginRequest,
    redirectStartPage: typeof window !== "undefined"
      ? window.location.origin + location.pathname + location.search : undefined, // volta à rota tentada
  });
  useEffect(() => { if (!error) recordAutoLoginAttempt(); }, [error]);
  if (error) { console.error("Auto-login falhou:", error); // nunca logar token
    return <Navigate to="/login" state={{ from: `${location.pathname}${location.search}` }} replace />; }
  return <Loading />; // redirect a caminho → segura no spinner
}

export function RequireAuth() {
  const { inProgress } = useMsal();
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  useEffect(() => { if (isAuthenticated) clearAutoLoginAttempts(); }, [isAuthenticated]);

  if (inProgress !== InteractionStatus.None) return <Loading />; // NUNCA navegar aqui (processa o #code=)
  if (isAuthenticated) return <Outlet />;
  if (!isEntraConfigured) return <Navigate to="/login" replace />;
  if (!canAutoLogin()) return <Navigate to="/login" state={{ from: `${location.pathname}${location.search}` }} replace />;
  return <AutoLogin />;
}
```

### 8.5 `use-auth.ts` — claims para exibição (nome/e-mail/iniciais) + `logout`

```ts
// src/auth/use-auth.ts (essência) — oid/name/email são só para EXIBIÇÃO; a autoria de auditoria
// deriva do JWT `oid` no backend, NUNCA enviada pelo front.
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { isEntraConfigured } from "@/auth/msal";

const DEV_MOCK_USER = { oid: "dev-mock-oid", name: "Usuário Dev", email: "dev@<app>.local" } as const;

export function useAuth() {
  const { instance, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = instance.getActiveAccount() ?? accounts[0] ?? null;
  if (import.meta.env.DEV && !account && !isEntraConfigured) // mock só em DEV e sem Entra configurado
    return { ...DEV_MOCK_USER, isAuthenticated: true, isMock: true, logout: () => {} };
  const claims = account?.idTokenClaims as { oid?: string; name?: string } | undefined;
  return { oid: claims?.oid, name: claims?.name ?? account?.name, email: account?.username,
    isAuthenticated, isMock: false, logout: () => { void instance.logoutRedirect(); } };
}
```

### 8.6 `auth-provider.tsx` — MsalProvider + (Trilha A) ponte do token → client

Na **Trilha B** (sem API), mantenha só o `MsalProvider` e remova o `TokenBridge`/`setTokenProvider`.

```tsx
// src/auth/auth-provider.tsx (essência)
import { InteractionRequiredAuthError, type IPublicClientApplication } from "@azure/msal-browser";
import { MsalProvider, useMsal } from "@azure/msal-react";
import { type ReactNode, useEffect } from "react";
import { setTokenProvider } from "@/api/client"; // ← Trilha A
import { createMsalInstance, loginRequest } from "./msal";

const msalInstance: IPublicClientApplication | null =
  typeof window === "undefined" ? null : createMsalInstance(); // singleton por módulo

function TokenBridge({ children }: { children: ReactNode }) { // Trilha A: registra o provider de token
  const { instance, accounts } = useMsal();
  useEffect(() => {
    setTokenProvider(async () => {
      const account = instance.getActiveAccount() ?? accounts[0] ?? null;
      if (!account) return null;
      try { return (await instance.acquireTokenSilent({ ...loginRequest, account })).accessToken; }
      catch (err) {
        // NÃO dispare acquireTokenRedirect daqui (perderia o estado da tela). Sem token → 401 → handler global.
        if (err instanceof InteractionRequiredAuthError) return null;
        return null;
      }
    });
  }, [instance, accounts]);
  return <>{children}</>;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  if (!msalInstance) return <>{children}</>; // Node/prerender
  return <MsalProvider instance={msalInstance}><TokenBridge>{children}</TokenBridge></MsalProvider>;
}
```

---

## 9. Camada de API — **duas trilhas**

> A camada de API/backend do novo app está **indefinida**. Escolha uma trilha; migrar B→A depois é
> incremental (o shell/tema/i18n/auth não mudam).
>
> 🔗 **O outro lado deste contrato está no [Blueprint de Backend](./blueprint-backend-novo-app.md).** O
> envelope `{ success, data, message }`, o `ProblemDetails` (`detail` = `code` estável), a paginação
> `PagedResult`, o `Accept-Language` e a geração do client Orval a partir do Swagger estão na tabela de
> contrato (§17 lá). Se o novo app tiver backend próprio, os dois blueprints devem ser lidos juntos.

### Trilha A — API real desde o dia 1 (client Orval)

Igual ao MigLoad: **client gerado por Orval** a partir de um `openapi/swagger.json` (snapshot
commitado), com um **mutator `customFetch`** que desembrulha o envelope `ApiResponse<T>`, injeta token/
idioma/`traceparent` e lança `ApiError` tipado.

```ts
// orval.config.ts
import { defineConfig } from "orval";
export default defineConfig({
  <app>: {
    input: "./openapi/swagger.json",
    output: {
      mode: "tags-split", target: "./src/api/generated", schemas: "./src/api/generated/model",
      client: "react-query", httpClient: "fetch",
      override: { mutator: { path: "./src/api/client.ts", name: "customFetch" }, query: { useQuery: true, useMutation: true } },
    },
  },
  <app>Zod: { input: "./openapi/swagger.json",
    output: { mode: "tags", target: "./src/api/generated/zod", client: "zod", fileExtension: ".zod.ts" } },
});
```

```ts
// src/api/client.ts (essência do mutator)
import { env } from "@/lib/env";
export type ApiError = { status: number; title: string; code: string; errors?: Record<string, string[]> };

let getAccessToken: () => Promise<string | null> = async () => null;
export function setTokenProvider(fn: () => Promise<string | null>) { getAccessToken = fn; }
let getLanguage: () => string = () => "pt-BR";
export function setLanguageProvider(fn: () => string) { getLanguage = fn; }

export async function customFetch<T>(url: string, options: RequestInit = {}): Promise<T> {
  const token = await getAccessToken();
  const headers = new Headers(options.headers);
  headers.set("Accept", "application/json");
  headers.set("Accept-Language", getLanguage());
  headers.set("traceparent", /* 00-<id>-<id16>-01 */ traceparent());
  if (token) headers.set("Authorization", `Bearer ${token}`); // NUNCA logar o token
  const res = await fetch(`${env.VITE_API_BASE_URL}${url}`, { ...options, headers });
  const body: unknown = res.status === 204 ? null : await res.json().catch(() => null);
  if (!res.ok) { const b = body as { title?: string; detail?: string; errors?: Record<string,string[]> } | null;
    throw { status: res.status, title: b?.title ?? "Erro inesperado", code: b?.detail ?? "Unknown",
      errors: b?.errors /* normalizar chaves p/ camelCase */ } satisfies ApiError; }
  const ok = body as { data?: T } | null;               // unwrap ApiResponse<T> = { success, data, message }
  return (ok && "data" in ok ? ok.data : body) as T;
}
```

- **`makeQueryClient({ onUnauthorized })`** (`src/api/query-client.ts`): qualquer query/mutation com
  **401** dispara o callback — ligado no `root.tsx` (§5.1) para navegar em SPA até `/login`. **Nunca**
  dispare redirect dentro do fetch.
- **Regeneração:** mudou o contrato do back? Com a API local rodando, `pnpm sync:api` (busca o swagger,
  formata e regenera). Commite `openapi/swagger.json` + `src/api/generated/` **juntos**. A pasta
  `generated/` é **gerada — NÃO EDITAR** (o CI barra drift).
- Reconcilie o tipo do envelope com `unwrapped<T>(query)` (o Orval tipa com o envelope; em runtime o
  mutator devolve o conteúdo).

### Trilha B — front primeiro, com **MSW**

- Construa a UI contra **dados mockados** via MSW (o mesmo harness dos testes, §11). Defina os
  contratos esperados em `src/test/msw/handlers.ts` e/ou num worker de dev.
- Mantenha o **mesmo formato de envelope** (`{ success, data, message }`) desde já — assim, ao plugar a
  API real (Trilha A), o `customFetch` e os hooks não mudam.
- O `client.ts` já é **plugável** (`setTokenProvider`/`setLanguageProvider`): na Trilha B você
  simplesmente não registra o token e aponta o `VITE_API_BASE_URL` para o mock/stub.
- **Fase 2 (plugar API):** adicione `openapi/swagger.json` + `orval.config.ts`, rode `pnpm gen:api`,
  troque os mocks pelos hooks gerados, e religue o `TokenBridge` (§8.6).

### Organização dos hooks de dados (as duas trilhas)

- Um hook por operação (`useFoos`, `useCreateFoo`…) em `src/features/<área>/hooks/` — **a página nunca
  chama a API/mock direto**.
- **Mutations:** invalidam as `query-keys` da área no `onSuccess`/`onSettled`. Ação de negócio com mais
  de uma chamada → encadeia dentro do `mutationFn` (para o usuário é **uma** ação).
- `query-keys.ts` centraliza as chaves (`all` / `list(params)` / `detail(id)`).
- **Estado de tabela vive na URL** (`useTypedSearchParams` + schema Zod: `page`, `pageSize`, `search`,
  `sortBy`, `descending`) — busca/ordenação server-side; não use `useState` (quebra link e voltar do
  navegador).

---

## 10. Configs prontos para copiar

### `package.json` (dependências e scripts)

```json
{
  "name": "<app>-front",
  "private": true,
  "type": "module",
  "packageManager": "pnpm@11.4.0",
  "engines": { "node": ">=22.22.0" },
  "scripts": {
    "dev": "react-router dev",
    "build": "react-router build",
    "preview": "vite preview",
    "typecheck": "react-router typegen && tsc --noEmit -p tsconfig.app.json && tsc --noEmit -p tsconfig.node.json",
    "lint": "biome check .",
    "format": "biome format --write .",
    "test": "vitest run --coverage",
    "test:watch": "vitest",
    "gen:api": "orval",
    "sync:api": "node scripts/sync-openapi.mjs && biome format --write openapi/swagger.json && orval"
  },
  "dependencies": {
    "@azure/msal-browser": "^4.30.0", "@azure/msal-react": "^5.5.0",
    "@hookform/resolvers": "^5.4.0", "@microsoft/applicationinsights-web": "^3.4.2",
    "@mig-now/ds": "^0.1.0",
    "@radix-ui/react-avatar": "^1.2.0", "@radix-ui/react-dialog": "^1.1.17",
    "@radix-ui/react-dropdown-menu": "^2.1.18", "@radix-ui/react-select": "^2.3.1",
    "@radix-ui/react-slot": "^1.3.0", "@radix-ui/react-tabs": "^1.1.21",
    "@react-router/node": "^8.3.0", "@tanstack/react-query": "^5.101.2",
    "class-variance-authority": "^0.7.1", "clsx": "^2.1.1",
    "i18next": "^26.3.3", "isbot": "^5", "lucide-react": "^1.22.0",
    "react": "^19.2.7", "react-dom": "^19.2.7", "react-hook-form": "^7.80.0",
    "react-i18next": "^17.0.8", "react-router": "^8.3.0", "recharts": "^3.10.1",
    "tailwind-merge": "^3.6.0", "zod": "^4.4.3"
  },
  "devDependencies": {
    "@biomejs/biome": "2.5.1", "@react-router/dev": "^8.3.0", "@tailwindcss/vite": "^4.3.1",
    "@testing-library/jest-dom": "^6.9.1", "@testing-library/react": "^16.3.2",
    "@testing-library/user-event": "^14.6.1", "@types/node": "^24.13.2",
    "@types/react": "^19.2.17", "@types/react-dom": "^19.2.3", "@vitejs/plugin-react": "^6.0.2",
    "@vitest/coverage-v8": "^4.1.9", "jsdom": "^29.1.1", "msw": "^2.14.6", "orval": "^8.19.0",
    "tailwindcss": "^4.3.1", "typescript": "~6.0.2", "vite": "^8.1.0", "vitest": "^4.1.9"
  }
}
```
> Na **Trilha B**, `orval` e os scripts `gen:api`/`sync:api` são opcionais até a fase 2. Mantenha `msw`.

### `.npmrc` (do projeto — sem segredo)

```
auto-install-peers=true
@mig-now:registry=https://npm.pkg.github.com
# O PAT (read:packages) fica no ~/.npmrc de CADA dev — nunca aqui (ver §1.2).
```

### `.nvmrc`

```
22.22.0
```

### `vite.config.ts`

```ts
import { reactRouter } from "@react-router/dev/vite";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";
// Paths do tsconfig ("@/…") resolvidos nativamente pelo Vite 8 (sem plugin).
export default defineConfig({ plugins: [tailwindcss(), reactRouter()], resolve: { tsconfigPaths: true } });
```

### `react-router.config.ts`

```ts
import type { Config } from "@react-router/dev/config";
export default {
  ssr: false,               // SPA: saída estática, sem servidor Node em prod
  splitRouteModules: true,  // code-splitting por rota
  // SEM prerender: toda rota (exceto /login) é protegida; não há página pública para pré-renderizar.
} satisfies Config;
```

### `components.json` (shadcn — new-york, ícones lucide)

```json
{
  "$schema": "https://ui.shadcn.com/schema.json",
  "style": "new-york", "rsc": false, "tsx": true,
  "tailwind": { "config": "", "css": "src/styles/app.css", "baseColor": "neutral", "cssVariables": true, "prefix": "" },
  "aliases": { "components": "@/components", "utils": "@/lib/cn", "ui": "@/components/ui", "lib": "@/lib", "hooks": "@/hooks" },
  "iconLibrary": "lucide"
}
```

### `biome.json`

```json
{
  "$schema": "https://biomejs.dev/schemas/2.5.1/schema.json",
  "vcs": { "enabled": true, "clientKind": "git", "useIgnoreFile": true },
  "files": { "ignoreUnknown": true, "includes": ["**", "!src/api/generated"] },
  "css": { "parser": { "tailwindDirectives": true } },
  "formatter": { "enabled": true, "indentStyle": "space", "indentWidth": 2 },
  "javascript": { "formatter": { "quoteStyle": "double" } },
  "linter": {
    "enabled": true,
    "rules": {
      "preset": "recommended",
      "correctness": { "useExhaustiveDependencies": "warn", "useHookAtTopLevel": "error" },
      "a11y": { "preset": "recommended" },
      "security": { "noDangerouslySetInnerHtml": "error" },
      "style": { "noRestrictedImports": { "level": "error", "options": { "patterns": [
        { "group": ["@mig-now/ds/_*"], "message": "Importe apenas a API pública do @mig-now/ds (styles.css/tokens/assets)." }
      ] } } }
    }
  },
  "assist": { "enabled": true, "actions": { "source": { "organizeImports": "on" } } }
}
```

### `tsconfig.json` + `tsconfig.app.json`

```jsonc
// tsconfig.json
{ "files": [], "references": [ { "path": "./tsconfig.app.json" }, { "path": "./tsconfig.node.json" } ] }
```

```jsonc
// tsconfig.app.json (essência)
{
  "compilerOptions": {
    "target": "es2023", "lib": ["ES2023", "DOM"], "module": "esnext",
    "types": ["vite/client", "vitest/globals"], "skipLibCheck": true,
    "moduleResolution": "bundler", "allowImportingTsExtensions": true, "verbatimModuleSyntax": true,
    "moduleDetection": "force", "noEmit": true, "jsx": "react-jsx",
    "strict": true, "noUncheckedIndexedAccess": true, "noUnusedLocals": true, "noUnusedParameters": true,
    "erasableSyntaxOnly": true, "noFallthroughCasesInSwitch": true,
    "rootDirs": [".", "./.react-router/types"],
    "paths": { "@/*": ["./src/*"] }
  },
  "include": ["src", "app", ".react-router/types/**/*"]
}
```

### `src/lib/env.ts` (variáveis validadas por Zod no boot)

```ts
import { z } from "zod";
const schema = z.object({
  VITE_API_BASE_URL: z.string().url(),
  VITE_ENTRA_CLIENT_ID: z.string().min(1),
  VITE_ENTRA_TENANT_ID: z.string().min(1),
  VITE_ENTRA_API_SCOPE: z.string().min(1),
  VITE_APPINSIGHTS_CONNECTION_STRING: z.string().optional(),
});
export type Env = z.infer<typeof schema>;
export function parseEnv(raw: Record<string, unknown>): Env {
  const r = schema.safeParse(raw);
  if (!r.success) throw new Error(`Env inválida: ${r.error.issues.map((i) => i.path.join(".")).join(", ")}`);
  return r.data;
}
export const env: Env = parseEnv(import.meta.env as Record<string, unknown>);
```

### `.env.example`

```
VITE_API_BASE_URL=https://localhost:7079
# GUIDs 00000000-… são PLACEHOLDERS: enquanto assim, o app usa o usuário mock de DEV (sem login real).
VITE_ENTRA_CLIENT_ID=00000000-0000-0000-0000-000000000000
VITE_ENTRA_TENANT_ID=00000000-0000-0000-0000-000000000000
VITE_ENTRA_API_SCOPE=api://00000000-0000-0000-0000-000000000000/access_as_user
# VITE_APPINSIGHTS_CONNECTION_STRING=
```

---

## 11. Harness de testes (Vitest + Testing Library + MSW)

- Testes **ao lado do alvo** (`<alvo>.test.ts(x)`), não em árvore espelhada.
- `src/test/render.tsx` → **`renderWithProviders`** (envolve QueryClient, **MSAL fake**, i18n pt-BR,
  ThemeProvider, ToastProvider e MemoryRouter; aceita `route` para iniciar numa URL).
- `src/test/msw/` → servidor + handlers de linha de base. `factories/` → dados determinísticos.
  `msal.tsx` → MSAL fake. `setup.ts` → polyfills de jsdom para Radix.

**Regras:**
- **Mocke a rede, nunca o hook/serviço.** Sobrescreva por caso com `server.use(...)`.
- **Interaja como o usuário** (`getByRole`/`getByLabelText` + `userEvent`); nada de `data-testid` para
  contornar a UI.
- **Afirme a consequência** (handler chamado com o argumento certo, estado atualizado, rota mudada) —
  presença de elemento é pré-requisito, não asserção.
- **Envelope da API** nos handlers: `{ success, data, message }`.
- **`onUnhandledRequest: "error"`**: request sem handler **falha o teste** (pega chamada inesperada).
- **Prova por mutação** para guards e contratos anti-regressão (ex.: "não dispara `loginRedirect` ao
  montar"): quebre a produção de propósito e confirme o vermelho.

### `vitest.config.ts` (com pisos de cobertura)

```ts
import { defineConfig } from "vitest/config";
export default defineConfig({
  resolve: { tsconfigPaths: true },
  test: {
    environment: "jsdom", globals: true, setupFiles: ["./src/test/setup.ts"],
    env: { VITE_API_BASE_URL: "https://api.test.local", VITE_ENTRA_CLIENT_ID: "test-client",
           VITE_ENTRA_TENANT_ID: "test-tenant", VITE_ENTRA_API_SCOPE: "api://test/access" },
    coverage: {
      provider: "v8", reporter: ["text-summary"],
      exclude: ["src/api/generated/**", "src/test/**", "**/*.test.{ts,tsx}", "**/*.config.{ts,js}", "**/*.d.ts"],
      // PISO (não meta): ~8 pontos abaixo do medido — trava regressão sem bloquear refatoração.
      thresholds: { lines: 80, statements: 80, functions: 70, branches: 65 },
    },
  },
});
```
> **`pnpm test` já inclui cobertura e pisos** — é exatamente o que o CI roda. Não há combinação de flags
> que passe local e reprove no CI. Ajuste os pisos ao medir a suíte real do novo app.

**E2E de browser: deliberadamente não existe** no MigLoad — componente+MSW cobre o front. Adote a mesma
política até haver necessidade real (ex.: conta de teste no tenant).

---

## 12. Qualidade / gates (rodar antes de qualquer commit)

```bash
pnpm typecheck   # react-router typegen + tsc
pnpm lint        # Biome (organizeImports ordena os named imports)
pnpm test        # Vitest sem watch + pisos de cobertura — o que o CI roda
pnpm format      # aplica formatação do Biome quando o lint acusar
```

**CI** (adapte do MigLoad): drift do client Orval (Trilha A) · lint · typecheck · testes (com pisos) ·
build · gate de dependência vulnerável (CVE npm, barra em Critical/High nas deps de produção) ·
varredura de segredo. O restore do `@mig-now/ds` autentica com `GITHUB_TOKEN` (pré-requisito §1.6).

---

## 13. Checklist de rebrand (o que trocar do MigLoad)

- [ ] `<APP>` no wordmark da sidebar (dois spans coloridos), no `<title>` (`root.tsx`) e nos `alt`.
- [ ] `src/assets/brand/<app>-symbol.png` (símbolo/logo) + `public/favicon.*`.
- [ ] `name` do `package.json` → `<app>-front`; chave do `orval.config.ts` → `<app>`/`<app>Zod`.
- [ ] Chave de storage do circuit breaker → `<app>.autologin.attempts`; e-mail do mock → `dev@<app>.local`.
- [ ] Novo **App Registration** (SPA) no Entra: Client ID, Tenant ID, redirect URIs, post-logout `/login`.
- [ ] Itens de menu (`NAV_ITEMS`) e chaves i18n (`nav.*`) do novo domínio, nos **3** idiomas.
- [ ] `VITE_*` do `.env` (guardar valores reais fora do git; `.env.example` com placeholders).
- [ ] **Não** trocar: `@mig-now/ds`, fontes (Geometria/Phonk), degradê de marca, tokens — reaproveitados.

---

## 14. Sequência sugerida de scaffolding com Claude Code (fases)

Peça ao Claude Code para executar **uma fase por vez** e rodar os gates ao fim de cada uma. Sugestão de
prompts:

1. **Scaffold base** — "Crie um SPA React 19 + React Router v8 (framework mode, `ssr:false`, sem
   prerender) com Vite 8, TypeScript e as versões do §10 deste blueprint. Configure Biome, Tailwind v4,
   `.npmrc`/`.nvmrc`, tsconfig, `src/lib/env.ts` (Zod) e os aliases `@/`. Rode `pnpm install` e
   `pnpm typecheck`."
2. **DS + tema** — "Integre o `@mig-now/ds` (tokens/fonts), crie `src/styles/app.css` e `theme.css`
   (ponte, sem redefinir valores) e o `ThemeProvider` (fonte única da classe, script anti-FOUC no
   `root.tsx`). Dark é o default. Confira light e dark."
3. **i18n** — "Configure i18next pt-BR/en/es (namespaces `common`/`validation`/`errors`), o
   `zod-error-map`, e o teste de paridade de chaves. Nada de string hardcoded."
4. **Shell** — "Monte a hierarquia de rotas (`_protected` → `_app-shell`), a sidebar recolhível (drawer
   no mobile), a topbar (busca, tema, idioma), o `UserMenu` (nome/e-mail/logout) e a persistência
   (`theme`/`lang`/`sidebar-collapsed`). Use os componentes do §5."
5. **Auth MSAL** — "Implemente o fluxo Entra: `msal.ts` (sessionStorage + loginRedirect), o guard
   anti-loop `RequireAuth` (com circuit breaker), `use-auth`, `auth-provider`, o mock de DEV e a tela
   `/login`. **Um único** ponto de disparo de login (§8.4). Teste guard por mutação."
6. **API (escolha a trilha, §9)** — Trilha A: "Adicione Orval + `customFetch` (envelope/erro/token/
   idioma), `makeQueryClient({onUnauthorized})` ligado ao 401 global." · Trilha B: "Configure MSW para
   dev e testes, mantendo o envelope `{success,data,message}`; adie Orval para a fase 2."
7. **1ª feature de referência** — "Crie uma feature `foos/` autocontida (pages/components/hooks/schemas/
   query-keys) com uma tabela server-side (estado na URL via `useTypedSearchParams`) e um formulário
   (RHF + Zod). Teste de fluxo com `renderWithProviders` + MSW. Chaves i18n nos 3 idiomas."

A cada fase: `pnpm typecheck && pnpm lint && pnpm test`.

---

## 15. Adote os `CLAUDE.md` por pasta (a disciplina que sustenta tudo)

O MigLoad mantém um `CLAUDE.md` **por pasta** — é o que mantém o Claude Code (e o dev) dentro das
regras. Recrie a mesma estrutura, adaptando o texto ao novo domínio:

| Pasta | O que o `CLAUDE.md` fixa |
|---|---|
| raiz do front | Stack, i18n como regra global, política de testes, gates, padrões, segredos. |
| `app/` | Hierarquia de rotas protegida, `root.tsx` (anti-FOUC, 401), shell. |
| `src/api/` | Client Orval, mutator, envelope, 401 global, "generated não editar". |
| `src/auth/` | MSAL/Entra, auto-login, **anti-loop**, token na API, mock de DEV. |
| `src/components/` | `ui/` (shadcn local) e `layout/` (shell); persistência; "não editar node_modules". |
| `src/features/` | Organização de pasta, hooks de dados, estado na URL. |
| `src/i18n/` | Namespaces, chaves planas, Zod error map, gate de paridade. |
| `src/lib/` | Utilitários puros, `env` por Zod, sem texto de UI. |
| `src/styles/` | Ponte de tokens, tema por classe, **gotchas de CSS** (`.field-focus` sem `@layer`). |
| `src/test/` | `renderWithProviders`, MSW, factories, MSAL fake. |

> **Companion opcional:** se o time vier de Angular, o MigLoad tem um guia de transição React
> (`docs/onboarding-react.md`) que vale reaproveitar.
```

