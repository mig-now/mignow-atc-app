using System.Diagnostics;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Application.Services;

namespace Mignow.Atc.Application.Features.SapSystems.TestConnection;

public sealed class TestSapConnectionHandler(IDataStore store, SapConnections connections, ISapReader reader, IDateTimeProvider clock, IStringLocalizer<Errors> l)
    : ICommandHandler<TestSapConnectionCommand, Result<ConnectionTestResponse>>
{
    public async Task<Result<ConnectionTestResponse>> Handle(TestSapConnectionCommand command, CancellationToken ct)
    {
        var system = await SapSystemLoader.LoadAsync(store, command.Id, true, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var now = clock.UtcNow;
        var connection = await connections.ForAsync(system, ct, trustOnFirstUse: true);
        if (connection.IsFailure)
        {
            system.RecordConnectionTest(now, false, connection.Error.Message, null, null, null);
            return new ConnectionTestResponse(false, null, null, system.CertThumbprint, connection.Error.Code, connection.Error.Message, 0, now);
        }
        var watch = Stopwatch.StartNew();
        var result = await reader.TestConnectionAsync(connection.Value, ct);
        watch.Stop();
        if (result.IsFailure)
        {
            system.RecordConnectionTest(now, false, result.Error.Message, null, null, null);
            return new ConnectionTestResponse(false, null, null, system.CertThumbprint, result.Error.Code, result.Error.Message, watch.ElapsedMilliseconds, now);
        }
        var info = result.Value;
        system.RecordConnectionTest(now, true, null, info.Release, info.ClientCategory, info.CertificateThumbprint);
        return new ConnectionTestResponse(true, info.Release, info.ClientCategory, system.CertThumbprint, null, null, watch.ElapsedMilliseconds, now);
    }
}
