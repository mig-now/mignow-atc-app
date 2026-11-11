using FluentAssertions;
using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Tests.Domain;

public sealed class RunTests
{
    private static Run NewRun() => Run.Create(RunKind.ConnectionTest, 1, 1, Guid.NewGuid(), "{\"x\":1}", "dev@migatc.local", ["A", "B"], DateTimeOffset.UtcNow);

    [Fact]
    public void Nasce_na_fila_com_os_passos_em_ordem()
    {
        var run = NewRun();
        run.Status.Should().Be(RunStatus.Queued);
        run.IsActive.Should().BeTrue();
        run.Steps.Select(s => (s.Code, s.Order)).Should().Equal(("A", 1), ("B", 2));
        run.ParametersJson.Should().Be("{\"x\":1}");
    }

    [Fact]
    public void Sem_passos_ou_com_passo_repetido_e_recusado()
    {
        ((Action)(() => Run.Create(RunKind.Apply, 1, 1, null, null, "u", [], DateTimeOffset.UtcNow)))
            .Should().Throw<DomainException>().Which.Code.Should().Be("Run.NoSteps");
        ((Action)(() => Run.Create(RunKind.Apply, 1, 1, null, null, "u", ["A", "A"], DateTimeOffset.UtcNow)))
            .Should().Throw<DomainException>().Which.Code.Should().Be("Run.DuplicateStep");
    }

    [Fact]
    public void Interromper_na_fila_cancela_na_hora_e_pula_os_passos()
    {
        var run = NewRun();
        run.RequestCancel("dev", DateTimeOffset.UtcNow);
        run.Status.Should().Be(RunStatus.Cancelled);
        run.IsActive.Should().BeFalse();
        run.Steps.Should().OnlyContain(s => s.Status == StepStatus.Skipped);
    }

    [Fact]
    public void Interromper_em_execucao_espera_o_ponto_seguro()
    {
        var run = NewRun();
        run.Start(DateTimeOffset.UtcNow);
        run.RequestCancel("dev", DateTimeOffset.UtcNow);
        run.Status.Should().Be(RunStatus.Cancelling);
        run.IsActive.Should().BeTrue("o ambiente continua bloqueado até o worker parar");
    }

    [Fact]
    public void Retomada_volta_para_a_fila_e_reabre_o_passo_interrompido()
    {
        var run = NewRun();
        run.Start(DateTimeOffset.UtcNow);
        run.Step("A").Complete(DateTimeOffset.UtcNow);
        run.Step("B").Start(DateTimeOffset.UtcNow);
        run.Requeue();
        run.Status.Should().Be(RunStatus.Queued);
        run.Step("A").Status.Should().Be(StepStatus.Completed);
        run.Step("B").Status.Should().Be(StepStatus.Pending);
    }

    [Fact]
    public void Reprocessar_volta_para_a_fila_a_partir_do_passo_que_falhou()
    {
        var run = NewRun();
        run.Start(DateTimeOffset.UtcNow);
        run.Step("A").Complete(DateTimeOffset.UtcNow);
        run.Step("B").Fail(DateTimeOffset.UtcNow, "X.Failed");
        run.Fail(DateTimeOffset.UtcNow, "X.Failed");

        run.Retry();

        run.Status.Should().Be(RunStatus.Queued);
        run.FailureCode.Should().BeNull();
        run.Step("A").Status.Should().Be(StepStatus.Completed, "o que já foi feito não roda de novo");
        run.Step("B").Status.Should().Be(StepStatus.Pending);
    }

    [Fact]
    public void So_execucao_com_falha_pode_ser_reprocessada() =>
        ((Action)NewRun().Retry).Should().Throw<DomainException>().Which.Code.Should().Be("Run.NotFailed");

    [Fact]
    public void Progresso_do_passo_fica_entre_zero_e_o_total_e_completa_no_fim()
    {
        var step = NewRun().Step("A");
        step.ReportProgress(15, 10);
        step.ProgressDone.Should().Be(10);
        step.ReportProgress(3, 10);
        step.ProgressDone.Should().Be(3);
        step.Complete(DateTimeOffset.UtcNow);
        step.ProgressDone.Should().Be(10);
    }

    [Fact]
    public void Uso_de_ia_soma_e_conta_chamadas_sem_custo()
    {
        var run = NewRun();
        run.AddAiUsage(100, 20, 0.5m);
        run.AddAiUsage(10, 2, null);
        run.AiCalls.Should().Be(2);
        run.AiInputTokens.Should().Be(110);
        run.AiCost.Should().Be(0.5m);
        run.AiCallsWithoutCost.Should().Be(1);
    }
}
