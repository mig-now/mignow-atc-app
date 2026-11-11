using System.Globalization;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Extraction;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Verificação da última execução concluída (<c>extrair.ps1 -SoVerificar</c>): lê os cabeçalhos pela porta, escolhe pela
/// <see cref="AtcRunSelector"/> e descreve (série, data, totais, em andamento, já extraída?). Só SAP; o banco (já extraída?)
/// é consultado por quem chama.
/// </summary>
public static class AtcLatestFinder
{
    /// <summary>Lê os cabeçalhos e escolhe a execução; sem execução concluída, falha <c>Extraction.NoCompletedRun</c>.</summary>
    public static async Task<Result<AtcRunSelection>> SelectAsync(
        IAtcExtractionReader reader, IStringLocalizer<Errors> l, SapConnection connection, CancellationToken ct)
    {
        var headers = await reader.ListRunsAsync(connection, AtcRunSelector.DefaultPrefixes, ct);
        if (headers.IsFailure)
            return headers.Error;
        var selection = AtcRunSelector.Select(headers.Value, AtcRunSelector.DefaultPrefixes);
        return selection.Latest is null
            ? l.NotFound("Extraction.NoCompletedRun", string.Join('/', AtcRunSelector.DefaultPrefixes), selection.ExecutionsFound.ToString(CultureInfo.InvariantCulture))
            : selection;
    }

    public static AtcLatestResponse Describe(AtcRunSelection selection, SapSystem system, AtcExtraction? existing)
    {
        var h = selection.Latest!;
        var executed = AtcRunSelector.ParseUtc(h.ScheduledOnTs);
        return new AtcLatestResponse(system.Guid, h.DisplayId, h.Series, h.Title, executed, h.ScheduledBy, h.CheckProfile, h.Configuration, h.Prio1, h.Prio2,
            h.Prio3, h.Prio4, h.PlanningErrors,
            [.. selection.InProgress.Select(p => new InProgressRun(p.DisplayId, p.Series, p.Title, AtcRunSelector.ParseUtc(p.ScheduledOnTs)))],
            existing is not null, existing?.Guid, ExtractionNaming.BaseName(system.Sid, system.Client, h.Series, executed, h.DisplayId));
    }
}
