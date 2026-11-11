# Aplicação web MigATC – planejamento

Registrado em 06.11.2026 · Situação: **em implementação** – todas as decisões fechadas com o consultor. Em 08.11.2026: `atc-back/` e
`atc-front/` com todos os módulos (remediação, isenção, extração, varredura, KB, relatório) e massa golden;
`dotnet test` 289/289 e `vitest` 110/110 passando. Sem registro de CI, piloto nem das validações em SAP real pedidas
nas fases F4–F7 (confirmar com o consultor).

## 1. Objetivo

Levar o processo ATC deste projeto (skills `atc-extracao`, `atc-remediacao`, `atc-isencao`, `atc-extracao-mignow`,
`atc-resultado` e `sap-access-point`) para uma **aplicação web** (frontend + backend). A aplicação:

- segue a **App 2 (MigFix ATC)** do plano `../mignow_migfix/plano-migfix.md` nas telas e no motor de execução
  assíncrono;
- segue a documentação de `blueprint/` (stack, arquitetura, padrões);
- mantém a **base de conhecimento existente (`KB_ATC`, markdown)**, sem banco vetorizado;
- onde o plano MigFix e este projeto divergem, vale o comportamento **deste projeto** (decisões na seção 2).

## 2. Decisões do consultor (06.11.2026)

| # | Assunto | Decisão |
|---|---|---|
| 1 | Onde a aplicação mora | **App nova e separada**, neste projeto (não é módulo do portal `mignow_migfix`) |
| 2 | Processamento backend | **Reescrever em .NET** a lógica das skills (`atc.py`, `converter.py`, `cruzar.py`, `listar.py`, `coletar.py`/`gerar.py` e os `.ps1`). A aplicação não chama os scripts |
| 3 | Fluxo | **Híbrido**: o worker prepara as propostas e a execução para em **Aguardando aprovação**; o consultor decide grupo a grupo, com o diff, antes de qualquer gravação |
| 4 | KB | **Markdown `KB_ATC`** indexado em memória; consulta igual ao `atc.py item` (objeto referenciado + RObT) |
| 5 | Aprendizado da KB | **Grava direto**: `Confirmado IA` vira cartão `C-<ELEM>-NNN` na hora (com a pergunta de substituição e os fatos `F-<ELEM>-NNN`), sem fila de aprovação |
| 6 | Isenção | **Incluída na app**: descobrir, simular, enviar confirmado, conferir |
| 7 | Marcador | **O da skill**: `*>>> S/4 MIGRATION MIG-NOW - START OF DELETION/INSERTION - DD.MM.YYYY AUT-XX` |
| 8 | Backup | **O da skill**: backup do fonte ativo antes de gravar e aborto se o fonte mudou; grava na request do consultor; sem request de backup; em erro de ativação, manter inativo ou rollback |
| 9 | Execução ATC | **A da skill**: última concluída das séries `ZMIG_`/`Z_MIG_`, sem baseline, nunca extraída duas vezes |
| 10 | Triagem | **A do projeto**: R1..R9 + `MIGNOW_FSCM_PRODUCT` + tipo de projeto (conversão/upgrade) |
| 11 | Critério de corrigido | **O do projeto**: revisão por IA (papel do `sap-atc-abap`) antes de cada gravação; corrigido = `ATIVADO` |
| 12 | Escopo extra | **O do projeto**: varredura MIGNOW (classe temporária em `$TMP`, liberada sempre) e relatório final (atc-resultado) entram |
| 13 | Importar CSV | **O do projeto**: importação no layout da remediação, inclusive o CSV P1 da varredura MIGNOW |
| 14 | Nome e pastas | `MigATC` / `migatc` / `Mignow.Atc`, em `atc-back/` e `atc-front/` neste projeto |
| 15 | RFC | **Fora da app**: só ADT + SQL pelo data preview do ADT. O pipeline `geracao/` da KB (RFC) continua script |
| 16 | KB compartilhada | A app lê e grava a **mesma pasta** `.claude/skills/atc-remediacao/KB_ATC` usada pelas skills (caminho configurável) |
| 17 | Dados existentes | **Importar** `clientes/<CLIENTE>/` para o banco (sessões, históricos, ciclos anteriores, isenções, extrações) |
| 18 | Skills × app | Cliente importado ou criado na app passa a ser **trabalhado só na app**; as skills ficam para os clientes que ainda não migraram |
| 19 | Credencial | Senha digitada na ⚙ e **guardada cifrada** no backend (cofre; Key Vault em produção), no lugar do `Export-Clixml`. Nunca volta pela API nem vai para log |
| 20 | Papéis | **Só Consultor** (todo usuário autenticado) |
| 21 | Design System | **Tokens provisórios** (como no migfix) até o PAT `read:packages`; depois troca pelo `@mig-now/ds` sem mexer no resto |
| 22 | Código do migfix | **Copiar e adaptar** como ponto de partida: adaptador ADT (validado no MI9), simulador SAP, motor de execução e shell do front. **Não** reaproveitar `ChangeMarker` (formato diferente) nem a busca vetorial |
| 23 | Modelo de IA (09.11.2026, consultor) | **Um só modelo para todas as necessidades**, padrão **Claude Opus**: o cadastro de IA não tem mais "uso" e tem um único padrão. O uso da chamada (mecânico/revisão/análise) só define esforço e teto de saída e fica no registro de cada chamada. No app substitui o "modelo mais barato que dê conta" (§3.3); as skills seguem com a premissa |
| 24 | Remediação automática (09.11.2026, consultor) | **Play** na aba Itens: um run `AutoRemediation` leva os itens escolhidos de pendente a aplicado, sem etapa manual (o play com o "confirmo" é a confirmação explícita da gravação). Travas: IA só é aplicada com sintaxe ok, sem alerta e sem bloqueio da revisão por IA; triagem com todas as regras R1–R9 (R1 conferida no DDIC); objetos standard ficam para o consultor; produção bloqueada; sem rollback automático. A confirmação automática da IA não grava cartão na KB. Monitor de remediação em `/remediacao`. Exceção às regras "fluxo assistido" e "confirmar cada etapa" só para o play |

