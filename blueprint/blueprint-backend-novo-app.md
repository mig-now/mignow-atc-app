# Blueprint de Backend — novo app nos moldes do MigLoad

> **Documento par do [Blueprint de Frontend](./blueprint-frontend-novo-app.md).** Juntos formam o **kit
> de handoff** para o time que vai iniciar um novo app da Mignow com **Claude Code**, reaproveitando a
> stack, a arquitetura, os padrões e os contratos do **MigLoad**. Leia o §0 dos dois; o **contrato
> back↔front** (envelope, ProblemDetails, `Accept-Language`, geração do client) está em §17 aqui e é
> referenciado pela camada de API do front.
>
> **Para quem é isto:** o dev de backend + o Claude Code. É (1) o **onboarding** de tudo que o dev
> instala/configura no PC e (2) o **spec** que o Claude Code segue para andaimar a API.
>
> **Idioma do time:** português (Brasil). Commits, comentários e docs em **pt-BR**; nomes de código
> (classes, métodos, tabelas) em inglês técnico do ecossistema .NET.
>
> **Fonte:** todos os trechos são **fiéis ao MigLoad em produção** (repo `mig-now/mignow-load`, pasta
> `mignow-load-back/`). Onde aparece o nome do app, use os placeholders do §0.

---

## 0. Como usar este documento

### Placeholders (compartilhados com o front)

| Placeholder | Significa | Exemplo |
|---|---|---|
| `<APP>` | Nome de exibição do app | `MigFoo` |
| `<app>` | Slug kebab-case | `migfoo` |
| `<ORG>` | Org do GitHub (feeds privados) | `mig-now` |
| `<Root>` | Raiz do namespace/solução .NET | `Mignow.Foo` |
| `<DB>` | Nome do banco (por ambiente) | `MIG_FOO_DEV` |

### Módulos **opcionais** (específicos do MigLoad — inclua só se o domínio pedir)

O núcleo (Clean Architecture + CQRS + `Result`/envelope + EF Core + auth Entra + testes) é a **base**.
Estes módulos são específicos do domínio do MigLoad e ficam **opt-in**:

- **Integração SAP (NCo 3.1):** o novo app pode não falar com SAP. O **padrão** Ports & Adapters
  permanece (§11); só a implementação SAP é substituída pela integração externa do novo app (ou por
  nenhuma). O pacote interno `Mignow.Sap.NCo` e o alvo **x64** só entram se houver SAP.
- **Multi-tenancy schema-por-cliente + tabelas espelho (mirror):** só se o novo app precisar isolar
  dados por cliente e ingerir alto volume. Caso contrário, um único schema `public` basta (§9).

### Como conduzir com o Claude Code

1. Onboarding do PC (§1) — uma vez por dev.
2. Claude Code anda em **fases** (§18): solução em camadas → Domain → Application (CQRS+Result) →
   API (envelope+erro+auth) → EF/migrations/seed → i18n → testes → 1ª feature.
3. **Adote os `CLAUDE.md` por projeto** (§19) — é o que mantém o Claude Code (e o dev) nas regras.

---

## 1. Onboarding do dev — instalar/configurar no PC (uma vez)

Ambiente de referência: **Windows 11**.

### 1.1 .NET SDK (versão fixada no `global.json`)

O SDK é **pinado** no `global.json` (hoje **10.0.301**, `rollForward: latestFeature`). Instale essa
major do SDK (não use RC).

```powershell
winget install Microsoft.DotNet.SDK.10   # ou o instalador do site
dotnet --version                          # deve casar com o global.json (10.0.3xx)
dotnet tool install --global dotnet-ef    # migrations do EF Core
```

### 1.2 Docker + PostgreSQL de dev

```bash
docker run --name <app>-pg -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=<DB> -p 5432:5432 -d postgres
```
> Docker também é exigido pelos **testes de integração** (Testcontainers). ⚠️ Em máquinas com **Docker
> Engine 29.x**, o reaper **Ryuk** do Testcontainers precisa ficar **desabilitado** (`TESTCONTAINERS_RYUK_DISABLED=true`
> como default sobreponível) — o CI o reabilita via variável.

### 1.3 PAT do GitHub (`read:packages`) — **só se usar pacote interno (ex.: SAP NCo)**

Se o novo app consumir um **NuGet interno da org** (como o `Mignow.Sap.NCo`), use o **mesmo PAT
`read:packages`** do front (§1.2 do blueprint de front). O `nuget.config` do projeto (commitado, sem
segredo) roteia `Mignow.*` para o feed; a credencial fica no perfil de cada dev:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/<ORG>/index.json" \
  --name <org>-github --username SEU_USUARIO_GITHUB --password ghp_SEU_TOKEN \
  --store-password-in-clear-text
```
> Sem SAP/pacote interno, **pule** este passo — o restore usa só o nuget.org público.

### 1.4 Segredos locais + certificado HTTPS

```bash
# Connection string (obrigatória — sem isto a API não sobe): mora em user-secrets, nunca no JSON.
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=<DB>;Username=postgres;Password=postgres" \
  --project <Root>.Api

