using FluentAssertions;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Application.Services;
using NSubstitute;

namespace Mignow.Atc.Tests.Sap;

/// <summary>Regra do Enable-AdtEscrita: produção no cadastro, T000=P ou categoria ilegível bloqueiam.</summary>
public sealed class SapWriteGuardTests
{
    private static readonly IStringLocalizer<Errors> L = Localizer();

    private static SapConnection Conn(bool production) => new(Guid.NewGuid(), "https://h:1", "400", "U", "p", "EN", null, production);

    [Theory]
    [InlineData(true, "T", "Sap.WriteBlockedProduction")]
    [InlineData(false, "P", "Sap.WriteBlockedProductionClient")]
    [InlineData(false, "p", "Sap.WriteBlockedProductionClient")]
    [InlineData(false, null, "Sap.WriteBlockedCategoryUnknown")]
    [InlineData(false, " ", "Sap.WriteBlockedCategoryUnknown")]
    public void Bloqueia(bool production, string? category, string code)
    {
        var r = SapWriteGuard.Evaluate(Conn(production), category, L);
        r.IsFailure.Should().BeTrue();
        r.Error.Code.Should().Be(code);
    }

    [Theory]
    [InlineData("T")]
    [InlineData("C")]
    [InlineData("S")]
    public void Libera_fora_de_producao(string category) =>
        SapWriteGuard.Evaluate(Conn(false), category, L).Value.ClientCategory.Should().Be(category);

    [Fact]
    public void Consulta_da_T000_escapa_o_mandante() =>
        SapWriteGuard.T000Query("40'0").Should().Be("SELECT cccategory FROM t000 WHERE mandt = '40''0'");

    private static IStringLocalizer<Errors> Localizer()
    {
        var l = Substitute.For<IStringLocalizer<Errors>>();
        l[Arg.Any<string>(), Arg.Any<object[]>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        l[Arg.Any<string>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        return l;
    }
}
