using FluentAssertions;
using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Tests.Domain;

public sealed class SapSystemTests
{
    private static SapSystem New(string host, int port = 44332, bool https = true) =>
        SapSystem.Create(Customer.Create("Cliente", "cli"), "di4", "400", host, port, https, "en", null, false, ProjectType.Conversion, "aut-ab", null, null, "user");

    [Fact]
    public void Monta_a_url_do_adt_e_normaliza()
    {
        var s = New("10.99.80.8");
        s.BaseUrl.Should().Be("https://10.99.80.8:44332");
        s.Sid.Should().Be("DI4");
        s.Language.Should().Be("EN");
        s.Author.Should().Be("AUT-AB");
        s.DisplayName.Should().Be("DI4/400");
        New("10.159.252.162", 8001, false).BaseUrl.Should().Be("http://10.159.252.162:8001");
    }

    [Fact]
    public void Simulador_usa_o_proprio_endereco()
    {
        var s = New("sim://Migracao-Tipica", 0, true);
        s.IsSimulator.Should().BeTrue();
        s.BaseUrl.Should().Be("sim://migracao-tipica");
        s.Https.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://host", 443, "SapSystem.InvalidHost")]
    [InlineData("host", 0, "SapSystem.InvalidPort")]
    public void Host_ou_porta_invalidos_sao_recusados(string host, int port, string code) =>
        ((Action)(() => New(host, port))).Should().Throw<DomainException>().Which.Code.Should().Be(code);

    [Fact]
    public void Autor_fora_do_formato_e_recusado() =>
        ((Action)(() => SapSystem.Create(Customer.Create("C", null), "DI4", "400", "h", 1, true, "EN", null, false, ProjectType.Upgrade, "XX", null, null, "u")))
            .Should().Throw<DomainException>().Which.Code.Should().Be("SapSystem.InvalidAuthor");

    [Fact]
    public void Primeiro_teste_ok_fixa_o_certificado_visto_e_guarda_a_categoria()
    {
        var s = New("h");
        s.RecordConnectionTest(DateTimeOffset.UtcNow, true, null, "S4CORE 108 SP0001", "T", "AB12");
        s.CertThumbprint.Should().Be("AB12");
        s.ClientCategory.Should().Be("T");
        s.RecordConnectionTest(DateTimeOffset.UtcNow, true, null, null, "T", "FFFF");
        s.CertThumbprint.Should().Be("AB12", "o fixado não muda sozinho");
    }
}
