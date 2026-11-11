using System.Text.Json.Serialization;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// Os dados do relatório final de um cliente: porte do <c>dados.json</c> do <c>coletar.py</c> (skill atc-resultado), com
/// as mesmas chaves e a mesma estrutura. Tipos aninhados para manter um tipo público por arquivo.
/// </summary>
public sealed record ReportData(
    [property: JsonPropertyName("gerado_em")] string GeneratedAt,
    [property: JsonPropertyName("cliente")] ReportData.CustomerInfo Customer,
    [property: JsonPropertyName("extracao")] ReportData.ExtractionInfo? Extraction,
    [property: JsonPropertyName("escopo")] IReadOnlyList<string> Scope,
    [property: JsonPropertyName("ciclos")] IReadOnlyList<ReportData.CycleSummary> Cycles,
    [property: JsonPropertyName("itens")] IReadOnlyList<ReportData.ItemRow> Items,
    [property: JsonPropertyName("objetos_total")] int TotalObjects,
    [property: JsonPropertyName("evolucao")] ReportData.EvolutionInfo Evolution,
    [property: JsonPropertyName("aplicacao")] ReportData.ApplicationInfo Application,
    [property: JsonPropertyName("isencao")] ReportData.ExemptionInfo? Exemption,
    [property: JsonPropertyName("kb")] ReportData.KbInfo Kb,
    [property: JsonPropertyName("versoes")] IReadOnlyDictionary<string, ReportData.SkillVersion> Versions,
    [property: JsonPropertyName("custos")] ReportData.CostsInfo Costs,
    [property: JsonPropertyName("comparacao_manual")] ReportData.Comparison ManualComparison,
    [property: JsonPropertyName("execucao_processo")] ReportData.ProcessInfo Process,
    [property: JsonPropertyName("complementos")] ReportData.ComplementsInfo Complements,
    [property: JsonPropertyName("validacao")] ReportData.ValidationInfo Validation)
{
    public sealed record CustomerInfo(
        [property: JsonPropertyName("nome")] string Name,
        [property: JsonPropertyName("sid")] string? Sid,
        [property: JsonPropertyName("mandante")] string? Client,
        [property: JsonPropertyName("release")] string? Release,
        [property: JsonPropertyName("papel")] string? Role,
        [property: JsonPropertyName("autor")] string? Author,
        [property: JsonPropertyName("request")] string? Request);

    public sealed record ExtractionInfo(
        [property: JsonPropertyName("serie")] string? Series,
        [property: JsonPropertyName("display_id")] string? DisplayId,
        [property: JsonPropertyName("executada")] string? ExecutedAt,
        [property: JsonPropertyName("total")] int? Total,
        [property: JsonPropertyName("em_baseline")] int? InBaseline,
        [property: JsonPropertyName("com_baseline")] int? WithoutBaseline,
        [property: JsonPropertyName("p1")] int? P1,
        [property: JsonPropertyName("p2")] int? P2,
        [property: JsonPropertyName("p3")] int? P3,
        [property: JsonPropertyName("fonte")] string Source);

    public sealed record AlreadyTreated(
        [property: JsonPropertyName("qtde")] int Count,
        [property: JsonPropertyName("por_solucao_anterior")] IReadOnlyDictionary<string, int> BySolution);

    public sealed record CycleSummary(
        [property: JsonPropertyName("ciclo")] int Cycle,
        [property: JsonPropertyName("fonte")] string Source,
        [property: JsonPropertyName("inicio")] string? Start,
        [property: JsonPropertyName("extracao")] ExtractionInfo? Extraction,
        [property: JsonPropertyName("escopo")] IReadOnlyList<string> Scope,
        [property: JsonPropertyName("findings")] int Findings,
        [property: JsonPropertyName("objetos")] int Objects,
        [property: JsonPropertyName("por_solucao")] IReadOnlyDictionary<string, int> BySolution,
        [property: JsonPropertyName("ja_tratados_antes")] AlreadyTreated AlreadyTreatedBefore);

    public sealed record HistoryEntry(
        [property: JsonPropertyName("data")] string Date,
        [property: JsonPropertyName("status")] string Status);

    public sealed record ItemRow(
        [property: JsonPropertyName("n")] int N,
        [property: JsonPropertyName("ciclo")] int Cycle,
        [property: JsonPropertyName("obj")] string Obj,
        [property: JsonPropertyName("tipo")] string Type,
        [property: JsonPropertyName("check")] string Check,
        [property: JsonPropertyName("msg")] string Message,
        [property: JsonPropertyName("ref")] string Ref,
        [property: JsonPropertyName("sol")] string Solution,
        [property: JsonPropertyName("sis")] string System,
        [property: JsonPropertyName("req")] string Request,
        [property: JsonPropertyName("obs")] string Obs,
        [property: JsonPropertyName("data")] string Date,
        [property: JsonPropertyName("hist")] IReadOnlyList<HistoryEntry> History);

    public sealed record EvolutionPoint(
        [property: JsonPropertyName("t")] long T,
        [property: JsonPropertyName("rot")] string Label,
        [property: JsonPropertyName("KB")] int Kb,
        [property: JsonPropertyName("Análise")] int Analysis,
        [property: JsonPropertyName("Isenção")] int Exemption,
        [property: JsonPropertyName("Cancelado")] int Cancelled,
        [property: JsonPropertyName("Pendente")] int Pending,
        [property: JsonPropertyName("objetos_pendentes")] int? PendingObjects,
        [property: JsonPropertyName("ciclo")] int Cycle,
        [property: JsonPropertyName("total")] int Total,
        [property: JsonPropertyName("total_objetos")] int TotalObjects,
        [property: JsonPropertyName("reconstruido")] bool? Reconstructed = null,
        [property: JsonPropertyName("sessao")] bool? SessionPoint = null);

    public sealed record EvolutionInfo(
        [property: JsonPropertyName("pontos")] IReadOnlyList<EvolutionPoint> Points,
        [property: JsonPropertyName("fonte")] IReadOnlyList<string> Sources);

    public sealed record ManualApplication(
        [property: JsonPropertyName("n")] int N,
        [property: JsonPropertyName("objeto")] string Object,
        [property: JsonPropertyName("tipo")] string Type,
        [property: JsonPropertyName("motivo")] string Reason);

    public sealed record ApplicationInfo(
        [property: JsonPropertyName("por_request")] IReadOnlyDictionary<string, int> ByRequest,
        [property: JsonPropertyName("fontes_ativadas")] int ActivatedSources,
        [property: JsonPropertyName("manuais")] IReadOnlyList<ManualApplication> Manual,
        [property: JsonPropertyName("request_sessao")] string? SessionRequest);

    public sealed record ExemptionCycle(
        [property: JsonPropertyName("ciclo")] int Cycle,
        [property: JsonPropertyName("lote")] string Batch,
        [property: JsonPropertyName("findings")] int Findings,
        [property: JsonPropertyName("pedidos")] int Requested,
        [property: JsonPropertyName("sem_escopo")] int NoScope);

    public sealed record ExemptionInfo(
        [property: JsonPropertyName("lote")] string Batch,
        [property: JsonPropertyName("findings")] int Findings,
        [property: JsonPropertyName("pedidos")] int Requested,
        [property: JsonPropertyName("sem_escopo")] int NoScope,
        [property: JsonPropertyName("bloqueados")] int Blocked,
        [property: JsonPropertyName("erros")] int Errors,
        [property: JsonPropertyName("ja_pedidos")] int AlreadyRequested,
        [property: JsonPropertyName("simulados_sem_envio")] int SimulatedNotSent,
        [property: JsonPropertyName("conferidos_ok")] int CheckedOk,
        [property: JsonPropertyName("causas_sem_escopo")] IReadOnlyDictionary<string, int> NoScopeCauses,
        [property: JsonPropertyName("conferir")] IReadOnlyList<string> ToCheck,
        [property: JsonPropertyName("aprovador")] string? Approver,
        [property: JsonPropertyName("motivos")] IReadOnlyList<string> Reasons,
        [property: JsonPropertyName("data")] string? Date,
        [property: JsonPropertyName("fonte")] string Source,
        [property: JsonPropertyName("por_ciclo")] IReadOnlyList<ExemptionCycle> ByCycle);

    public sealed record KbInfo(
        [property: JsonPropertyName("fatos_novos")] int NewFacts,
        [property: JsonPropertyName("cartoes_novos")] int NewCards,
        [property: JsonPropertyName("elementos_com_cartoes")] int? ElementsWithCards,
        [property: JsonPropertyName("relatorios_lote")] int BatchReports);

    public sealed record SkillVersion(
        [property: JsonPropertyName("inicial")] string? Initial,
        [property: JsonPropertyName("atual")] string? Current);

    public sealed record CostPhase(
        [property: JsonPropertyName("etapa")] string Stage,
        [property: JsonPropertyName("ciclo")] int Cycle,
        [property: JsonPropertyName("ini")] string Start,
        [property: JsonPropertyName("fim")] string End,
        [property: JsonPropertyName("sessoes")] int Sessions,
        [property: JsonPropertyName("liquido_s")] long NetSeconds,
        [property: JsonPropertyName("total")] long Total,
        [property: JsonPropertyName("saida")] long Output,
        [property: JsonPropertyName("cache_leitura")] long CacheRead,
        [property: JsonPropertyName("entrada_escrita")] long InputAndCacheWrite,
        [property: JsonPropertyName("periodo")] string Period);

    public sealed record CostTotal(
        [property: JsonPropertyName("sessoes")] int Sessions,
        [property: JsonPropertyName("liquido_s")] long NetSeconds,
        [property: JsonPropertyName("total")] long Total,
        [property: JsonPropertyName("saida")] long Output,
        [property: JsonPropertyName("cache_leitura")] long CacheRead,
        [property: JsonPropertyName("entrada_escrita")] long InputAndCacheWrite,
        [property: JsonPropertyName("periodo")] string? Period,
        [property: JsonPropertyName("calendario_h")] long? CalendarHours);

    public sealed record CostsInfo(
        [property: JsonPropertyName("fases")] IReadOnlyList<CostPhase> Phases,
        [property: JsonPropertyName("total")] CostTotal Total,
        [property: JsonPropertyName("remediacao_liquido_s")] long RemediationNetSeconds,
        [property: JsonPropertyName("liquido_por_finding_s")] long? NetPerFindingSeconds,
        [property: JsonPropertyName("liquido_por_objeto_s")] long? NetPerObjectSeconds,
        [property: JsonPropertyName("fonte")] string Source,
        [property: JsonPropertyName("relatorio_incluido")] bool ReportIncluded,
        [property: JsonPropertyName("relogio_no_lugar_do_liquido")] IReadOnlyList<string> ClockInsteadOfNet);

    public sealed record Premises(
        [property: JsonPropertyName("objetos_dia")] int ObjectsPerDay,
        [property: JsonPropertyName("horas_dia")] int HoursPerDay,
        [property: JsonPropertyName("fonte")] string Source);

    public sealed record ComparisonRow(
        [property: JsonPropertyName("ciclo")] int Cycle,
        [property: JsonPropertyName("objetos_tratados")] int ObjectsTreated,
        [property: JsonPropertyName("objetos_pendentes")] int ObjectsPending,
        [property: JsonPropertyName("manual_s")] long ManualSeconds,
        [property: JsonPropertyName("manual_dias")] double ManualDays,
        [property: JsonPropertyName("ia_s")] long AiSeconds,
        [property: JsonPropertyName("ia_remediacao_s")] long AiRemediationSeconds);

    public sealed record ComparisonTotal(
        [property: JsonPropertyName("objetos_tratados")] int ObjectsTreated,
        [property: JsonPropertyName("objetos_pendentes")] int ObjectsPending,
        [property: JsonPropertyName("manual_s")] long ManualSeconds,
        [property: JsonPropertyName("ia_s")] long AiSeconds,
        [property: JsonPropertyName("ia_remediacao_s")] long AiRemediationSeconds,
        [property: JsonPropertyName("manual_dias")] double ManualDays,
        [property: JsonPropertyName("ganho_s")] long GainSeconds,
        [property: JsonPropertyName("reducao_pct")] double? ReductionPct,
        [property: JsonPropertyName("fator")] double? Factor,
        [property: JsonPropertyName("fator_remediacao")] double? RemediationFactor);

    public sealed record Comparison(
        [property: JsonPropertyName("premissas")] Premises Premises,
        [property: JsonPropertyName("ciclos")] IReadOnlyList<ComparisonRow> Cycles,
        [property: JsonPropertyName("total")] ComparisonTotal Total,
        [property: JsonPropertyName("fonte")] string Source);

    public sealed record ProcessAgent(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("tipo")] string? Type,
        [property: JsonPropertyName("descricao")] string? Description,
        [property: JsonPropertyName("modelos")] IReadOnlyList<string> Models,
        [property: JsonPropertyName("ativo_s")] long ActiveSeconds,
        [property: JsonPropertyName("tokens")] long Tokens,
        [property: JsonPropertyName("profundidade")] int? Depth);

    public sealed record ProcessParallel(
        [property: JsonPropertyName("inicio")] string Start,
        [property: JsonPropertyName("fim")] string End,
        [property: JsonPropertyName("agentes")] int Agents,
        [property: JsonPropertyName("max_simultaneos")] int? MaxSimultaneous,
        [property: JsonPropertyName("soma_ativo_s")] long? ActiveSum,
        [property: JsonPropertyName("relogio_s")] long? ClockSeconds,
        [property: JsonPropertyName("ganho_s")] long? GainSeconds);

    public sealed record ProcessRun(
        [property: JsonPropertyName("etapa")] string Stage,
        [property: JsonPropertyName("fonte")] string Source,
        [property: JsonPropertyName("inicio")] string? Start,
        [property: JsonPropertyName("fim")] string End,
        [property: JsonPropertyName("modelo_principal")] IReadOnlyList<string> MainModel,
        [property: JsonPropertyName("tokens_principal")] long? MainTokens,
        [property: JsonPropertyName("skills")] IReadOnlyDictionary<string, int> Skills,
        [property: JsonPropertyName("agentes")] IReadOnlyList<ProcessAgent> Agents,
        [property: JsonPropertyName("agentes_qtde")] int AgentCount,
        [property: JsonPropertyName("agentes_ativo_s")] long AgentsActiveSeconds,
        [property: JsonPropertyName("agentes_tokens")] long AgentsTokens,
        [property: JsonPropertyName("paralelismo")] IReadOnlyList<ProcessParallel> Parallelism,
        [property: JsonPropertyName("ganho_paralelismo_s")] long ParallelismGainSeconds);

    public sealed record ProcessMissing(
        [property: JsonPropertyName("etapa")] string Stage,
        [property: JsonPropertyName("fim")] string End,
        [property: JsonPropertyName("fonte")] string Source,
        [property: JsonPropertyName("versao_skill")] string? SkillVersion);

    public sealed record ProcessTotals(
        [property: JsonPropertyName("agentes")] int Agents,
        [property: JsonPropertyName("agentes_por_tipo")] IReadOnlyDictionary<string, int> AgentsByType,
        [property: JsonPropertyName("agentes_por_modelo")] IReadOnlyDictionary<string, int> AgentsByModel,
        [property: JsonPropertyName("tokens_por_modelo")] IReadOnlyDictionary<string, long> TokensByModel,
        [property: JsonPropertyName("ativo_agentes_s")] long AgentsActiveSeconds,
        [property: JsonPropertyName("ganho_paralelismo_s")] long ParallelismGainSeconds,
        [property: JsonPropertyName("execucoes_com_registro")] int RunsWithRecord,
        [property: JsonPropertyName("execucoes_sem_registro")] int RunsWithoutRecord);

    public sealed record ProcessInfo(
        [property: JsonPropertyName("execucoes")] IReadOnlyList<ProcessRun> Runs,
        [property: JsonPropertyName("sem_registro")] IReadOnlyList<ProcessMissing> Missing,
        [property: JsonPropertyName("totais")] ProcessTotals Totals,
        [property: JsonPropertyName("fonte")] string Source);

    public sealed record ComplementsInfo(
        [property: JsonPropertyName("destaques")] IReadOnlyList<Mignow.Atc.Domain.Report.HighlightText> Highlights,
        [property: JsonPropertyName("pendencias")] IReadOnlyList<Mignow.Atc.Domain.Report.PendingText> Pending);

    public sealed record ValidationInfo(
        [property: JsonPropertyName("ok")] bool Ok,
        [property: JsonPropertyName("avisos")] IReadOnlyList<string> Warnings);
}
