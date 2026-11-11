using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Sessions.Monitor;

/// <summary>Linha do monitor de remediação: a sessão (com a situação dos itens) e a última execução dela (qualquer etapa, com passos).</summary>
public sealed record RemediationMonitorRow(SessionResponse Session, RunResponse? LastRun);
