# MigATC – desenho técnico (contrato entre back e front)

Fonte das decisões: [`roadmap/app-atc-web.md`](../roadmap/app-atc-web.md) (decisões 1–22). Padrões:
[`blueprint/`](../blueprint/). Ponto de partida do código: `../../mignow_migfix` (copiar e **adaptar**, decisão 22).
Comportamento: **as skills deste projeto** (`.claude/skills/*`) – o código .NET é **porte fiel** dos scripts
(`atc.py`, `converter.py`, `cruzar.py`, `gerar.py`, `listar.py`, `coletar.py`, `*.ps1`/`*.psm1`). Na dúvida, o script
manda. Não inventar regra, coluna, status nem número.

## 1. Pastas e nomes

```
mignow-atc-process-main/
  atc-back/                    Mignow.Atc.slnx (.NET 10, SDK 10.0.301)
    src/Mignow.Atc.Domain/            entidades ricas, enums, VOs (sem deps)
    src/Mignow.Atc.Application/       CQRS (Features/<Área>/<Caso>), portas (Ports/), Engine/ (motor), Modules/<Módulo>/ (lógica pura portada dos scripts)
    src/Mignow.Atc.Infra.Database/    EF Core + Npgsql (schema public), fila SKIP LOCKED, cofre AES-GCM, migrations, seed
    src/Mignow.Atc.Infra.External/    Sap/ (ADT + simulador), Ai/ (Anthropic + Fake), Knowledge/ (KB markdown), Identity/, Time/
    src/Mignow.Atc.Infra.IoC/         composition root
    src/Mignow.Atc.Api/               controllers v1, middleware, SignalR (/hubs/runs), worker hospedado; porta https 7091
    tests/Mignow.Atc.Tests/           unidade, golden (paridade com os scripts), integração (Postgres local), arquitetura
    sap-scenarios/                    cenários do simulador (sim://)
  atc-front/                   React Router v8 SPA (cópia adaptada do migfix-front), 1 client Orval (`atc`)
  docs/APP_ATC_DESENHO.md      este arquivo
```

- Exibição `MigATC`, slug `migatc`, namespace `Mignow.Atc`. Banco `MIGNOW_ATC_DEV` / testes `MIGNOW_ATC_TEST`
  (Postgres.app 17 local, `Host=localhost;Username=postgres`, sem Docker).
- pt-BR em comentários/docs; código em inglês técnico. Um tipo público por arquivo, `sealed`, primary constructors.
- Regras do blueprint e do `migfix-back/CLAUDE.md` valem: sem MediatR, sem `SaveChanges` em handler, `IDataStore`
  na Application, `Result`/`Error` com **código estável**, mensagens em `.resx` nos 3 idiomas, NetArchTest.

## 2. Papéis e auth

Só **Consultor** = qualquer usuário autenticado (decisão 20). Sem tabela de usuários/papéis, sem `AdminOnly`.
Dev sem Entra: usuário `dev@migatc.local`. Auditoria grava o e-mail/oid.

## 3. Domínio (tabelas `TB_<NOME>`, colunas MAIÚSCULAS como no migfix, ids `int`)

### 3.1 Cadastros
- **Customer**: Name, Code, Active.
- **SapSystem** (ambiente): CustomerId, Sid, Client (mandante), Host, AdtPort, Https, Language (EN padrão),
  CertThumbprint?, IsProduction (bool, bloqueia escrita), ProjectType (`Conversion`|`Upgrade`), Author (`AUT-XX`),
  DefaultRequest?, ExemptionApprover?, Username, PasswordSecretRef (cofre), Active, LastConnectionTest*.
  URL `sim://<cenario>` = simulador.
- **GeneralSettings** (linha única): Parallelism (padrão 8, máx 16), SourceReadParallelism (8), BatchSize (40),
  KbPath (padrão `../.claude/skills/atc-remediacao/KB_ATC`, relativo à raiz do repo), MignowRulesPath
  (`../.claude/skills/atc-extracao-mignow/regras/regras.json`), MignowParallel (3, máx 9), MignowWeightPerBatch (1000),
  ManualObjectsPerDay (64) e HoursPerDay (8) para o relatório.