dotnet dev-certs https --trust    # uma vez por máquina (o front consome a API em https)
dotnet run --project <Root>.Api --launch-profile https   # em Dev: auto-migração + seed no startup
```
> **Use o perfil `https`.** O front (`.env` → `VITE_API_BASE_URL`) aponta para `https://localhost:7079`
> (ou a porta que o novo app escolher — mantenha os dois lados alinhados). `dotnet run` sem
> `--launch-profile https` sobe em http e o front não conecta.

---

## 2. Stack (versões exatas) e porquês

| Camada | Libs | Versão | Por quê |
|---|---|---|---|
| Runtime | .NET / ASP.NET Core | **10** (SDK 10.0.301) | Plataforma. |
| Dispatch de casos de uso | **Handlers próprios** (`ICommandHandler`/`IQueryHandler`) + decorators | — | **Sem MediatR** (a v13+ virou licença comercial). Controller injeta o handler direto (`[FromServices]`); validação/transação são **decorators**. Registrar a decoração de open generics: **Scrutor** (MIT, opcional) ou factory manual (dep-free). |
| Validação | FluentValidation | 12.1.1 (Apache-2.0) | Um validator por Command/Query; roda no decorator de validação. |
| ORM | EF Core + Npgsql | 10.0.x | Leitura e escrita. Dapper reservado para leitura performática pontual. |
| Log | Serilog (`Serilog.AspNetCore` + Console) | 10.0.0 | Log estruturado por request (auditoria HTTP). |
| API docs | Swashbuckle.AspNetCore | 10.2.3 | Swagger/OpenAPI (fonte do client Orval do front). |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.11 | JWT do Entra ID. |
| Testes | xUnit + FluentAssertions + Testcontainers + **NetArchTest** | — | Unidade (EF InMemory) + integração (Postgres efêmero) + guarda de arquitetura. |
| (Opcional SAP) | `Mignow.Sap.NCo` | interno | NCo 3.1; exige alvo **x64**; só neste projeto de Infra.External. |
| (Opcional XLSX) | ClosedXML / DocumentFormat.OpenXml | 0.105 / 3.1 | Geração/leitura de planilhas (Mass Change do MigLoad). |

---

## 3. Arquitetura em camadas + regras de dependência (OBRIGATÓRIAS)

Clean Architecture. **Sem atalhos entre camadas** — guardado por **NetArchTest**.

| Camada | Projeto | Depende de |
|---|---|---|
| API | `<Root>.Api` | Application, Infra.IoC *(só composition root)* |
| Application | `<Root>.Application` | Domain |
| Domain | `<Root>.Domain` | — (nada) |
| Infra.Database | `<Root>.Infra.Database` | Application, Domain |
| Infra.External | `<Root>.Infra.External` | Application, Domain |
| Infra.IoC | `<Root>.Infra.IoC` | Application, Domain, Infra.Database, Infra.External |

**Testes de arquitetura** (`Tests/Architecture/ArchitectureTests.cs`) travam o build se violar:
- Domain não toca EF/Npgsql/FluentValidation/(SAP).
- Application não toca infra (EF/Npgsql/HttpContext).
- Controllers não tocam `Infra.*` (só Application + Infra.IoC no bootstrap).
- Todo Command é um `ICommand<T>` (marcador transacional); **nenhum handler chama `SaveChanges`** (o
  commit é do `TransactionDecorator`). Todo `ICommandHandler`/`IQueryHandler` tem exatamente um handler.

Solution (`.slnx`) organizada em solution folders por camada (`1 - Presentation` … `5 - Tests`).

---

## 4. Estrutura de projetos e pastas

```
<Root>.slnx
  <Root>.Domain/            # entidades ricas, enums, VOs, EntityBase, DomainException  (sem deps)
    Entities/  Enums/  ValueObjects/  Common/
  <Root>.Application/       # casos de uso (CQRS), DTOs, ports, validações, i18n
    Features/<Área>/<CasoDeUso>/   # Command|Query + Handler + Validator por pasta
    Dtos/<Área>/                   # *Response + mapeamentos (separados das Features)
    Abstractions/                  # ICommand/IQuery + ICommandHandler/IQueryHandler + decorators (validação/transação)
    Common/                        # Result/Error, ApiResponse/PagedResult, atores de sistema
    Ports/<Preocupação>/           # Persistence/ Identity/ External/  (abstrações)
    Resources/                     # i18n (.resx en-US/pt-BR/es-ES + classes marker)
    Services/                      # serviços de aplicação que NÃO são handlers
    Exceptions/                    # exceções públicas reutilizáveis entre handlers
  <Root>.Infra.Database/   # EF Core: DbContext, Mappings, Migrations, Seeding, Interceptors, Repositories
  <Root>.Infra.External/   # adapters das Ports: (SAP opcional), Identity (Entra), DateTime
  <Root>.Infra.IoC/        # composition root: AddDiConfiguration(...)
  <Root>.Api/              # controllers v1, middlewares, Program.cs, Contracts/<Área>/
  <Root>.Tests/            # unidade + integração + arquitetura
```

**Padrões de código:** namespace **espelha a pasta**; **um tipo público por arquivo**; classes `sealed`
por padrão; **primary constructors**; imutabilidade; sem God Classes. Um novo caso de uso = **nova
subpasta** em `Features/<Área>/` (nunca Command/Handler/Validator soltos).

