using FluentAssertions;
using Mignow.Atc.Application.Engine.Diff;

namespace Mignow.Atc.Tests.Engine;

public sealed class LineDiffTests
{
    [Fact]
    public void Mostra_so_o_bloco_alterado_com_contexto()
    {
        var before = "REPORT z.\nDATA a TYPE konv.\nWRITE a.\nEND.";
        var after = "REPORT z.\nDATA a TYPE prcd_elements.\nWRITE a.\nEND.";
        var hunk = LineDiff.Compute(before, after).Should().ContainSingle().Subject;
        hunk.BeforeStart.Should().Be(2);
        hunk.Before.Should().Equal("DATA a TYPE konv.");
        hunk.After.Should().Equal("DATA a TYPE prcd_elements.");
        hunk.ContextBefore.Should().Equal("REPORT z.");
    }

    [Fact]
    public void Fontes_iguais_nao_tem_diferenca() => LineDiff.Compute("a\nb", "a\nb\n").Should().BeEmpty();
}