- **AiProvider**: Kind (`Anthropic`|`Fake`), Model, ApiKeySecretRef, IsDefault, Active. **Um único padrão para todos os
  usos** (decisão 23, migration `SingleAiModel`). Seed: Claude Opus 5.5; em dev o Fake é o padrão.

### 3.2 Execução (motor genérico, adaptado do migfix)
- **Run**: Kind (ver 3.7), CustomerId, SapSystemId, SubjectId? (sessão/lote/varredura/extração alvo),
  ParametersJson, Status (`Queued`|`Running`|`Cancelling`|`Cancelled`|`Completed`|`Failed`), StartedBy, QueuedAt,
  StartedAt, FinishedAt, HeartbeatAt, FailureCode/Args, contadores de IA (tokens in/out, custo informado, chamadas),
  ResultJson (resumo do run, ex.: contagens). Steps (RunStep: Code, Status, Start/End, MessageCode/Args, Progress
  `done/total`). **Uma execução ativa por ambiente** (índice único parcial em SapSystemId para status ativos).
- **RunEvent** (log por run: Level, Code, Args, ItemRef?), **RunAiCall** (uso, modelo, tokens, custo?, ok/erro).
- Fila Postgres `FOR UPDATE SKIP LOCKED`, um `BackgroundService`, heartbeat + retomada, cancelamento em ponto seguro
  (nunca entre lock e unlock), SignalR `RunChanged/StepChanged` (grupo por runId) + polling no front.
- Pipelines registrados por `RunKind` (`IRunPipeline { RunKind Kind; IReadOnlyList<Type> StepTypes; }`); passos
  `IRunStep` idempotentes.

### 3.3 Extração (atc-extracao)
- **AtcExtraction**: SapSystemId, Series, DisplayId (EXEC_ID), Title, ExecutedAtUtc, ExtractedAt, TotalCount,
  InBaselineCount, P1/P2/P3 (total e sem baseline), Variation (vs. anterior), Situation (`Extracted`|`AlreadyExtracted`).
  Uma execução nunca é extraída duas vezes (salvo `force`).
- **AtcFinding**: ExtractionId, campos indexados (Priority, CheckTitle, MessageTitle, ObjectName, ObjectType, Package,
  RefObjectName, RefObjectType (RObT), Note, FindingId (`quickfixInfo`/markerId), InBaseline) + **AllColumnsJson**
  (todas as colunas do CSV total, na ordem). Export: CSV total, `_novos` e `_remediacao` (layout §6) idênticos aos da
  skill (`;`, UTF-8 BOM, mesmos nomes de arquivo).

### 3.4 Remediação (atc-remediacao)
- **RemediationSession**: CustomerId, SapSystemId, Source (`Extraction`|`CsvImport`|`MignowScan`), SourceRef,
  Priorities ("1" | "1,2" | "1,2,3"), Request, Author, ProjectType (copiado do ambiente), CsvColumnsJson + encoding/
  delimitador do CSV de origem, Cycle (nº), Archived (bool; reimportar arquiva a anterior = ciclo anterior, como
  `backup_sessao_*`), AutoMode (bool, "modo sem confirmação").
- **RemediationItem**: SessionId, N (linha do CSV), ObjectName, ObjectType, RefObject, RefType, Check, Message, Note,
  Text, Package, Classification, Priority, FindingId?, Origin (`ATC`|`MIGNOW`), RawRowJson (linha original completa),
  **Status** (`""` pendente | `Confirmado KB` | `Confirmado IA` | `Cancelado` | `Isenção` – textos exatos da skill),
  Obs, StatusDate, StatusHistory (lista {data, status}), TriageFlags (R3/R4/R6…), Preparation (fonte, faixas, novo
  fonte, diff, relacionados), Proposal (ver abaixo), Application (resultado, request, backup, decisão de erro,
  histórico), CorrectionSheetMd (a "ficha" `correcoes/item_NNNN_<obj>.md`).