---

## 5. CQRS sem mediator — handlers diretos + decorators

> **Decisão de arquitetura (novos apps): sem MediatR.** A v13+ do MediatR virou **licença comercial**;
> para não depender disso, os novos apps **não usam lib de mediator**. Cada caso de uso é um **Command
> (escrita)** ou **Query (leitura)** + **um Handler** + **um Validator**, com **interfaces próprias**. O
> controller injeta o handler **direto** (via `[FromServices]`, §7.1) e o antigo pipeline vira
> **decorators** (validação/transação) registrados na DI. O shape do trio e as regras (`Result`, sem
> `SaveChanges` no handler, sem validar no handler) **não mudam**.

### 5.1 Interfaces de handler (dependency-free)

```csharp
// Application/Abstractions/Handlers.cs
public interface ICommand<TResult>;   // ESCRITA — transacional por definição (é o "marcador"; some o ITransactionalRequest)
public interface IQuery<TResult>;     // LEITURA — sem transação

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{ Task<TResult> Handle(TCommand command, CancellationToken ct); }

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{ Task<TResult> Handle(TQuery query, CancellationToken ct); }
```
Um Command **sempre** retorna `Result`/`Result<T>` — é o canal de "não-commit" sem lançar exceção. **Ser
`ICommand<T>` já é o marcador transacional** (o `TransactionDecorator` envolve todo `ICommandHandler`);
queries (`IQuery<T>`) recebem só validação.

### 5.2 O trio (exemplo — criar entidade)

```csharp
// Features/Foos/Create/CreateFooCommand.cs
public sealed record CreateFooCommand(string Name) : ICommand<Result<FooResponse>>;

// Features/Foos/Create/CreateFooCommandValidator.cs  (FluentValidation — inalterado)
public sealed class CreateFooCommandValidator : AbstractValidator<CreateFooCommand>
{
    public CreateFooCommandValidator() => RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
}

// Features/Foos/Create/CreateFooHandler.cs
public sealed class CreateFooHandler(IFooRepository repository, IStringLocalizer<Errors> localizer)
    : ICommandHandler<CreateFooCommand, Result<FooResponse>>
{
    public async Task<Result<FooResponse>> Handle(CreateFooCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByNameAsync(command.Name, null, ct))
            return Result.Failure<FooResponse>(
                Error.Conflict("Foo.NameAlreadyExists", localizer["Foo.NameAlreadyExists"]));

        var foo = Foo.Create(command.Name);   // factory rica valida invariantes (§8)
        await repository.AddAsync(foo, ct);
        return Result.Success(foo.ToResponse());   // commit é do TransactionDecorator — NÃO chamar SaveChanges
    }
}
```
**Regras:** Command/Query são `record` só com dados; Handler `sealed`, um por caso de uso, assíncrono com
`CancellationToken`; **proibido validar no handler**; **proibido `SaveChanges` no handler**.

### 5.3 `Result` / `Error` (fluxo de negócio sem exceção)

```csharp
// Common/Error.cs — categoria mapeada para HTTP na API
public enum ErrorType { Failure, Validation, NotFound, Conflict, Upstream }
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error NotFound(string code, string msg)  => new(code, msg, ErrorType.NotFound);
    public static Error Conflict(string code, string msg)  => new(code, msg, ErrorType.Conflict);
    public static Error Upstream(string code, string msg)  => new(code, msg, ErrorType.Upstream); // → HTTP 502
    public static Error Failure(string code, string msg)   => new(code, msg, ErrorType.Failure);  // → HTTP 500
}
```
`Result`/`Result<T>` (`Common/Result.cs`): `Success()` / `Failure(Error)`; `Result<T>` tem `Value`
(lança se acessado em falha) e conversão implícita de `T` → `Result<T>`. **O `Code` é a chave estável
de domínio** (i18n/telemetria) e vai para o `detail` do ProblemDetails — **é o que o front lê** (§17).

### 5.4 Decorators (o antigo pipeline, sem MediatR)

Ordem — **Command:** `Validation` (externo) → `Transaction` → `Handler`; **Query:** `Validation` →
`Handler`. Input inválido nunca abre transação; falha de negócio (`Result.IsFailure`) não commita.

```csharp
// Application/Abstractions/ValidationCommandHandlerDecorator.cs
public sealed class ValidationCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner, IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    public async Task<TResult> Handle(TCommand command, CancellationToken ct)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TCommand>(command);
            var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors).Where(f => f is not null).ToArray();
            if (failures.Length != 0) throw new ValidationException(failures);  // → 400 no middleware (§7.2)
        }
        return await inner.Handle(command, ct);
    }
}

// Application/Abstractions/TransactionCommandHandlerDecorator.cs
public sealed class TransactionCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner, IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    public async Task<TResult> Handle(TCommand command, CancellationToken ct)
    {
        var response = await inner.Handle(command, ct);
        if (response is Result { IsFailure: true }) return response;   // falha de negócio → não commita
        await unitOfWork.SaveChangesAsync(ct);                          // sucesso → um commit atômico de N agregados
        return response;
    }
}
```
Um **`ValidationQueryHandlerDecorator<TQuery, TResult>`** análogo cobre a validação de queries (sem
transação). Registro por convenção (adicionar feature = só escrever o trio, sem editar DI) em §15.4.

