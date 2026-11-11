# atc-back — API do MigATC (.NET 10)

Contrato: [`../docs/APP_ATC_DESENHO.md`](../docs/APP_ATC_DESENHO.md) (fonte da verdade de nomes, entidades e rotas) ·
decisões: [`../roadmap/app-atc-web.md`](../roadmap/app-atc-web.md) · padrões: [`../blueprint/`](../blueprint/).
Base copiada e adaptada do `mignow_migfix/migfix-back` (decisão 22). Comportamento = **porte fiel dos scripts das
skills** (`.claude/skills/*`): na dúvida, o script manda. Não inventar regra, coluna, status nem número.

## Projetos e dependências (NetArchTest guarda — `tests/.../Architecture`)

| Projeto | Depende de | Papel |
|---|---|---|
| `Mignow.Atc.Domain` | — | Entidades ricas por módulo (`Registry/`, `Execution/`, `Extraction/`, `Remediation/`, `Exemption/`, `Scan/`), factories `Create`, `DomainException`, records das colunas jsonb |
| `Mignow.Atc.Application` | Domain | CQRS (`Features/<Área>/<Caso>/`), `Dtos/`, portas (`Ports/`), **motor** (`Engine/`), `Resources/*.resx`, `Services/` (`SapConnections`, `SapWriteGuard`) |
| `Mignow.Atc.Infra.Database` | Application | EF Core + Npgsql (schema `public`), `Mappings/<Módulo>Mappings.cs`, `EfDataStore`, fila `SKIP LOCKED`, cofre AES-GCM, migration `Initial`, seed |
| `Mignow.Atc.Infra.External` | Application | `Sap/` (ADT + simulador + roteador `SapPortProxy`), `Ai/` (Anthropic + Fake), `Knowledge/` (KB – esqueleto), `Identity/`, `Time/` |
| `Mignow.Atc.Infra.IoC` | todos | `AddDiConfiguration` (Scrutor: Validação → Transação → Handler; pipelines/passos por varredura) + `<Módulo>Module.cs` |
| `Mignow.Atc.Api` | Application, IoC | `Program.cs` único (Serilog, Swagger, Entra/dev, CORS, i18n, migrate+seed, SignalR `/hubs/runs`, worker), controllers v1 (uma tag Swagger cada) |

## Regras de código

- **Sem MediatR**: handler injetado no controller (`[FromServices]`). Proibido `SaveChanges` em handler (commit do
  `TransactionCommandHandlerDecorator`) e validar no handler (FluentValidation pelo decorator).
- **Dados**: a Application usa `IDataStore` (sem EF). No motor, todo banco passa por `RunContext.WithDbAsync`/`MutateAsync`.
- **Mensagens**: código estável + texto em `.resx` nos 3 idiomas (en-US, pt-BR, es-ES). `IStringLocalizer<Errors>` é o
  `ErrorText`, que procura o código no núcleo e em todos os `*Errors.resx` de módulo. Eventos/passos guardam código + args.
- **Papéis**: só Consultor = qualquer autenticado (decisão 20). Sem `AdminOnly`. Dev sem Entra: `dev@migatc.local`.
- **SAP**: leitura e SQL **stateless**. **Escrita só pelo `ISapWriter`** (ou porta de módulo que chame
  `GuardWriteAsync`/`GuardWrite` antes): `IsProduction` no cadastro, `T000-CCCATEGORY = P` ou categoria ilegível
  bloqueiam (porte de `Enable-AdtEscrita`). LOCK → PUT (`corrNr`) → UNLOCK em `finally`, sem cancelamento entre lock e
  unlock; ativação stateless com `?context=` para include. Resultados com os códigos do `aplicar_adt.ps1`.
- **IA**: `IAiGateway.CompleteAsync<T>(AiRequest, onCall)` — `onCall` registra TODA chamada (inclusive falhas); no
  motor use `u => ctx.AddAiUsage(u, req.Usage, itemRef)`. **Um só modelo para tudo** (decisão 23): o gateway usa
  o único provedor padrão do cadastro (seed: Claude Opus 5.5); `AiUsage` (Mechanical · Review · Analysis) é só a finalidade
  da chamada (esforço, teto de saída e registro em `RunAiCall`). Fake é o padrão em Development/Testing.
