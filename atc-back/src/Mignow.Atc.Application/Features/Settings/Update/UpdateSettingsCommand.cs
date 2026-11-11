using System.ComponentModel.DataAnnotations;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.Settings.Update;

/// <summary>Faixas válidas nos atributos <c>[Range]</c> (saem no Swagger como minimum/maximum para o front).</summary>
public sealed record UpdateSettingsCommand(
    [Range(1, 16)] int Parallelism,
    [Range(1, 16)] int SourceReadParallelism,
    [Range(1, int.MaxValue)] int BatchSize,
    [StringLength(500, MinimumLength = 1)] string KbPath,
    [StringLength(500, MinimumLength = 1)] string MignowRulesPath,
    [Range(1, 9)] int MignowParallel,
    [Range(1, int.MaxValue)] int MignowWeightPerBatch,
    [Range(1, int.MaxValue)] int ManualObjectsPerDay,
    [Range(1, 24)] int HoursPerDay) : ICommand<Result<SettingsResponse>>;