Regras do `CLAUDE.md` que valem em tudo:
- SAP só leitura, com as exceções controladas (simular antes, confirmar, nunca em produção: `producao` ou
  `T000-CCCATEGORY=P` bloqueiam);
- não inventar conteúdo;
- paralelizar o que for independente, com só a escrita em fila;
- modelo mais barato que dê conta (nas skills; no app vale a decisão 23: um só modelo, padrão Opus);
- registrar tempo, tokens e custo de cada execução.

## 3. Arquitetura

### 3.1 Backend – `atc-back/` (blueprint de backend)

- **Projetos:** `Mignow.Atc.Domain`, `.Application`, `.Infra.Database`, `.Infra.External`, `.Infra.IoC`, `.Api` e
  `.Tests`.
- **Base técnica:** .NET 10, CQRS sem MediatR, `Result` com envelope `{success,data,message}`, ProblemDetails, EF Core
  com Postgres no schema `public`, Entra (anônimo em dev), i18n pt/en/es.
- **Motor** (adaptado do migfix):
  - fila no Postgres (`FOR UPDATE SKIP LOCKED`) e worker `BackgroundService`;
  - **uma execução ativa por ambiente**, com índice único parcial;
  - heartbeat e retomada pelo último passo concluído;
  - progresso por SignalR, com polling de reserva;
  - estados de execução: `Fila`, `Executando`, `AguardandoAprovacao`, `Cancelando`, `Cancelada`, `Concluida`,
    `Falhou`.
- **Portas** (adaptadores em `Infra.External`):

| Porta | Equivale a | Observação |
|---|---|---|
| `ISapReader` | `Adt.psm1` / `SapAdt` GET | Leitura stateless: fonte, estrutura, DDIC, discovery, resultados ATC, sintaxe (checkruns); cache de fontes |
| `ISapSql` | `Invoke-AdtSql` | Data preview **stateless** (o stateful dá HTTP 500 após ~18 consultas), só SELECT |
| `ISapWriter` | `Enable-AdtEscrita` + `aplicar_adt`/`aplicar_ddic`/`solicitar`/classe temporária | **Único ponto de escrita**: trava de produção; lock → PUT → unlock em `finally`; ativação com `?context=` para include compartilhado; DDIC STRU/TABL com trava de estrutura; classe em `$TMP`; pedido de isenção |
| `IKnowledgeBase` | `kb_lookup`, `parse_cards`, `registrar`, `fato` | Markdown da pasta compartilhada; reindexa quando o arquivo muda; uma escrita por vez |
| `IAiGateway` | Agentes e modelos das skills | API Anthropic, modelo por uso (ver 3.3); tokens e custo por chamada |
| `ISecretStore` | `cred.xml` | Credencial cifrada |

- **Certificado HTTPS autoassinado:** thumbprint fixado no primeiro teste de conexão, como faz hoje o
  `testar_conexao`.
- **Simulador SAP** (`sim://`), adaptado do migfix, com findings que tenham objeto referenciado, RObT, nota e
  `Finding ID`, para os testes ponta a ponta.

### 3.2 Frontend – `atc-front/` (blueprint de frontend)

- **Stack:** React Router v8 SPA (`ssr:false`), Trilha A (Orval do Swagger), TanStack Query, RHF + Zod, i18n pt/en/es,
  MSAL (mock em dev), Tailwind v4, tokens provisórios.
