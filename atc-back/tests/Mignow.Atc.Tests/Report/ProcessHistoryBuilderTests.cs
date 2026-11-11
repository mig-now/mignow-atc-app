using FluentAssertions;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Tests.Report;

public sealed class ProcessHistoryBuilderTests
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);

    private static RunWithCalls R(RunKind kind, Guid subject, int startMin, int endMin, int parallelism = 1, params RunAiCall[] calls)
    {
        var run = Run.Create(kind, 1, 1, subject, null, "t", ["S"], T0.AddMinutes(startMin));
        run.Start(T0.AddMinutes(startMin));
        run.SetParallelism(parallelism);
        foreach (var c in calls)
            run.AddAiUsage(c.InputTokens, c.OutputTokens, c.Cost);
        run.Complete(T0.AddMinutes(endMin));
        return new RunWithCalls(run, calls);
    }

    [Fact]
    public void Tempo_liquido_descarta_pausas_acima_de_10_minutos()
    {
        var s = Guid.NewGuid();
        var h = ProcessHistoryBuilder.Build([R(RunKind.Propose, s, 0, 10), R(RunKind.Apply, s, 15, 20), R(RunKind.Apply, s, 50, 55)]);
        var e = h.Should().ContainSingle().Subject;
        e.Module.Should().Be(ProcessModule.Remediation);
        e.ClockSeconds.Should().Be(55 * 60);
        e.NetSeconds.Should().Be((10 + 5 + 5 + 5) * 60);
        e.Runs.Should().Be(3);
    }

    [Fact]
    public void Agrupa_por_modulo_e_alvo_e_soma_tokens_por_modelo()
    {
        var a = Guid.NewGuid();
        var calls = new[]
        {
            RunAiCall.Create(0, AiUsage.Review, "sonnet", 100, 20, 0.5m, true, null, null, T0),
            RunAiCall.Create(0, AiUsage.Review, "sonnet", 50, 10, null, true, null, null, T0),
            RunAiCall.Create(0, AiUsage.Mechanical, "haiku", 10, 5, 0.1m, false, "x", null, T0),
        };
        var h = ProcessHistoryBuilder.Build([R(RunKind.Propose, a, 0, 5, 4, calls), R(RunKind.ExemptionSend, Guid.NewGuid(), 0, 2), R(RunKind.ConnectionTest, a, 0, 1)]);
        h.Should().HaveCount(2);
        var rem = h.Single(x => x.Module == ProcessModule.Remediation);
        rem.InputTokens.Should().Be(160);
        rem.OutputTokens.Should().Be(35);
        rem.MaxParallelism.Should().Be(4);
        rem.ParallelPeriods.Should().ContainSingle();
        rem.ByModel.Should().HaveCount(2);
        rem.ByModel.Single(m => m.Model == "sonnet").InputTokens.Should().Be(150);
        rem.ByModel.Single(m => m.Model == "haiku").FailedCalls.Should().Be(1);
        ProcessHistoryBuilder.Build([R(RunKind.Propose, a, 0, 5)], ProcessModule.Exemption).Should().BeEmpty();
    }
}