---

## 6. Envelope de resposta — o contrato com o front

**Sucesso** sai no envelope `ApiResponse<T>`; **erro** sai como `ProblemDetails` (RFC 9457). O front
desembrulha `data` e lança `ApiError { status, title, code }` a partir do ProblemDetails (§17).

```csharp
// Common/ApiResponse.cs — envelope de SUCESSO (erros usam ProblemDetails, não este envelope)
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public static ApiResponse<T> Ok(T data, string? message = null) => new() { Success = true, Data = data, Message = message };
}

// Common/PagedResult.cs — listagens
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total);
```
> **Mantenha este shape `{ success, data, message }` idêntico** — o `customFetch` e os testes (MSW) do
> front dependem dele. Mudar o envelope quebra o front silenciosamente.

---

## 7. Camada API — controller fino, erros centralizados, `Program.cs`

### 7.1 Controller fino + documentação de resposta

- Em `Controllers/v1`; rota `v1/[controller]` (**sem** prefixo `api/`). Identificador externo é sempre
  o `Guid`. **Sem** regra de negócio, **sem** `try/catch` (o middleware trata).
- **Injeta o handler direto** por action, via **`[FromServices]`** — mantém o construtor do controller
  limpo e cada action declara exatamente o handler que usa (sem lib de mediator, sem construtor gordo).
- Retorna `ApiResponse<T>` (sucesso) ou ProblemDetails via `ApiControllerBase.HandleFailure(error)`.
- **Documente todas as respostas** com `[ProducesResponseType<T>(StatusCodes.Status…)]` — zero
  "Undocumented" no Swagger (um teste de integração trava). Sucesso = `ApiResponse<T>`; validação =
  `ValidationProblemDetails`; demais = `ProblemDetails`. `500` e `401` na **classe**.

```csharp
// Controllers/v1/FoosController.cs (essência — sem IMediator; handler por action via [FromServices])
[ApiController]
[Route("v1/[controller]")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class FoosController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<ApiResponse<FooResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<FooResponse>>> Create(
        CreateFooRequest request,
        [FromServices] ICommandHandler<CreateFooCommand, Result<FooResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreateFooCommand(request.Name), ct);   // decorators já envolvem o handler
        return result.IsFailure ? HandleFailure(result.Error)
            : CreatedAtAction(nameof(GetByGuid), new { guid = result.Value.Id }, ApiResponse<FooResponse>.Ok(result.Value));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult<FooResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<FooResponse>>>> List(
        [FromServices] IQueryHandler<ListFoosQuery, Result<PagedResult<FooResponse>>> handler,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null, [FromQuery] bool descending = false, CancellationToken ct = default)
    {
        var result = await handler.Handle(new ListFoosQuery(page, pageSize, search, sortBy, descending), ct);
        return result.IsSuccess ? Ok(ApiResponse<PagedResult<FooResponse>>.Ok(result.Value)) : HandleFailure(result.Error);
    }
    // GetByGuid, Update (PUT), Deactivate (DELETE→204)…  Contratos de entrada em Contracts/<Área>/*Request.cs
}
```

```csharp
// Controllers/ApiControllerBase.cs — Error.Type → status HTTP
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult HandleFailure(Error error) => Problem(
        title: error.Message,                        // mensagem já localizada
        statusCode: error.Type switch {
            ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400,
            ErrorType.Upstream => 502, _ => 400 },
        detail: error.Code);                          // code = chave estável (o front lê como `code`)
}
```

### 7.2 Middleware de erro (exceções → ProblemDetails)

`ExceptionHandlingMiddleware` centraliza: `ValidationException` → 400 (erros por campo);
`ConflictException` → 409; (opcional) `TenantNotResolvedException` → 404/409; `OperationCanceledException`
(cliente desistiu) → log debug, sem erro; demais → 500. Sempre `application/problem+json`, título
**localizado** via `IStringLocalizer`. **Nunca logar token.**

### 7.3 `Program.cs` — bootstrap (auth por ambiente, CORS, i18n, Serilog, migrate+seed)

Pontos-chave (ver §10 para auth e §13 para segredos):