- **Organização:**
  - shell com sidebar e topbar, rotas finas → página de feature → hook → client gerado;
  - `features/<módulo>/` autocontidas;
  - componentes comuns: `StepsTimeline`, `EventsLog`, `SourceDiff`, `ItemDetailDialog`, `RunStatusBadge`, copiados do
    migfix.
- **Gates:** `pnpm typecheck && pnpm lint && pnpm test`.

### 3.3 IA (mesmo papel que tem nas skills)

Decisão 23 (09.11.2026): o mesmo modelo para todos os usos, padrão **Claude Opus**. O uso muda só o esforço e o teto:

| Uso | Esforço | Origem |
|---|---|---|
| Preparo de grupo simples (aplicar cartão da KB) | baixo | subagente haiku da etapa 4 |
| Preparo de grupo médio, revisão antes de gravar, textos do relatório | médio | subagente sonnet, `sap-atc-abap`, `analise_mignow` |
| Análise da IA e grupos complexos | alto | modelo principal da skill |

- **Prompts:** versionados, derivados do `SKILL.md`, do `paralelo_agente.md` e dos agentes em `.claude/agents/`.
- **Contexto da Análise da IA:** código atual, mensagem do check, objeto referenciado, nota SAP e DDIC lido antes.
- **Saída:** JSON validado.

## 4. Módulos e telas

| Módulo | Origem | Telas / passos |
|---|---|---|
| **⚙ Cadastros** | `config.json`, sap-access-point | Cliente; Ambiente (SID, host, porta ADT, mandante, HTTPS, idioma, produção, tipo de projeto, autor `AUT-XX`, request padrão, aprovador de isenção); credencial; teste de conexão; descoberta ADT; parâmetros (paralelismo, tamanho do lote, modelo de IA único, caminho da KB); **importar cliente** de `clientes/<CLIENTE>/` |
| **Extração** | atc-extracao | Verificar a última execução → confirmar → job de extração (resultados ADT + `SATC_AC_RESULTH/ITM/FND/BSI`, TADIR, nota, textos) → resumo P1/P2/P3 com a variação → exportar CSV total, `_novos` e `_remediacao` → histórico |
| **Remediação** | atc-remediacao | Ver 4.1 |
| **Isenção** | atc-isencao | Ver 4.2 |
| **Varredura MIGNOW** | atc-extracao-mignow | Regras (`regras.json`) → inventário, escopo e pesos → gerar lotes (template da classe) → simular (`T000`, nomes livres) → executar em paralelo (cria, roda e apaga; resíduo com "remover") → cruzar com o CSV total (`ATC_PEGOU`, `ATC_OUTRA_LINHA`, `MIGNOW_FONTE_COM/SEM_FINDING`) → resumo e CSV P1 → importar na remediação a pedido. **Nunca altera os objetos encontrados** |
| **Relatório final** | atc-resultado | Coleta de todos os históricos e ciclos → burndown, visões por findings e por objetos, check × solução, manual × IA, isenção, KB, execução do processo e custos; análises de isenção, pendências e MIGNOW; destaques manuais; exportar HTML na identidade MIGNOW |
| **Base de conhecimento** | KB_ATC | Navegar por elemento e por padrão; contribuições, fatos, substituições, cache DDIC |

### 4.1 Execução de remediação

| # | Passo | Quem decide |
|---|---|---|
| 1 | Configurar: origem (extração ou CSV importado), prioridades (P1 / P1+P2 / todas), request validada | Consultor |
| 2 | Ler fontes em paralelo, com os objetos standard no fim da fila | – |
| 3 | Triagem R1..R9 + FSCM (proposta, sem IA); conferência da R1 no DDIC obrigatória | **Consultor aplica as regras** |
| 4 | Agrupar por objeto referenciado e forma de chamada (`kb_top`, complexidade, modelo sugerido) | – |
| 5 | Propor, em paralelo por fonte físico: KB → IA; marcadores; novo fonte; diff; verificação de sintaxe sem gravar; alertas da KB | – |
| 6 | **Aguardando aprovação**, por grupo: Confirmar KB · Análise da IA · Analisar individualmente · Cancelar · Isenção (sair e voltar = pausar) | **Consultor** |
| 7 | Revisão pela IA do que vai ser gravado, com o parecer na tela | – |
| 8 | Simular a gravação (fonte mudou? backup) → aplicar ou não → gravar e ativar → `ATIVADO` / `ERRO_ATIVACAO` (manter inativo ou rollback) / `ERRO_GRAVACAO` / `ERRO_LOCK` / `ERRO_REQUEST`; DDIC com `ABORTADO_ESTRUTURA_TABELA` | **Consultor** |
| 9 | Gravar as contribuições e os fatos na KB compartilhada | Consultor (substituir cartão?) |
| 10 | Exportar `atc_status.csv` e gravar o histórico (antes/depois, transições, aplicações, tempo líquido, tokens, custo) | – |

