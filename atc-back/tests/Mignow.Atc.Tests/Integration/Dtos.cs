namespace Mignow.Atc.Tests.Integration;

// Formas mínimas das respostas usadas nas asserções (o contrato completo é o Swagger).
public sealed record PagedDto<T>(List<T> Items, int Page, int PageSize, long Total);

public sealed record IdDto(Guid Id);

public sealed record MeDto(string? Email, string? Name, string Role);

public sealed record SapSystemDto(Guid Id, string DisplayName, bool HasPassword, bool IsSimulator, string Host, bool? LastConnectionTestOk,
    string? ClientCategory, string Author);

public sealed record ConnectionTestDto(bool Ok, string? Release, string? ClientCategory, string? MessageCode);

public sealed record StepDto(string Code, string Status, string? MessageCode, int ProgressDone, int ProgressTotal);

public sealed record RunDto(Guid Id, string Kind, string Status, Guid SapSystemId, Guid? SubjectId, string? FailureCode, List<StepDto> Steps,
    System.Text.Json.JsonElement? Result);

public sealed record EventDto(long Id, string Level, string Code, string? StepCode);

public sealed record AiProviderDto(Guid Id, string Kind, string Model, bool HasApiKey, bool IsDefault);

public sealed record SettingsDto(int Parallelism, int SourceReadParallelism, int BatchSize, string KbPath, string MignowRulesPath, int MignowParallel,
    int MignowWeightPerBatch, int ManualObjectsPerDay, int HoursPerDay);