```csharp
var builder = WebApplication.CreateBuilder(args);
// Auth exigida FORA de Development/Local (mesma régua do migrator).
var authEnforced = !builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Local");

builder.Host.UseSerilog((ctx, sp, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(sp).Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(o => {
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "<Root> API", Version = "v1" });
    // Fidelidade nullable → o client Orval do front tipa campos não-anuláveis como required (não opcional).
    o.SupportNonNullableReferenceTypes(); o.NonNullableReferenceTypesAsRequired();
    /* + AddSecurityDefinition("Bearer") para colar token no Swagger UI */
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, config) => { /* Entra: Authority v2.0, MapInboundClaims=false, ValidAudiences — §10 */ });
builder.Services.AddAuthorization(o => { if (authEnforced) o.FallbackPolicy =
    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); }); // deny-by-default

builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(o => { o.SetDefaultCulture("en-US");
    o.AddSupportedCultures("en-US","pt-BR","es-ES"); o.AddSupportedUICultures("en-US","pt-BR","es-ES");
    o.ApplyCurrentCultureToResponseHeaders = true; }); // segue Accept-Language do front

// CORS config-driven: habilita só quando há origens em Cors:AllowedOrigins (o front em dev).
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length > 0) builder.Services.AddCors(o => o.AddPolicy("Front",
    p => p.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddDiConfiguration(builder.Configuration);   // composition root (Infra.IoC)
var app = builder.Build();

// Pós-Build: Entra lido aqui (os overrides da WebApplicationFactory dos testes só entram no Build()).
// Fail-fast: se authEnforced && !entraConfigured → throw (melhor falhar no deploy que subir API sem auth).

// Migration + seed em runtime (ÚNICO ponto autorizado):
await using (var scope = app.Services.CreateAsyncScope()) {
    await scope.ServiceProvider.GetRequiredService<DatabaseMigrator>().MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
}

app.UseSerilogRequestLogging(/* enriquece UserId = oid do Entra (ou ator "system") */);
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRequestLocalization();
if (!app.Environment.IsProduction()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
if (corsOrigins.Length > 0) app.UseCors("Front");
if (entraConfigured) app.UseAuthentication();   // condicional: em Dev/Local sem Entra, requests anônimos
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;   // WebApplicationFactory<Program> nos testes de integração
```

---

## 8. Domínio — entidades ricas

- **`sealed`, setters privados, construtor privado sem parâmetros** (materialização do EF) e **factory
  `Create(...)`** que valida invariantes. Comportamento na entidade (métodos de intenção: `Rename`,
  `Activate`, `MarkProvisioned`…), **não** em handlers.
- Herdam de **`EntityBase`**: `int Id` (PK técnica) + `Guid Guid` (id público, gerado pela **aplicação**
  no construtor, `ValueGeneratedNever`) + auditoria `CreatedAt/By`, `UpdatedAt/By` (só o interceptor
  escreve). Invariante violada → `DomainException`.

```csharp
// Domain/Common/EntityBase.cs
public abstract class EntityBase
{
    protected EntityBase() => Guid = Guid.NewGuid();  // id público conhecido antes do commit
    public int Id { get; private set; }
    public Guid Guid { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
}
```

---

## 9. Persistência (EF Core / Npgsql)

- **DbContext** carrega `IEntityTypeConfiguration<T>` via `ApplyConfigurationsFromAssembly` (entidade +
  mapeamento **em par**).
- **Convenção de nomes:** tabelas `TB_` **MAIÚSCULAS**, colunas `MAIÚSCULAS_COM__`. `Guid` → coluna
  `uuid` `ValueGeneratedNever` (nunca `gen_random_uuid()`). Auditoria mapeada por convenção no
  `OnModelCreating`. FKs explícitas + `DeleteBehavior.ClientNoAction`; timestamps `timestamptz`;
  unicidade de nome **case-insensitive + trim** via índice funcional `LOWER("NAME")`.
- **Migrations:** único executor `DatabaseMigrator` (auto-migra em Dev/Local; em Prod só com
  `Database:MigrateOnStartup=true`). Proibido `MigrateAsync`/`EnsureCreated` em outro lugar.
- **Seed:** runtime, idempotente (sem `HasData`); corrida entre réplicas tolerada via `ConflictException`.
- **Interceptors:** `AuditSaveChangesInterceptor` carimba `CreatedBy/UpdatedBy` (ator via `ICurrentUser`
  → `oid` do Entra → `system`); tradução de índice único (Npgsql `23505`) → `ConflictException`.
- **(Opcional) Multi-tenancy schema-por-cliente + mirror:** `TenantSchemaConnectionInterceptor`
  (`SET search_path` por conexão), `ITenantContext`, `MirrorSchemaGenerator`/`MirrorReader` (Dapper)/
  `MirrorWriter` (escrita controlada + auditoria transacional). **Inclua só se o domínio exigir** isolamento
  por cliente e alto volume; caso contrário, schema `public` único.

```bash
dotnet ef migrations add <Nome> --project <Root>.Infra.Database --startup-project <Root>.Api
dotnet ef database update      --project <Root>.Infra.Database --startup-project <Root>.Api
```

---

## 10. Autenticação — Entra ID / JWT (o outro lado do front)

Enforcement **por ambiente** (não por config): **Dev/Local** anônimo (auditoria carimba `system`);
**Staging/Prod** deny-by-default (`FallbackPolicy`), exceto `/v1/health*`. Sem `Entra:TenantId`/
`Entra:ClientId` em Staging/Prod, a API **não sobe** (fail-fast).

Config do JWT (options pattern, pós-Build): `Authority = https://login.microsoftonline.com/{tenant}/v2.0`,
**`MapInboundClaims = false`** (preserva `oid`/`name`), `ValidAudiences = [clientId, api://clientId]`.

### Roteiro de App Registration (referência)

