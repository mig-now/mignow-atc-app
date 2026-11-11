using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Report;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// Tudo que o <see cref="ReportDataBuilder"/> lê do banco para um cliente e um ambiente (o equivalente à pasta
/// <c>clientes/&lt;CLIENTE&gt;/</c> do <c>coletar.py</c>). <c>Sessions</c> inclui as arquivadas (ciclos anteriores), em ordem de ciclo.
/// </summary>
public sealed record ReportInput(
    Customer Customer,
    SapSystem System,
    GeneralSettings Settings,
    IReadOnlyList<SessionData> Sessions,
    IReadOnlyList<ExemptionBatchData> Batches,
    IReadOnlyList<AtcExtraction> Extractions,
    IReadOnlyList<RunWithCalls> Runs,
    ReportHighlights? Highlights,
    DateTimeOffset Now,
    TimeZoneInfo Zone,
    bool IncludeReportRuns = false,
    IReadOnlyDictionary<string, string?>? SkillVersions = null);