- **SourceCache**: SapSystemId, ObjectName, IncludeName, AdtUri, Source, Hash, ReadAt (equivale a `fontes/`).
- **Proposal** (por item ou grupo): Origin (`KB`|`IA`), KbCardId?, Blocks [{before[], after[], line?, kind
  modificacao|delecao|insercao, objeto relacionado?}], MarkedBlock (texto com marcadores), NewSource, Diff, Alerts[],
  SyntaxOk/messages, Model, Tokens, Justification, Action (`corrigir`|`isencao`|`cancelar`|`pendente`), Review (parecer
  da revisão por IA antes de gravar).
- Status do item e transições seguem `marcar()` do `atc.py` (histórico só quando muda).

### 3.5 Isenção (atc-isencao)
- **ExemptionBatch**: SessionId, SapSystemId, Priorities, Approver, Reason (padrão `FPOS`), Validity (`U`),
  JustificationPrefix, Discovery (política, motivos, aprovadores – JSON), Status.
- **ExemptionRequest**: BatchId, ItemId, MarkerId, Seq, Reason, Justification, Status (`SIMULADO`|`BLOQUEADO`|
  `SEM_ESCOPO`|`JA_PEDIDO`|`PEDIDO`|`INFO`|`ERRO`), ProbablyNoScope, RequestXml, ResponseXml, Message.

### 3.6 Varredura MIGNOW (atc-extracao-mignow)
- **MignowScan**: SapSystemId, ExtractionId (CSV total base), Rules snapshot, Scope counts, Batches (nº, peso,
  objetos, classe `ZCL_MIGNOW_ATC_SCAN_n`, status, classe_removida), Summary JSON (como `resumo.json`).
- **MignowOccurrence**: ScanId, RuleId, ObjectName/Type, Include, AdtSource, Line, LineEnd, Snippet, Cross
  (`ATC_PEGOU`|`ATC_OUTRA_LINHA`|`MIGNOW_FONTE_COM_FINDING`|`MIGNOW_FONTE_SEM_FINDING`), Class (R/I/pendente).
  Export CSV P1 no layout da remediação + `Origem;Regra;Fonte ADT;Linha;Linha fim;Cruzamento;Trecho`.

### 3.7 RunKind (um pipeline cada)
`ConnectionTest`, `ExtractionCheck`, `Extraction`, `ReadSources`, `ConfirmR1`, `Propose`, `ApplySimulate`, `Apply`,
`Rollback`, `ExemptionDiscover`, `ExemptionSimulate`, `ExemptionSend`, `ExemptionCheck`, `ScanInventory`,
`ScanSimulate`, `ScanExecute`, `ScanCleanup`, `AutoRemediation` (play da remediação, decisão 24).

### 3.8 Histórico de processo
Cada Run grava tempo de relógio, **tempo líquido** (sem pausas > 10 min entre runs da mesma sessão, como o
`resultado.py`), tokens/custo por modelo, paralelismo usado. A tela de histórico por módulo e o relatório final leem
daqui (equivale aos `historico.json|md` + bloco `execucao`).

## 4. Portas (Application/Ports)