1. **API** — *New registration* (`<APP> API`), single tenant. Em *Expose an API*: Application ID URI
   `api://<client-id>` + scope `access_as_user`.
   ⚠️ **Obrigatório:** em **Manifest**, `requestedAccessTokenVersion` = **`2`** (não `null`). O default
   emite tokens **v1** para scopes `api://…`, e o backend valida contra authority **v2.0** → todo request
   morre com **401 `invalid_token`**. Não aparece em dev (anônimo) nem nos testes (tenant fictício) — só
   no 1º ambiente com enforcement. *(Aprendido em produção.)*
2. **SPA** — *New registration* (`<APP> Front`), plataforma **Single-page application**, com **duas**
   redirect URIs por ambiente: a raiz (dev `http://localhost:5173`) **e** `/login` (dev
   `http://localhost:5173/login` — pouso do pós-logout). Em *API permissions*: adicione
   `access_as_user` + consentimento de admin.
3. **Backend** — `Entra__TenantId` + `Entra__ClientId` (o da **API**) via env (nunca commit).
4. **Frontend** — `.env`: `VITE_ENTRA_TENANT_ID`, `VITE_ENTRA_CLIENT_ID` (o da **SPA**),
   `VITE_ENTRA_API_SCOPE = api://<client-id-da-api>/access_as_user`.

Auditoria (`CreatedBy`/`UpdatedBy`) usa o claim `oid`. Contrato coberto por `AuthEnforcementTests`
(rodam offline — nenhum tenant real necessário).

---

## 11. Integrações externas — Ports & Adapters

Toda integração: **Port** em `Application/Ports/<Preocupação>/` → **adapter** em `Infra.External` →
registro em `Infra.IoC`. Handlers injetam **só a Port**. Adapters transversais: `EntraCurrentUser`
(identidade, lê `oid` do JWT), `SystemDateTimeProvider` (relógio).

**(Opcional) SAP (NCo 3.1):** só se o novo app falar com SAP. Pacote interno `Mignow.Sap.NCo` (GitHub
Packages), **só neste projeto**, build **x64** (nativos x64-only); nunca commitar as DLLs. Erros
`RfcBaseException` → `SapException` → `Result.Failure(Upstream)` → **HTTP 502** (detalhe logado
server-side; ao cliente só a mensagem localizada). Se não houver SAP, **remova** o pacote, o alvo x64 e
os adapters SAP — o padrão Port→Adapter permanece para as integrações do novo domínio.

---

## 12. i18n do backend (acompanha o front)

- Mensagens via **`IStringLocalizer<Errors>`** (`Resources/`), **sem texto hardcoded**. Culturas
  **en-US / pt-BR / es-ES** — ao adicionar chave, atualize os **3** `.resx`.
- O backend **segue o `Accept-Language`** que o front envia (o mutator do front manda o idioma ativo).
  `RequestLocalization` resolve a cultura e devolve mensagens de erro localizadas — que o front exibe.
  *(Mapeamento no front: `pt-BR→pt-BR`, `en→en-US`, `es→es-ES`.)*

---

## 13. Segredos e config por ambiente (OBRIGATÓRIO)

- **Nenhuma credencial commitada** (connection string, host/usuário/senha SAP, tokens).
  **Dev:** `dotnet user-secrets`. **Staging/Prod:** variável de ambiente (notação `__`) ou cofre.
  `appsettings.*` traz no máximo placeholders **`CHANGE_ME`** (contam como "não configurado" →
  fail-fast).
- Bancos por ambiente: `<DB>_DEV` (Dev, auto-migra) · `<DB>_QAS` (Staging, `MigrateOnStartup=true`) ·
  `<DB>` (Prod, migração **manual**). Nomes em maiúsculas exigem banco criado com o mesmo case.
- Ao adicionar qualquer config que carregue segredo, siga este padrão — **não** ponha o valor no JSON.

---

## 14. Testes (definição de pronto)

**Nenhuma feature/bugfix entra sem teste.** Por tipo:
- **Caso de uso novo:** teste de **unidade** do handler (EF InMemory) **e** do validator; se toca
  endpoint/erro novo/SQL real → **integração** (`WebApplicationFactory` + Testcontainers).
- **Guard de config/ambiente:** teste dos **dois lados** (inclusive "não faz nada"); na dúvida, **prove
  por mutação** (quebre a produção e veja o vermelho).
- **Bugfix:** primeiro o teste que reproduz (vermelho), depois o fix (verde).
- **Não exige teste:** glue de DI, tipos sem lógica, adapter externo real (SAP).
- **Arquitetura:** `NetArchTest` guarda as regras de dependência (§3).
- **E2E de browser: deliberadamente não existe** até haver Staging + conta de teste no Entra.

```bash
dotnet test --filter "Category!=Integration"                 # LOOP RÁPIDO (unidade+arquitetura, sem Docker)
dotnet test                                                  # tudo (integração exige Docker)
dotnet test --collect:"XPlat Code Coverage" --settings coverage.runsettings   # número que casa com o CI
```
> **Cobertura tem piso no CI** (fonte única no `ci-back.yml`) — trava de **regressão**, não meta. O
> `coverage.runsettings` exclui **só código gerado** (mirror/migrations). **Não amplie exclusões para
> "melhorar" o número.** Testes de integração sobem Postgres efêmero (Testcontainers) — não tocam o
> banco de dev. **Ryuk desabilitado** em Docker Engine 29.x (a variável do CI o reabilita).

