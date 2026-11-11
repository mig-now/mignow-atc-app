# Comece aqui — Kit de handoff (protótipo com Claude Code)

Este é o **ponto de entrada** do kit. Leia-o antes dos outros dois.

| Arquivo | O quê |
|---|---|
| **`COMECE-AQUI-kit-handoff.md`** (este) | Como conduzir o Claude Code, o que decidir antes, e o caminho rápido do protótipo. |
| **`blueprint-frontend-novo-app.md`** | Spec + onboarding do **frontend** (React/Tailwind/DS/i18n/auth/shell). |
| **`blueprint-backend-novo-app.md`** | Spec + onboarding do **backend** (.NET 10, Clean Arch, CQRS sem MediatR, Entra). |

---

## 0. A dúvida principal, respondida: vocês NÃO criam os arquivos de projeto

**O Claude Code cria `package.json`, `*.csproj`, `*.slnx`, `tsconfig`, `vite.config`, etc. — vocês não.**
Ele roda os **geradores oficiais** (`create-react-router`, `dotnet new sln`, `dotnet new webapi`…) e
depois **ajusta** os arquivos para casar com o blueprint (versões, configs, `ssr:false`, aliases…). É o
trabalho central da ferramenta.

O papel de vocês é **conduzir**: dar o objetivo de cada fase, revisar o que ele fez, rodar os "gates" e
confirmar antes de avançar. Os blueprints são a **fonte da verdade** que o Claude Code segue.

---

## 1. Antes de abrir o Claude Code

### 1.1 Instalar o Claude Code
Cada dev instala o Claude Code (CLI/desktop/IDE). Peça acesso à conta/licença ao time.

### 1.2 Pré-requisitos do PC (uma vez por dev)
Faça o **onboarding do lado que você vai tocar** — já está pronto nos blueprints:
- **Frontend:** §1 do `blueprint-frontend-novo-app.md` (Node fixado + pnpm, **PAT `read:packages`** no
  `~/.npmrc`, VS Code + extensões).
- **Backend:** §1 do `blueprint-backend-novo-app.md` (.NET SDK 10, Docker + Postgres, `dev-certs https`).

### 1.3 O que pedir ao Anselmo / Mignow (acessos)
- **PAT do GitHub com escopo `read:packages`** (o mesmo serve para o DS no front e para qualquer NuGet
  interno no back). Sem ele, o `pnpm install`/`dotnet restore` falha com 401.
- **Acesso ao pacote `@mig-now/ds`** (Design System — reaproveitado como está).
- **Para o CI depois:** o pacote `@mig-now/ds` precisa conceder acesso ao repo novo em *Manage Actions
  access* (uma vez, na UI do pacote). **Para o protótipo local não é necessário.**
- **Entra ID:** **não precisa** para começar (ver o caminho rápido, §3). Só quando forem colocar login
  real é que se criam os App Registrations (roteiro no §10 do blueprint de backend).

---

## 2. Decisões a tomar antes de começar

Anote as respostas — o Claude Code vai perguntar (ou vocês passam já no 1º prompt):

| Decisão | Opções | Sugestão para protótipo |
|---|---|---|
| **Nome do app** | `<APP>` (exibição) + `<app>` (slug) + `<Root>` (namespace .NET) | ex.: `MigFoo` / `migfoo` / `Mignow.Foo` |
| **Backend?** | tem API própria agora, ou front primeiro? | Comece pelo back **ou** use a Trilha B do front (mocks) |
| **Front ↔ API** | Trilha A (Orval no dia 1) ou Trilha B (MSW, API depois) | **B** se o back ainda não existe; **A** assim que o back subir |
| **SAP no back?** | sim / não | **não** (é módulo opcional; ligue só se o domínio pedir) |
| **Multi-tenant (schema por cliente)?** | sim / não | **não** (schema `public` único) |
| **Login real (Entra)?** | agora / depois | **depois** — protótipo roda com mock/anônimo (§3) |

---

## 3. Caminho rápido do protótipo (sem Entra, sem SAP)

Para ver algo rodando **hoje**, sem provisionar tenant nem SAP:

- **Frontend:** use o `.env` com os **placeholders `00000000-…`** (via `cp .env.example .env`). O app
  sobe com um **usuário mock de DEV** — sem tela de login, já autenticado. (Detalhe: §8 do blueprint de
  front.)
- **Backend:** rode em `Development`. A autenticação fica **anônima** (a auditoria carimba `system`), e a
  **migração + seed rodam sozinhos** no startup. Só precisa do Postgres via Docker. (Detalhe: §10 e §13
  do blueprint de back.)
- **Ligar os dois:** o front aponta `VITE_API_BASE_URL` para o back (`https://localhost:7079`); o back
  libera CORS para a origem do front em dev.

Login real com Entra é uma etapa **posterior** — não bloqueia o protótipo.

---

## 4. Passo a passo (por lado)