| Porta | Métodos principais (porte de) |
|---|---|
| `ISapReader` | TestConnection (discovery, T000), GetSource/objectstructure/includes (`ler_fonte`), GetDdic (`ler_ddic`), CheckSyntax(+com conteúdo) (`verificar_sintaxe`), GetAtcResult(displayId) (`extrair.ps1`), Search |
| `ISapSql` | Select(sql) via `POST /sap/bc/adt/datapreview/freestyle` **stateless** (`Invoke-AdtSql`/`Invoke-AdtSqlIn`) |
| `ISapWriter` | **Único ponto de escrita**; cada método chama `EnsureWriteAllowed` (IsProduction, `T000-CCCATEGORY=P` ou ilegível ⇒ bloqueia). WriteSource (lock→PUT corrNr→unlock em finally) + Activate (`?context=` p/ include), WriteDdic (STRU/TABL, trava de estrutura), CreateRunDeleteClass (`$TMP`), RequestExemption (POST `/atc/exemptions/apply`) |
| `IKnowledgeBase` | Lookup(ref, robt, top=3) (= `kb_lookup`), Facts(elemento), Register(contribuição, substitui?, motivo), AddFact, Browse (índices), Reload; escrita com trava de arquivo, relê antes de numerar `C-`/`F-` |
| `IAiGateway` | Complete<T>(usage, system, user, schema) → JSON validado + `onCall` (tokens/custo) |
| `ISecretStore`, `ICurrentUser`, `IDateTimeProvider`, `IDataStore`, `IRunQueue`, `IRunNotifier` | como no migfix |

## 5. Fluxos (híbrido – decisão 3)

**Extração**: `GET sap-systems/{id}/atc/latest` (ExtractionCheck síncrono ou run curto) → `POST extractions`
(run Extraction) → resumo → downloads CSV.
**Monitor de extrações** (`/extracao`, tela principal; "Nova extração" de um ambiente em `/extracao/nova`): `GET extractions/monitor?customerId=` (ambientes ativos com a última
execução `Extraction`, com passos, e a última extração gravada) → seleção de vários ambientes, de clientes diferentes →
um `POST extractions` por ambiente, em paralelo (409 fica na linha, sem parar os outros) → atualização a cada 3 s enquanto
houver execução ativa (15 s parado). O worker roda até `Engine:MaxConcurrentRuns` (padrão 6) execuções ao mesmo tempo (uma por
ambiente); o resto fica na fila, **justa por cliente** (a vaga vai primeiro ao cliente com menos execuções rodando), então
clientes diferentes rodam juntos mesmo com um lote grande de um só.

**Remediação** (sessão; cada passo pesado é um Run):
1. `POST sessions` (de extração `_novos` + prioridades, ou upload CSV, ou varredura) → itens.
2. Run `ReadSources` (paralelo, standard no fim).
3. `GET sessions/{id}/triage` (prévia das regras do tipo de projeto da sessão: R1..R8 + FSCM, R9 só em upgrade; contagens por regra) → `POST triage/apply {rules[]}` →
   Run `ConfirmR1` obrigatório depois da R1.
4. `GET sessions/{id}/groups` (agrupamento do `cmd_grupos`: kb_top, complexidade; o `modelo sugerido` do script fica na API, mas não é exibido – decisão 23).
5. Run `Propose {groupKeys|itemIds, mode: kb|ia}`: KB lookup → preparo por IA (mesmo modelo; esforço baixo p/ simples,
   médio p/ média, alto p/ complexa e para `mode: ia`) → marcadores (§7) → `preparar` (novo fonte + diff) → sintaxe sem gravar.
6. **Decisão do consultor** (síncrono): `POST items/decision {itemIds, decision: ConfirmKb|ConfirmIa|Cancel|Exemption,
   obs, substitui?, motivo?}`; `ConfirmIa` grava o cartão `C-<ELEM>-NNN` na KB (decisão 5) e fatos.
7. Run `ApplySimulate {itemIds}`: revisão por IA (Review) + simulação (`aplicar_adt -Simular`: versão ativa ≠
   preparada ⇒ `ABORTADO_FONTE_MUDOU`; backup do fonte).
8. **Consultor confirma** → Run `Apply {itemIds}` (objeto por objeto, em fila) → `ATIVADO`|`ERRO_ATIVACAO`|
   `ERRO_GRAVACAO`|`ERRO_LOCK`|`ERRO_REQUEST` (DDIC: + `ABORTADO_ESTRUTURA_TABELA`, `ERRO_VALIDACAO`,
   `ERRO_CRIACAO`, `NADA_A_FAZER`).
