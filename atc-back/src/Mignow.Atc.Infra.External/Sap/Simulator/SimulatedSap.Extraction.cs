using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Extraction;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Módulo Extraction: execução do ATC simulada coerente com o cenário (objetos + <c>atcRules</c>; ver
/// <c>SapScenario.Extraction.cs</c>). Mesmas portas do <c>AdtSap.Extraction.cs</c>. Falha injetável: <c>ATC_RESULTS</c> + display id.
/// </summary>
public sealed partial class SimulatedSap : IAtcExtractionReader
{
    public Task<Result<IReadOnlyList<AtcRunHeader>>> ListRunsAsync(SapConnection connection, IReadOnlyList<string> prefixes, CancellationToken ct) =>
        With(connection, s =>
        {
            s.Record("ATC_RUNS");
            return Result.Success(s.AtcHeaders(prefixes));
        });

    public Task<Result<AtcRawFindings>> ReadFindingsAsync(SapConnection connection, string displayId, string language, CancellationToken ct) =>
        With(connection, s =>
        {
            s.Record($"ATC_RESULTS {displayId}");
            if (s.ShouldFail("ATC_RESULTS", displayId))
                return Result.Failure<AtcRawFindings>(SapErrors.Upstream(l, "atc-results (simulado)"));
            return s.AtcFindings(displayId) is { } f
                ? Result.Success(f)
                : Result.Failure<AtcRawFindings>(SapErrors.ObjectNotFound(l, displayId));
        });

    public Task<Result<AtcComplements>> ReadComplementsAsync(SapConnection connection, AtcComplementRequest request, CancellationToken ct) =>
        With(connection, s =>
        {
            s.Record("ATC_COMPLEMENTS");
            return Result.Success(s.AtcComplementsFor(s.SimDisplayId));
        });
}
