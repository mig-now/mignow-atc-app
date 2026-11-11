namespace Mignow.Atc.Application.Dtos.Registry;

public sealed record SettingsResponse(
    int Parallelism, int SourceReadParallelism, int BatchSize, string KbPath, string MignowRulesPath, int MignowParallel, int MignowWeightPerBatch,
    int ManualObjectsPerDay, int HoursPerDay);