- Um tipo público por arquivo, `sealed`, primary constructors; pt-BR em comentários; código em inglês.

## Motor (Application/Engine)

`Run` genérico (`Kind`, `SubjectId`, `ParametersJson`, `ResultJson`, passos com `ProgressDone/Total`, eventos, chamadas
de IA, contadores). Fila Postgres (`PostgresRunQueue`, SKIP LOCKED, **justa por cliente**: a vaga vai primeiro ao cliente com
menos execuções rodando, depois à mais antiga) → `RunWorker` (hospedado, até `Engine:MaxConcurrentRuns` = 6 simultâneas) → `RunProcessor` →
passos do `IRunPipeline` do `Kind`. **Uma execução ativa por ambiente**: `RunLock` (409 com o run que bloqueia em
`data`) + índice único parcial `UX_RUN_ACTIVE_PER_SAP_SYSTEM`. Cancelamento em ponto seguro
(`ctx.IsCancellationRequestedAsync()` entre itens → `StepOutcome.Cancel`), heartbeat + `RunRecovery`, retry a partir do
passo que falhou (passos idempotentes), SignalR `RunChanged`/`StepChanged`. Exemplo funcional: `ConnectionTest`
(`Engine/Pipelines/ConnectionTestPipeline.cs` + `Engine/Steps/TestConnectionStep.cs`).

## Como um módulo se pluga (sem editar arquivo compartilhado)

1. **Porta SAP nova**: interface em `Application/Ports/Sap/<Módulo>/I<Algo>.cs` (todo método recebe `SapConnection` e
   devolve `Task<Result>`/`Task<Result<T>>`). Implemente nos partials já criados `Infra.External/Sap/Adt/AdtSap.<Módulo>.cs`
   e `Sap/Simulator/SimulatedSap.<Módulo>.cs` (acrescente a interface na declaração `partial` do arquivo do módulo).
   O registro é automático (`SapServiceCollectionExtensions.SapPorts()` lê as interfaces do `AdtSap` em `Ports.Sap*`)
   e o `SapPortProxy` roteia `sim://` → simulador, resto → ADT. O teste de arquitetura exige paridade ADT × simulador.
   Escrita: comece com `await GuardWriteAsync(c, ct)` (ADT) / `GuardWrite(s, c)` (simulador). Estado extra do
   simulador: partials `SapScenario.<Módulo>.cs` / `SimulatedSapState.<Módulo>.cs`.
2. **Execução**: `RunKind` já tem todos os tipos (§3.7). Crie `sealed class XPipeline : IRunPipeline` e os `IRunStep`
   (sealed, idempotentes) — registrados por varredura (um pipeline por `Kind`; duplicado falha na partida). Enfileire
   pelo `RunStarter.EnqueueAsync(kind, system, subjectId, parametros, ct)` num command handler e devolva 202.
3. **CQRS**: `Features/<Área>/<Caso>/` (Command|Query + Validator + Handler) — registro automático. Controller novo em
   `Api/Controllers/<Área>Controller.cs` com `[Tags("<Área>")]` (o front gera o client Orval por tag).
4. **Mensagens**: só no `Resources/<Módulo>Errors*.resx` do módulo (3 idiomas). O `ErrorText` acha o código.
5. **Registro manual** (o que a varredura não cobre): só em `Infra.IoC/<Módulo>Module.cs` (já chamado no `AddDiConfiguration`).
6. **IA de desenvolvimento**: `Infra.External/Ai/Fake/<Módulo>FakeResponder.cs` (`IFakeAiResponder`, por `Purpose`).
7. **Banco**: entidades do módulo já existem em `Domain/<Módulo>/` com mapeamento em `Mappings/<Módulo>Mappings.cs`.
   **Sem migrations por módulo**: mudança de schema = uma migration nova, coordenada (`dotnet ef migrations add <Nome>`).