- **Status do item:** `Confirmado KB`, `Confirmado IA`, `Cancelado`, `Isenção` ou pendente.
- **Modo sem confirmação:** opção por execução, com as condições de parada da skill (dúvida de negócio, cópia Z de
  standard, pré-requisito ausente, erro de ativação, tabela com conversão) e relatório do lote.

### 4.2 Isenção

1. Lote dos itens `Isenção`, filtrado por prioridade e vinculado pelo `Finding ID`; sessões antigas, por cruzamento
   com o `_novos`.
2. Descoberta: política `A`, aprovadores, motivos, validades.
3. Parâmetros: aprovador ≠ solicitante, motivo `FPOS`, validade, justificativa com prefixo
   `S/4 MIGRATION MIG-NOW <data> AUT-XX:`.
4. Simular, com status por finding: `SIMULADO`, `BLOQUEADO`, `SEM_ESCOPO` (não envia), `JA_PEDIDO`.
5. Enviar tudo ou por sequência/motivo, **com confirmação**.
6. Conferir (`PEDIDO`/`ERRO`) e gravar o histórico.

Escopo sempre `thisFinding`; nunca em produção.

## 5. Modelo de dados (principal)

- **Cadastros:** `Cliente`, `Ambiente`, `Parametros`.
- **Extração:** `Extracao` (série, display id, totais) e `Finding` (todas as colunas da extração em JSONB + campos
  indexados: prioridade, check, mensagem, objeto, tipo, pacote, objeto referenciado, RObT, nota, `Finding ID`,
  baseline).
- **Remediação:** `Sessao` (cliente + escopo + ciclo; o reimport guarda a sessão anterior como ciclo) e `Item`
  (status, obs, histórico de status, ficha de correção, preparação, aplicações com backup).
- **Execução:** `Execucao`, `Passo`, `Evento`, `ChamadaIA` (motor, tokens e custo).
- **Isenção:** `LoteIsencao` e `PedidoIsencao` (markerId, seq, status, XML enviado e recebido).
- **Varredura:** `Varredura`, `Lote`, `Ocorrencia`.
- **Histórico:** `HistoricoExecucao` por módulo, com o bloco `execucao` (tempo de relógio e líquido, tokens, modelos,
  paralelismo).

## 6. Fases (cada uma termina com os gates do blueprint e confirmação do consultor)

| Fase | Entrega | Pronto quando |
|---|---|---|
| F0 | Testes "golden": entradas e saídas reais de `atc.py` (triagem, `item`, `grupos`, `marcadores`, `preparar`, `exportar`), `converter.py`, `cruzar.py`, `coletar.py`/`gerar.py` | Massa de comparação gravada no repositório |
| F1 | Scaffold do back (fases 1–8 do blueprint) e do front (fases 1–6); ⚙ com cadastros, credencial cifrada e teste de conexão | Teste de conexão OK no simulador e num ambiente real |
| F2 | Adaptadores ADT/SQL, `ISapWriter` com a trava, motor (fila, lock, SignalR), simulador | Execução vazia de ponta a ponta; testes da trava de produção |
| F3 | Indexador e navegador da KB_ATC | `kb_lookup` .NET igual ao Python na massa golden |
| F4 | Extração, importação de CSV e importação de `clientes/<CLIENTE>/` | Extração igual à da skill no mesmo ambiente; cliente importado com os ciclos |
| F5 | Remediação completa (4.1) | Triagem, grupos e marcadores iguais aos golden; gravação real num ambiente de teste (Q01) |
| F6 | Isenção | Simulação e envio validados num ambiente com política `A` |
| F7 | Varredura MIGNOW | Mesmo resultado do cruzamento da skill num ambiente já varrido |
| F8 | Relatório final e histórico de execução | Somas validadas como no `coletar.py` |
| F9 | Entra real, CI (front + API), piloto, troca para `@mig-now/ds` quando houver PAT; docs (`CLAUDE.md`, `HISTORICO_PROJETO.md`, este roadmap) | Go-live |

## 7. Riscos

| Risco | Mitigação |
|---|---|
| Porte em .NET diverge do comportamento das skills | Testes golden da F0 em cada fase |
| App e skills gravando a KB compartilhada ao mesmo tempo | Escrita da app com trava de arquivo, releitura antes de gravar e numeração `C-`/`F-` calculada na hora |
| Cliente trabalhado nos dois lados | Regra 18: cliente na app = só app; a importação marca o cliente |
| Ativação stateful que falha deixa o objeto preso no SM12 | Mesma estratégia da skill (ativação stateless, unlock em `finally`) |
| Volume (19 mil findings, 10 mil objetos na varredura) | Paralelismo configurável, lotes, leitura stateless, retomada |
| Custo de IA | KB antes da IA, modelo por complexidade, custo por execução visível |