1. **Crie a pasta/repo** do app (um por lado, ou um monorepo `app-front/` + `app-back/` como o MigLoad).
2. **Coloque o blueprint na raiz** dessa pasta (o Claude Code lê arquivos do diretório de trabalho).
3. **Abra o Claude Code** nessa pasta.
4. **Dê o prompt de abertura** (§5) — ele gera o esqueleto e aplica o blueprint.
5. **Avance por fases** (front: §14 do blueprint; back: §18). **Uma fase por vez.**
6. **Rode os gates ao fim de cada fase** e só então confirme o avanço:
   - Front: `pnpm typecheck && pnpm lint && pnpm test`
   - Back: `dotnet build && dotnet test --filter "Category!=Integration"`
7. **Verifique de verdade:**
   - Front: o Claude Code sobe o preview no navegador e tira print/checa o console.
   - Back: `GET /v1/health` responde e o Swagger abre em `https://localhost:7079/swagger`.

---

## 5. Prompts de abertura (copiar e colar)

Troque `<APP>`/`<app>`/`<Root>` pelos valores decididos no §2.

### Frontend
```
Leia o arquivo blueprint-frontend-novo-app.md na raiz deste diretório — ele é a fonte da verdade.
Vamos criar o frontend do app <APP> (slug <app>) seguindo esse blueprint, fase por fase.

Comece pela Fase 1 (Scaffold base): gere o esqueleto de uma SPA React Router v8 em framework mode
usando o gerador oficial (você escolhe/roda o comando — eu NÃO crio package.json/tsconfig à mão) e
depois reconcilie as versões e configs EXATAMENTE com o §10 do blueprint (package.json, .npmrc, .nvmrc,
vite/react-router/tsconfig/biome/components.json, src/lib/env.ts, aliases @/). ssr:false, sem prerender.

Decisões do protótipo: Design System = reusar @mig-now/ds; camada de API = Trilha B (MSW) por enquanto.
Ao terminar, rode `pnpm install` e `pnpm typecheck`, me mostre a saída e PARE para eu confirmar antes
da Fase 2. Se algo no meu pedido conflitar com o blueprint, aponte o conflito em vez de adivinhar.
```

### Backend
```
Leia o arquivo blueprint-backend-novo-app.md na raiz deste diretório — ele é a fonte da verdade.
Vamos criar a API do app <APP> (namespace raiz <Root>) seguindo esse blueprint, fase por fase.

Comece pela Fase 1 (Solução em camadas): use `dotnet new` para criar a solução .slnx e os 6 projetos
(<Root>.Domain, .Application, .Infra.Database, .Infra.External, .Infra.IoC, .Api) + <Root>.Tests, com
as referências de projeto do §3, o global.json (SDK 10.0.301) e o NetArchTest guardando as regras de
dependência (você cria os .csproj/.slnx — eu NÃO faço isso à mão).

Decisões do protótipo: SEM SAP e SEM multi-tenant; sem MediatR (handlers próprios + decorators, §5).
Ao terminar, rode `dotnet build` e me mostre a saída, e PARE para eu confirmar antes da Fase 2.
```

---

## 6. Dicas para conduzir bem

- **Uma fase por vez, com gate no fim.** Não deixe o Claude Code "fazer tudo" de uma vez — revise e
  confirme entre as fases. É o que mantém a qualidade e evita retrabalho grande.
- **O blueprint vence.** Se uma resposta do Claude Code divergir do blueprint (versão, estrutura, regra
  de i18n/teste), aponte: *"isso contraria o §X do blueprint, ajuste"*.
- **Commit por fase.** Peça um commit ao final de cada fase (mensagem em pt-BR, Conventional Commits).
- **i18n desde o começo (front):** nenhuma string no JSX — sempre `t()`, nos 3 idiomas. É a regra que
  mais escapa; cobrada pelo gate de paridade.
- **Teste faz parte do pronto** (os dois lados). Não trate como etapa separada.
- **Copiem também os `CLAUDE.md` por pasta/projeto** (front §15 · back §19): são eles que mantêm o
  Claude Code nas regras nas próximas sessões, sem precisar repetir tudo no prompt.
- **Segredos nunca no git:** `.env` (front) e user-secrets (back). O PAT vai no `~/.npmrc`/perfil, nunca
  no repo.

---

## 7. Checklist de "pronto para começar"

- [ ] Cada dev instalou o Claude Code e fez o §1 do blueprint do seu lado.
- [ ] PAT `read:packages` no `~/.npmrc` (front) e/ou no perfil NuGet (back, se houver pacote interno).
- [ ] Acesso ao `@mig-now/ds` confirmado (o `pnpm install` do front resolve sem 401).
- [ ] Docker rodando + Postgres de dev no ar (back).
- [ ] Decisões do §2 anotadas (nome, trilha de API, SAP/multi-tenant = não, Entra = depois).
- [ ] Os **3 arquivos do kit** na raiz do repo (ou um em cada lado do monorepo).
- [ ] Prompt de abertura (§5) pronto com `<APP>`/`<app>`/`<Root>` preenchidos.
```

