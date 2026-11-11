using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps;

/// <summary>
/// Passo único do pipeline <c>ConnectionTest</c>: discovery + release + T000 pelo <see cref="ISapReader"/>, grava o
/// resultado no ambiente (e fixa o certificado visto) e o resumo no run. Idempotente.
/// </summary>
public sealed class TestConnectionStep(ISapReader reader) : IRunStep
{
    public string Code => StepCodes.TestConnection;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        await ctx.ReportProgressAsync(0, 1);
        var result = await reader.TestConnectionAsync(ctx.SapConnection, ctx.Ct);
        var now = ctx.Clock.UtcNow;
        if (result.IsFailure)
        {
            await ctx.MutateAsync(() => ctx.SapSystem.RecordConnectionTest(now, false, result.Error.Message, null, null, null));
            await ctx.SetResultAsync(new { ok = false, code = result.Error.Code, message = result.Error.Message });
            return StepOutcome.Stop(result.Error.Code, Json.Args(("message", result.Error.Message)));
        }
        var info = result.Value;
        await ctx.MutateAsync(() => ctx.SapSystem.RecordConnectionTest(now, true, null, info.Release, info.ClientCategory, info.CertificateThumbprint));
        await ctx.SetResultAsync(new { ok = true, release = info.Release, clientCategory = info.ClientCategory, certThumbprint = ctx.SapSystem.CertThumbprint });
        await ctx.ReportProgressAsync(1, 1);
        await ctx.LogAsync(EventLevel.Info, "Sap.ConnectionOk", Json.Args(("release", info.Release), ("category", info.ClientCategory)));
        return StepOutcome.Info("Sap.ConnectionOk", Json.Args(("release", info.Release), ("category", info.ClientCategory)));
    }
}
