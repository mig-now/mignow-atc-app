using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions;

/// <summary>
/// Sessão de remediação com a situação dos itens (<see cref="Counts"/>) e o resumo da importação, quando acabou de ser criada. Na criação, a leitura
/// dos fontes começa sozinha: <see cref="SourcesRunId"/> = o run <c>ReadSources</c>, ou <see cref="SourcesNotStarted"/> = código do motivo
/// (ex.: <c>Run.EnvironmentBusy</c>).
/// </summary>
public sealed record SessionResponse(
    Guid Id, Guid CustomerId, string CustomerName, Guid SapSystemId, string SapSystemName, RemediationSource Source, string? SourceRef, string Priorities,
    string? Request, string Author, ProjectType ProjectType, int Cycle, bool Archived, DateTimeOffset? ArchivedAt, bool AutoMode, string CsvEncoding,
    string CsvDelimiter, IReadOnlyList<string> Columns, DateTimeOffset CreatedAt, SituationCounts Counts, ImportSummary? Import = null, int CarriedOver = 0,
    Guid? SourcesRunId = null, string? SourcesNotStarted = null);
