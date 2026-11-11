using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Dtos.Registry;

public static class RegistryMappings
{
    /// <summary>O ambiente precisa vir com o cliente (Include).</summary>
    public static SapSystemResponse ToResponse(this SapSystem s) =>
        new(s.Guid, s.Customer.Guid, s.Customer.Name, s.DisplayName, s.Sid, s.Client, s.Host, s.AdtPort, s.Https, s.Language, s.CertThumbprint,
            s.IsProduction, s.ProjectType, s.Author, s.DefaultRequest, s.ExemptionApprover, s.Username, s.PasswordSecretRef is not null, s.Active,
            s.IsSimulator, s.Release, s.ClientCategory, s.LastConnectionTestAt, s.LastConnectionTestOk, s.LastConnectionTestMessage);

    public static AiProviderResponse ToResponse(this AiProvider p) =>
        new(p.Guid, p.Name, p.Kind, p.Model, p.ApiKeySecretRef is not null, p.IsDefault, p.Active);

    public static SettingsResponse ToResponse(this GeneralSettings s) =>
        new(s.Parallelism, s.SourceReadParallelism, s.BatchSize, s.KbPath, s.MignowRulesPath, s.MignowParallel, s.MignowWeightPerBatch,
            s.ManualObjectsPerDay, s.HoursPerDay);
}
