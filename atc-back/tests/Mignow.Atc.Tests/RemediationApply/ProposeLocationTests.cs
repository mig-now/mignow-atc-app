using FluentAssertions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.External.Ai.Fake;

namespace Mignow.Atc.Tests.RemediationApply;

/// <summary>
/// Linha do finding para a proposta: o CSV da sessão (layout da remediação) não traz a linha, então ela vem da <c>location</c> da extração pelo
/// Finding ID (porte do <c>extracao_por_finding</c>). Sem linha nenhuma, a IA recebe o fonte inteiro, não só as primeiras 70 linhas.
/// </summary>
public sealed class ProposeLocationTests
{
    private const string FindingId = "atc:273438E85E731FD1B595F4AE71307EAC,2728";

    private static RemediationItem Item(int n, string? findingId) =>
        RemediationItem.Create(1, n, "ZPROG", "PROG", null, null, "Search problematic statements for result of SELECT/OPEN CURSOR without ORDER BY",
            "READ .. BINARY SEARCH for result of statement at ... line ...", null, null, "ZPACK", null, "1", findingId, ItemOrigin.Atc, "[]");

    [Fact]
    public void Linha_vem_da_location_da_extracao_pelo_finding_id()
    {
        var kit = new ApplyTestKit();
        var locations = new Dictionary<string, string> { [FindingId] = "/sap/bc/adt/programs/programs/zprog/source/main#start=3,0" };

        var (cache, line) = new ItemSourceResolver([kit.Cache()], [], locations).Resolve(Item(1, FindingId));
        cache!.IncludeName.Should().Be("ZPROG");
        line.Should().Be(3);

        new ItemSourceResolver([kit.Cache()], [], locations).Resolve(Item(2, "atc:OUTRO,1")).Line.Should().BeNull();
    }

    [Fact]
    public async Task Sem_linha_a_ia_recebe_o_fonte_inteiro()
    {
        var kit = new ApplyTestKit();
        var source = "REPORT zprog.\n" + string.Join('\n', Enumerable.Range(2, 199).Select(i => $"* LINHA_{i:000}")) + "\n";
        var capture = new CaptureGateway(new ResponderGateway(new RemediationFakeResponder()));
        var engine = new ProposeEngine(capture, kit.Kb, kit.Sap, kit.Sap, kit.Sap, kit.Clock, kit.Localizer);

        await engine.RunAsync(kit.Input("ia", [Item(1, null)], [kit.Cache(source)]), kit.HostFor());

        capture.Prompts.Should().ContainSingle().Which.Should().Contain("LINHA_200", "sem linha, o fonte vai inteiro (antes só as linhas 1–70)");
    }

    [Fact]
    public async Task Programa_pequeno_vai_inteiro_mesmo_com_a_linha_conhecida()
    {
        var kit = new ApplyTestKit();
        var source = "REPORT zprog.\n" + string.Join('\n', Enumerable.Range(2, 399).Select(i => $"* LINHA_{i:000}")) + "\n";
        var capture = new CaptureGateway(new ResponderGateway(new RemediationFakeResponder()));
        var engine = new ProposeEngine(capture, kit.Kb, kit.Sap, kit.Sap, kit.Sap, kit.Clock, kit.Localizer);
        var locations = new Dictionary<string, string> { [FindingId] = "/sap/bc/adt/programs/programs/zprog/source/main#start=300,0" };

        await engine.RunAsync(kit.Input("ia", [Item(1, FindingId)], [kit.Cache(source)]) with { Locations = locations }, kit.HostFor());

        // a linha 300 daria a janela 270–340; o tipo e o SELECT do começo do fonte ficariam de fora
        capture.Prompts.Single().Should().Contain("LINHA_002").And.Contain("LINHA_400");
    }

    private sealed class CaptureGateway(IAiGateway inner) : IAiGateway
    {
        public List<string> Prompts { get; } = [];

        public Task<Result<T>> CompleteAsync<T>(AiRequest request, Action<AiUsageInfo> onCall, CancellationToken ct)
        {
            Prompts.Add(request.User);
            return inner.CompleteAsync<T>(request, onCall, ct);
        }
    }
}