9. Erro de ativação → `POST items/{id}/activation-decision {keepInactive|rollback}` (rollback = Run `Rollback`).
10. `GET sessions/{id}/export` → `atc_status.csv` (CSV original + coluna `Status Remediação`, como `cmd_exportar`).
    Acréscimo do porte (10.11.2026): coluna `Aplicação SAP` (resultado da última tentativa no SAP, `ERRO_x: detalhe` nas falhas) só quando algum
    item foi ao SAP – sem aplicação o arquivo é idêntico ao da skill. "Confirmado" continua sendo só a decisão; a falha de gravação aparece na
    coluna SAP da lista de itens, no filtro `status=applyError` e em `counts.applyErrors` (coluna `APPLICATION_RESULT` do item).
    **Pendente de aprovação** (10.11.2026): pendente (status vazio, como na skill) que já tem proposta esperando o consultor – correção que não
    passou nas travas do modo automático, sugestão de isenção/cancelamento ou "sem solução" da IA (só "sem cartão na KB" não conta). Coluna
    `AWAITING_APPROVAL`, filtro `status=approval` (o oposto: `unprocessed`), `counts.awaitingApproval`, `proposalAction` na lista. Decisão
    `AcceptSuggestion` em `POST sessions/{id}/items/decision`: cada item recebe o que a proposta recomenda (isenção/cancelar/ConfirmKb/ConfirmIa),
    observação vazia = justificativa da proposta; sem solução fica como está (`skipped`). O play também reaplica confirmados com erro de
    gravação (request/lock/gravação/produção/fonte mudou/preparação), sem nova proposta; erro de ativação e rollback ficam com o consultor.
- AutoMode: Propose confirma KB sem alerta e aplica; para nas condições da skill (dúvida de negócio, cópia Z de
  standard, pré-requisito ausente, erro de ativação, tabela com conversão) e gera o relatório do lote.

**Remediação automática (play, decisão 24)**: `/remediacao` é o **monitor** (`GET sessions/monitor?customerId=&includeArchived=`:
sessões com contagens e a última execução, atualização a cada 3 s com execução ativa). Na aba Itens da sessão, os itens marcados (sem
marcação = todos os pendentes) → diálogo com as travas, ambiente e request + "confirmo" → `POST sessions/{id}/auto-remediation {itemIds?}`
(202, run `AutoRemediation`; recusa sem request, em produção, arquivada ou sem pendente). Passos: `AUTO_SOURCES_PLAN` → `READ_SOURCES_FETCH`
→ `AUTO_TRIAGE` (regras ativas do cadastro que valem para o tipo de projeto da sessão – escopo Todos + as específicas dele; R9 de fábrica só em upgrade –, sem escolha do consultor; consulta em `GET v1/triage-rules?projectType=`; baseline já fora pela sessão `_novos`) → `CONFIRM_R1_CHECK`/`CONFIRM_R1_APPLY` → `PROPOSE` (KB) →
`AUTO_PROPOSE_AI` (o que ficou sem correção) → `AUTO_REMEDIATION_CONFIRM` (KB sem alerta; IA com sintaxe ok e sem alerta, sem cartão novo
na KB) → `AUTO_SIMULATE` (backup) → `AUTO_REVIEW` → `AUTO_APPLY` (revisão que bloqueia segura o item; erro de ativação sem rollback) →
`BATCH_REPORT` → `AUTO_REMEDIATION_SUMMARY` (`result.summary`). Standard fica fora. O resultado do run acumula as chaves
(`RunContext.MergeResultAsync`). Ao fim: painel com resumo, `GET sessions/{id}/export`, histórico e relatório do lote.

**Isenção**: `POST exemption-batches {sessionId, priorities}` (= `listar.py`) → Run `ExemptionDiscover` → `PUT
params` → Run `ExemptionSimulate` → **confirmar** Run `ExemptionSend {seq?}` → Run `ExemptionCheck`.

