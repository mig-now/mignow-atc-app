using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Entrada de <see cref="SessionCreator"/>: origem, referência (extração/arquivo/varredura), prioridades ("1,2"; vazio = todas), request e o CSV já lido.</summary>
public sealed record SessionSource(RemediationSource Source, string? SourceRef, string? Priorities, string? Request, CsvDocument Document);