---

## 15. Configs prontos para copiar

### `global.json`

```json
{ "sdk": { "rollForward": "latestFeature", "version": "10.0.301" } }
```

### `<Root>.Api.csproj` (shape)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- x64 SÓ se houver SAP NCo (nativos x64). Sem SAP, remova esta linha. -->
    <PlatformTarget>x64</PlatformTarget>
    <UserSecretsId>GERE-UM-GUID</UserSecretsId>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.11">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.11" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="10.2.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\<Root>.Application\<Root>.Application.csproj" />
    <ProjectReference Include="..\<Root>.Infra.IoC\<Root>.Infra.IoC.csproj" />
  </ItemGroup>
</Project>
```

### `<Root>.Application.csproj` (pacotes de aplicação)

```xml
<PackageReference Include="FluentValidation" Version="12.1.1" />
<!-- SEM MediatR (v13+ é licença comercial): handlers próprios + decorators. Ver §5. -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.11" />
<PackageReference Include="Microsoft.Extensions.Localization.Abstractions" Version="10.0.11" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.11" />
<!-- (Opcional, XLSX) ClosedXML 0.105.1 + DocumentFormat.OpenXml 3.1.1 -->
```
> A referência ao **Scrutor** (MIT, opcional — para registrar a decoração de open generics; §15.4) fica
> no projeto **Infra.IoC**, não aqui.

### `nuget.config` (só com pacote interno; sem segredo — a credencial é do perfil do dev)

```xml
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="<org>-github" value="https://nuget.pkg.github.com/<ORG>/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
    <packageSource key="<org>-github"><package pattern="Mignow.*" /></packageSource>
  </packageSourceMapping>
