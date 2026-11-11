using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Uma sessão de remediação (um ciclo) com os itens: ciclos anteriores = sessões arquivadas (os <c>backup_sessao_*</c>).</summary>
public sealed record SessionData(RemediationSession Session, IReadOnlyList<RemediationItem> Items, DateTimeOffset CreatedAt);
