using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Settings.Update;

public sealed class UpdateSettingsHandler(IDataStore store) : ICommandHandler<UpdateSettingsCommand, Result<SettingsResponse>>
{
    public async Task<Result<SettingsResponse>> Handle(UpdateSettingsCommand c, CancellationToken ct)
    {
        var settings = await store.FirstOrDefaultAsync(store.Query<GeneralSettings>().OrderBy(s => s.Id), ct);
        if (settings is null)
        {
            settings = GeneralSettings.CreateDefault();
            store.Add(settings);
        }
        settings.Update(c.Parallelism, c.SourceReadParallelism, c.BatchSize, c.KbPath, c.MignowRulesPath, c.MignowParallel,
            c.MignowWeightPerBatch, c.ManualObjectsPerDay, c.HoursPerDay);
        return settings.ToResponse();
    }
}