**Varredura**: `POST scans` → Run `ScanInventory` → `PUT scans/{id}/scope` (regras ativas) → gerar lotes (síncrono,
template `abap/classe_varredura.abap.tpl`) → Run `ScanSimulate` → Run `ScanExecute` (paralelo, cria/roda/apaga;
liberado sem nova confirmação) → cruzamento com o CSV total da extração → resumo, CSV P1, `POST scans/{id}/import`
(cria sessão Origin=MIGNOW, só a pedido). Nunca altera os objetos encontrados.

**Relatório final**: `GET customers/{id}/report` (= `coletar.py` → `dados.json`: ciclos, burndown por findings e por
objetos, check × solução, manual × IA, isenção, KB, execução e custos) e `GET .../report.html?view=findings|objetos`
(HTML MIGNOW do `modelo/relatorio.html`). Análises: isenção, pendências, MIGNOW.

**KB**: `GET kb/index` (por elemento/padrão), `GET kb/elements/{cat}/{elem}` (cartões + contribuições + fatos +
substituições), `GET kb/lookup?ref=&robt=`.

## 6. Layout do CSV de remediação
`Priority;Check Title;Check Message;Object name;Obj.;Exemption State;Contact;Package;1st Found;Obj. Resp.;Changed by;Note;Short Text;Component;RObT;Referenced Object;Additional Info;Category;Category7;Text;Remarks;Finding ID`
(colunas sem origem confirmada saem vazias – ver `atc-extracao/scripts/converter.py`).

## 7. Marcadores (decisão 7 – igual ao `atc.py marker`)
```
*>>> S/4 MIGRATION MIG-NOW - START OF DELETION - DD.MM.YYYY AUT-XX
*<linha original>
*<<< S/4 MIGRATION MIG-NOW - END OF DELETION - DD.MM.YYYY AUT-XX
*>>> S/4 MIGRATION MIG-NOW - START OF INSERTION - DD.MM.YYYY AUT-XX
<linha nova>
*<<< S/4 MIGRATION MIG-NOW - END OF INSERTION - DD.MM.YYYY AUT-XX
```
Um DELETION + um INSERTION por bloco; declarações novas dentro do INSERTION; data do dia; autor do ambiente/sessão.

## 8. API (prefixo `v1/`, envelope `{success,data,message}`, ProblemDetails com `detail` = código)

| Área (tag Swagger) | Rotas |
|---|---|
| `Customers` | CRUD `customers` |
| `SapSystems` | CRUD `sap-systems`, `PUT {id}/password`, `POST {id}/test`, `GET {id}/atc/latest` |
| `Settings`, `AiProviders` | `settings` GET/PUT, CRUD `ai-providers` |
| `Runs` | `GET runs` (filtros sapSystemId, customerId, kind, activeOnly, paginação), `GET {id}`, `GET {id}/events`, `GET {id}/ai-calls`, `POST {id}/cancel`, `POST {id}/retry` |
| `Extractions` | `GET extractions`, `GET extractions/monitor`, `POST extractions`, `GET {id}`, `GET {id}/findings`, `GET {id}/files/{total\|novos\|remediacao}` |
| `Sessions` | `GET/POST sessions`, `POST sessions/import-csv` (multipart), `GET {id}`, `GET {id}/items` (filtros status, prioridade, objeto, grupo), `GET {id}/items/{itemId}`, `POST {id}/read-sources`, `GET {id}/triage`, `POST {id}/triage/apply`, `POST {id}/confirm-r1`, `GET {id}/groups`, `POST {id}/propose`, `POST {id}/items/decision`, `POST {id}/apply-simulate`, `POST {id}/apply`, `POST {id}/items/{itemId}/activation-decision`, `GET {id}/export`, `GET {id}/history` |
| `Exemptions` | `GET/POST exemption-batches`, `GET {id}`, `POST {id}/discover`, `PUT {id}/params`, `POST {id}/simulate`, `POST {id}/send`, `POST {id}/check` |
| `Scans` | `GET/POST scans`, `GET {id}`, `GET scans/rules`, `POST {id}/inventory`, `PUT {id}/scope`, `POST {id}/simulate`, `POST {id}/execute`, `POST {id}/cleanup`, `GET {id}/occurrences`, `GET {id}/files/p1`, `POST {id}/import` |
| `Reports` | `GET customers/{id}/report`, `GET customers/{id}/report.html` |
| `Knowledge` | `GET kb/index`, `GET kb/elements/{category}/{element}`, `GET kb/lookup` |
| `Me`, `Health` | `GET me`, `GET health` |

