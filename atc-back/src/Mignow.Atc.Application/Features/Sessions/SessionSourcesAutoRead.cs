using Mignow.Atc.Application.Engine.Remediation;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions;

/// <summary>
/// Leitura automática dos fontes logo depois de carregar os itens (sessão nova pela extração, pelo CSV ou pela varredura MIGNOW): run
/// <c>ReadSources</c> com todos os pendentes, sem standard (a leitura do standard pede confirmação) e aproveitando o cache. Entra na mesma
/// transação da sessão. Se o ambiente estiver ocupado (ou inativo), a sessão é criada assim mesmo e a resposta diz o motivo.
/// </summary>
public static class SessionSourcesAutoRead
{
    public static async Task<SessionResponse> StartAsync(RunStarter starter, SapSystem system, RemediationSession session, SessionResponse response,
        CancellationToken ct)
    {
        var run = await starter.EnqueueAsync(RunKind.ReadSources, system, session.Guid,
            new ReadSourcesParameters(session.Guid, int.MaxValue, false, false), ct);
        return run.IsSuccess ? response with { SourcesRunId = run.Value.Guid } : response with { SourcesNotStarted = run.Error.Code };
    }
}
