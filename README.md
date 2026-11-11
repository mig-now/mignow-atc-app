# MigATC – aplicação web da remediação ATC S/4HANA (MIGNOW)

Porte das skills de remediação ATC (extração, varredura MIGNOW, remediação, isenção e relatório) para uma aplicação web.

| Pasta | Conteúdo |
|---|---|
| [`atc-back/`](atc-back/) | API .NET 10 (CQRS, motor de execuções, EF Core + Postgres, ADT/simulador SAP, IA). Como rodar e regras em [`atc-back/CLAUDE.md`](atc-back/CLAUDE.md). |
| [`atc-front/`](atc-front/) | Portal React Router 8 (Vite, Tailwind, TanStack Query, Orval). Como rodar e regras em [`atc-front/CLAUDE.md`](atc-front/CLAUDE.md). |
| [`docs/APP_ATC_DESENHO.md`](docs/APP_ATC_DESENHO.md) | Contrato: entidades, rotas, telas e decisões. |
| [`roadmap/app-atc-web.md`](roadmap/app-atc-web.md) | Decisões e fases do app. |
| [`blueprint/`](blueprint/) | Padrões de back e front. |

## Rodar (resumo)

```bash
# API (https://localhost:7091) – Postgres local; connection string e chave via dotnet user-secrets
cd atc-back
dotnet tool restore
dotnet run --project src/Mignow.Atc.Api --launch-profile https

# Front (http://localhost:5173)
cd atc-front
cp .env.example .env
pnpm install
pnpm dev
```

Testes: `dotnet test` (atc-back) · `pnpm typecheck && pnpm lint && pnpm test` (atc-front).

## Segurança

Nenhuma credencial vai para o repositório: connection string e chave de criptografia ficam em `dotnet user-secrets`, senhas SAP no cofre
do banco (AES-GCM), PAT do GitHub Packages no `~/.npmrc` de cada dev. Escrita no SAP só pelo `ISapWriter`, nunca em produção.