</configuration>
```

### 15.4 Composition root + registro dos handlers/decorators (`Infra.IoC/DependencyInjection.cs`)

Sem MediatR, o composition root registra os handlers por convenção (varrendo o assembly da Application)
e **envolve** cada um pelos decorators. Duas formas — escolha uma:

**Forma A — com Scrutor (MIT, recomendado pela clareza):**

```csharp
public static IServiceCollection AddDiConfiguration(this IServiceCollection services, IConfiguration configuration)
{
    var applicationAssembly = typeof(IApplicationMarker).Assembly;
    services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

    // 1) registra todo handler concreto pela sua interface (ICommandHandler<,> / IQueryHandler<,>)
    services.Scan(scan => scan.FromAssemblies(applicationAssembly)
        .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>))).AsImplementedInterfaces().WithScopedLifetime()
        .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))).AsImplementedInterfaces().WithScopedLifetime());

    // 2) decora — ORDEM IMPORTA: o ÚLTIMO Decorate é o mais EXTERNO (roda primeiro).
    //    Command: Validation (externo) → Transaction → Handler.
    services.Decorate(typeof(ICommandHandler<,>), typeof(TransactionCommandHandlerDecorator<,>));
    services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
    //    Query: Validation → Handler.
    services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationQueryHandlerDecorator<,>));

    services.AddDatabaseInfrastructure(configuration);   // cada módulo de infra expõe seu Add…Infrastructure
    services.AddExternalInfrastructure(configuration);
    return services;
}
```

**Forma B — zero dependência (sem Scrutor):** registre o mesmo encadeamento com um factory manual — para
cada handler concreto, resolva o inner e envolva com `TransactionCommandHandlerDecorator` e depois
`ValidationCommandHandlerDecorator` (via `ActivatorUtilities.CreateInstance`). ~30 linhas de reflexão no
composition root; o padrão é idêntico, só a fiação muda. Use se quiser **nenhuma** dependência externa.

> **Lifetimes:** `DbContext` → Scoped; handlers e decorators → Scoped; serviços externos → Scoped;
> stateless (relógio, tenant context) → Singleton.

### `coverage.runsettings`

Exclui **só o gerado** (`**/Migrations/*_*.cs`, `*ModelSnapshot.cs`, e o mirror se houver) +
`GeneratedCode`/`CompilerGenerated`/`ExcludeFromCodeCoverage`. Formato `cobertura`. **Não** exclua
código escrito à mão (mesmo em `Migrations/`, como o `DatabaseMigrator`).

---

## 16. CI + gates (adapte do MigLoad)

`.github/workflows/ci-back.yml`, dois jobs em paralelo:
- **Build · Testes rápidos** em `windows-latest` (plataforma-alvo do NCo, se houver SAP).
- **Testes de integração** em `ubuntu-latest` (Testcontainers precisa de containers Linux; reabilita o
  Ryuk via `TESTCONTAINERS_RYUK_DISABLED=false`).

Além de build/teste: **gate de cobertura** (piso, barra o PR), **gate de dependência vulnerável** (CVE
NuGet, barra Critical/High), **varredura de segredo**. Restore do pacote interno autentica com
`GITHUB_TOKEN` (o pacote concede acesso ao repo em *Manage Actions access*).

---

## 17. Contrato back↔front (a costura entre os dois blueprints)

| Aspecto | Backend produz | Frontend consome |
|---|---|---|
| **Sucesso** | `ApiResponse<T>` = `{ success, data, message }` | `customFetch` desembrulha e retorna `data` |
| **Erro** | `ProblemDetails` (`title` localizado, `detail` = `code` estável, `errors` por campo) | lança `ApiError { status, title, code, errors }`; 401 → `/login` global |
| **Paginação** | `PagedResult<T>` = `{ items, page, pageSize, total }` | tabelas server-side (estado na URL) |
| **Idioma** | segue `Accept-Language` (en-US/pt-BR/es-ES) | mutator envia o idioma ativo |
| **Auth** | JWT do Entra (aud = client id da API); auditoria via `oid` | MSAL adquire token e envia `Bearer` |
| **Contrato de tipos** | Swagger com nullable fiel (`SupportNonNullableReferenceTypes`) | **Orval** gera o client TS do `swagger.json` |
| **Nomes** | rota `v1/[controller]`, id externo = `Guid` | `VITE_API_BASE_URL` + `pnpm sync:api` |

**Fluxo de sincronização do client (Trilha A do front):** com a API rodando, o front roda
`pnpm sync:api` → baixa o `swagger.json` → **Orval** regenera `src/api/generated/`. Mudou um contrato no
back? Regenere no front e commite `swagger.json` + `generated/` juntos. O CI do front barra drift.

---

## 18. Sequência sugerida de scaffolding com Claude Code (fases)

Uma fase por vez; ao fim de cada uma: `dotnet build` + `dotnet test --filter "Category!=Integration"`.

1. **Solução em camadas** — "Crie a solução `.slnx` com os 6 projetos (Domain/Application/Infra.Database/
   Infra.External/Infra.IoC/Api) + Tests, as **referências de projeto** do §3 e o `global.json` (SDK
   10.0.301). Adicione o `NetArchTest` guardando as regras de dependência."
2. **Domain** — "EntityBase (Id + Guid gerado na app + auditoria), DomainException e a 1ª entidade rica
   (`Create` valida invariantes; comportamento na entidade)."
3. **Application (CQRS sem mediator + Result)** — "Sem MediatR. Crie `ICommand<T>`/`IQuery<T>` +
   `ICommandHandler`/`IQueryHandler`, os decorators de Validação e Transação (ordem: Validação →
   Transação → Handler), Result/Error/ErrorType, ApiResponse<T> e PagedResult<T>. O 1º caso de uso como
   trio (Command + Validator + Handler) retornando Result. Registro por convenção no composition root."
4. **API (envelope + erro + auth)** — "ApiControllerBase (Error→HTTP), ExceptionHandlingMiddleware
   (ProblemDetails), controller fino v1 com ProducesResponseType, e o `Program.cs` (Serilog, Swagger
   nullable-fiel, auth Entra por ambiente, CORS config-driven, RequestLocalization, migrate+seed)."
5. **Persistência** — "EF Core/Npgsql: DbContext + mapeamento (TB_/MAIÚSCULAS, Guid uuid
   ValueGeneratedNever), AuditSaveChangesInterceptor, DatabaseMigrator (único executor), seed
   idempotente. Migration inicial."
6. **i18n** — "IStringLocalizer<Errors> + .resx en-US/pt-BR/es-ES; nenhuma mensagem hardcoded."
7. **Integrações (Ports & Adapters)** — "ICurrentUser (EntraCurrentUser lê `oid`), IDateTimeProvider.
   SAP/NCo **só se o domínio exigir** (x64 + pacote interno)."
8. **Testes** — "Unidade do handler (EF InMemory) + validator; integração com WebApplicationFactory +
   Testcontainers; `AuthEnforcementTests`; `coverage.runsettings` excluindo só o gerado."

---

## 19. Adote os `CLAUDE.md` por projeto

Recrie a disciplina do MigLoad — um `CLAUDE.md` por projeto, adaptado ao novo domínio:

| Projeto | O que o `CLAUDE.md` fixa |
|---|---|
| raiz do back | Camadas + regras de dependência (NetArchTest), comandos, segredos por ambiente, política de testes, padrões de código. |
| `Domain` | Entidades ricas, EntityBase, enums/VOs, DomainException. |
| `Application` | CQRS **sem mediator** (handlers próprios + decorators), Result/Error, Ports, organização de Features/Dtos, i18n. |
| `Api` | Controller fino, ProducesResponseType, ProblemDetails, auth Entra/JWT. |
| `Infra.Database` | EF Core, mapeamentos, migrations, seed, interceptors, (multi-tenancy). |
| `Infra.External` | Ports & Adapters, (SAP/NCo x64), identidade (`oid`). |
| `Infra.IoC` | Composition root, ordem dos behaviors, lifetimes. |

> **Nota "sem MediatR":** o `CLAUDE.md` da Application deve registrar a decisão — **nenhuma lib de
> mediator** (a v13+ do MediatR é licença comercial). O dispatch é por **handlers próprios**
> (`ICommandHandler`/`IQueryHandler`) injetados direto no controller (`[FromServices]`), e o pipeline é
> feito por **decorators** (Validação → Transação para commands; Validação para queries). Se usar
> **Scrutor** (MIT) para registrar a decoração, registre também que a **ordem** dos `Decorate` importa
> (o último é o mais externo).
```

