using FluentAssertions;
using Mignow.Atc.Application.Features.Sessions.Decide;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.RemediationApply;

/// <summary>
/// "Pendente de aprovação" = o item tem proposta esperando a decisão do consultor (correção, isenção, cancelamento ou "sem solução" da IA).
/// Só a proposta KB pendente (sem cartão na KB) não conta: ainda falta a análise da IA.
/// </summary>
public sealed class AwaitingApprovalTests
{
    private static ItemProposal Proposal(string origin, string action) =>
        new(origin, null, [], null, null, null, [], null, null, null, null, "motivo", action, null, DateTimeOffset.UtcNow);

    [Theory]
    [InlineData(ItemProposal.OriginKb, ItemProposal.ActionPending, false)]
    [InlineData(ItemProposal.OriginKb, ItemProposal.ActionFix, true)]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionPending, true)]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionExemption, true)]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionCancel, true)]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionFix, true)]
    public void Proposta_esperando_o_consultor_fica_pendente_de_aprovacao(string origin, string action, bool expected)
    {
        var item = ApplyTestKit.Item(1);
        item.AwaitingApproval.Should().BeFalse("sem proposta, ainda não foi processado");
        item.SetProposal(Proposal(origin, action));
        item.AwaitingApproval.Should().Be(expected);
    }

    [Theory]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionExemption, SessionDecision.Exemption)]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionCancel, SessionDecision.Cancel)]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionFix, SessionDecision.ConfirmIa)]
    [InlineData(ItemProposal.OriginKb, ItemProposal.ActionFix, SessionDecision.ConfirmKb)]
    [InlineData(ItemProposal.OriginAi, ItemProposal.ActionPending, null)]
    public void Aceitar_a_sugestao_aplica_o_que_a_proposta_recomenda(string origin, string action, SessionDecision? expected)
    {
        var item = ApplyTestKit.Item(1);
        item.SetProposal(Proposal(origin, action));
        DecideItemsHandler.SuggestionOf(item).Should().Be(expected);
    }
}