8. **Golden**: entradas sintéticas + saída do script original (`python3 -I`) em `tests/Mignow.Atc.Tests/Golden/<módulo>/`.

Portas/esqueletos já criados para os módulos: `IKnowledgeBase` (`Ports/Knowledge`, stub `MarkdownKnowledgeBase` que
lança `NotImplementedException`), `AdtSap.{Extraction,Exemption,Scan,Ddic}.cs`, `SimulatedSap.{…}.cs`,
`{Extraction,Remediation,Exemption,Scan,Report,Knowledge}Module.cs`, `{…}Errors*.resx`.

## Rodar

```bash
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=MIGNOW_ATC_DEV;Username=postgres" --project src/Mignow.Atc.Api
dotnet user-secrets set "Security:SecretKey" "$(openssl rand -base64 32)" --project src/Mignow.Atc.Api
dotnet run --project src/Mignow.Atc.Api --launch-profile https   # https://localhost:7091 – migra + seed (cliente DEMO, sim://migracao-tipica)
```

- Postgres.app 17 local, sem Docker: testes de integração no banco `MIGNOW_ATC_TEST` (criado se faltar; sobreponível
  por `ATC_TEST_DB`), schema `public` recriado a cada execução da suíte.
- `dotnet test --filter "Category!=Integration"` = loop rápido; `dotnet test` = tudo.
- Migration: `dotnet ef migrations add <Nome> --project src/Mignow.Atc.Infra.Database --startup-project src/Mignow.Atc.Api --output-dir Migrations`.
- Swagger: `https://localhost:7091/swagger` · contrato salvo em `openapi/atc.json` (regere após mudar a API).
- CORS de desenvolvimento: `http://localhost:5173` e `http://localhost:5180` (front MigATC).
- Ambiente `sim://<cenario>` = simulador (`sap-scenarios/<cenario>.json`; `clientCategory` controla a T000).

## Contratos e limites (integração)

- **AllColumnsJson** (`AtcFinding`): objeto JSON nome da coluna do CSV total → valor (texto). Todo leitor usa `ExtractionFiles.ParseRow`
  (chaves sem diferenciar maiúsculas; formato ilegível = sem colunas). Teste: `JourneyE2ETests`.
- **409 de ambiente ocupado** (`Run.EnvironmentBusy`, `RunStarter`/`RunLock`): ProblemDetails com `detail` = código, `data` = o run que
  bloqueia (RunResponse) e, direto, `blockingRunId` e `blockingRunKind`. A corrida pega só pelo índice único do banco (`ConflictException`) vem sem eles.
- **ADT**: `Sap:Adt:TimeoutSeconds` (180) vale para toda chamada; `Sap:Adt:LongTimeoutSeconds` (900) só para o classrun do lote da varredura
  MIGNOW (`AdtSession.LongTimeout`). `Sap:Adt:MaxConcurrencyPerSystem` (padrão 10): cada processo paralelo da varredura segura um espaço até
  acabar, então o paralelismo da varredura é limitado a `MaxConcurrencyPerSystem - 1` (um espaço reservado) — recusado no PUT `v1/settings`
  e no `execute`/`simulate` (`Settings.MignowParallelAboveAdtLimit`) e reduzido na execução.
- **Configurações**: faixas (`[Range]` no Swagger e FluentValidation) — Parallelism/SourceReadParallelism 1–16, MignowParallel 1–9, BatchSize,
  MignowWeightPerBatch e ManualObjectsPerDay ≥ 1, HoursPerDay 1–24.
- **Códigos para o front**: `openapi/step-codes.json` ({passo: [tipos de run]}) e `openapi/message-codes.json` ({código: texto pt-BR}) são
  gerados/conferidos por `CodesContractTests` (regerar: `ATC_WRITE_OPENAPI=1 dotnet test --filter Category=Contract`); todo código usado no código-fonte precisa
  existir nos 3 `.resx`.
- `GET v1/runs?status=Failed&status=Cancelled` (lista); `PUT v1/sessions/{id}/auto-mode {enabled}`.
