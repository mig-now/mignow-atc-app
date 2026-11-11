using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Sessão criada (ainda não gravada: o commit é do handler que chamou), resumo da importação e o que o ciclo anterior deixou.</summary>
public sealed record SessionCreated(RemediationSession Session, ImportSummary Summary, int CarriedOver, int ArchivedSessions);
