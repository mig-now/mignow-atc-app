using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.Sessions;

namespace Mignow.Atc.Application.Features.Scans.Import;

/// <summary>Cria a sessão de remediação (Origin = MIGNOW, prioridade 1) a partir do CSV P1 da varredura. Só a pedido do consultor.</summary>
public sealed record ImportScanCommand(Guid Id, bool? IncludeOtherLine = null) : ICommand<Result<SessionResponse>>;