Ações que disparam Run devolvem **202** com `RunResponse`; conflito de ambiente = **409** com o run que bloqueia.

## 9. Front (`atc-front`, rotas)

`/` início (atalhos dos módulos; ambientes e execuções ficam no monitor de extração e na ⚙) · `/config/{clientes,ambientes,ia,parametros,execucoes}` · `/regras-triagem` (módulo do cadastro de triagem, `TB_TRIAGE_RULE`: escopo Todos / só upgrade / só conversão; padrão R1..R9 + produtos com lógica no código – edita código (renomeia; a chave fixa R1..R9 escolhe a lógica e fica na obs "Triagem R1: …" lida pela conferência da R1, isenção e relatório; marcações dos itens usam o código), descrição, escopo, ativa, objetos da R7/R9, parâmetros da lógica (R1 tipos/classificações, R5 notas/textos, R8 operações/tipos, produto objetos) e filtro adicional (nome do objeto, texto do check); o efeito não muda –; cada regra exibe a condição do código e a ordem de avaliação; novas por critério – objetos referenciados, nome do objeto com *, texto do check – com efeito e obs; só a nova se exclui; `GET/POST v1/triage-rules`, `PUT/DELETE v1/triage-rules/{code}`) ·
`/extracao` (monitor), `/extracao/nova` · `/remediacao` (monitor), `/remediacao/nova`, `/remediacao/:sessionId` (abas Itens · Triagem · Grupos e aprovação ·
Aplicação · Histórico; diálogo do item com diff lado a lado; aba Fontes só de consulta – objetos com fonte lido, `GET sessions/{id}/sources`, e o fonte num popup, `GET sessions/{id}/sources/{sourceId}` –, sem disparo manual: ao criar a sessão – extração, CSV ou varredura – o run `ReadSources` de
todos os pendentes, sem standard, entra na sequência e o painel "Leitura dos fontes" o acompanha (`sourcesRunId`, ou `sourcesNotStarted` com o motivo,
ex.: ambiente ocupado); Triagem só exibe as regras ativas do tipo de projeto, na ordem de avaliação, sem link para o cadastro – a triagem e a conferência da R1 rodam no play,
sem aplicação manual na tela) · `/isencao`, `/isencao/:batchId` · `/varredura`,
`/varredura/:scanId` · `/relatorio` (por cliente) · `/kb`, `/kb/:category/:element` · `/execucoes/:runId` (monitor).

i18n: namespaces por módulo (`common`, `config`, `extraction`, `remediation`, `exemption`, `scan`, `report`, `kb`,
`errors`, `validation`) nos 3 idiomas; gate de paridade.

## 10. Testes e gates
- Back: `dotnet build && dotnet test` (integração no Postgres local `MIGNOW_ATC_TEST`, categoria `Integration`).
- **Golden**: `tests/Mignow.Atc.Tests/Golden/<modulo>/` com entradas sintéticas + saída gerada **pelo script Python
  original** (`python3 -I`), e o teste .NET compara (triagem, kb_lookup, grupos, marcadores, preparar, exportar,
  converter, cruzar, coletar).
- Front: `pnpm typecheck && pnpm lint && pnpm test`.
