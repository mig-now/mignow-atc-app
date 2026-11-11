using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Infra.External.Ai.Fake;

namespace Mignow.Atc.Tests.RemediationApply;

public sealed class ProposalFactsTests
{
    private sealed class FactsGateway : IAiGateway
    {
        private readonly RemediationFakeResponder _responder = new();

        public Task<Result<T>> CompleteAsync<T>(AiRequest request, Action<AiUsageInfo> onCall, CancellationToken ct)
        {
            var json = JsonNode.Parse(_responder.Respond(request))!;
            if (request.Purpose == "remediation.propose")
                json["items"]![0]!["fatos_candidatos"] = new JsonArray(new JsonObject
                    { ["elemento"] = "KONV", ["fato"] = "KONV foi substituída por PRCD_ELEMENTS", ["prova"] = "nota 2220005", ["impacto"] = "alto" });
            onCall(new AiUsageInfo("fake", 1, 1, 0m));
            return Task.FromResult<Result<T>>(json.Deserialize<T>(Json.Options)!);
        }
    }

    [Fact]
    public async Task Fatos_candidatos_da_ia_ficam_na_proposta_do_item()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S00012", "simples");
        var item = ApplyTestKit.Item(1);
        var engine = new ProposeEngine(new FactsGateway(), kit.Kb, kit.Sap, kit.Sap, kit.Sap, kit.Clock, kit.Localizer);
        var summary = await engine.RunAsync(kit.Input("kb", [item], [kit.Cache()]), kit.HostFor());
        summary.FactCandidates.Should().ContainSingle();
        item.Proposal!.FactCandidates.Should().ContainSingle().Which.Element.Should().Be("KONV");
        item.Proposal.FactCandidates![0].Proof.Should().Be("nota 2220005");
    }
}
