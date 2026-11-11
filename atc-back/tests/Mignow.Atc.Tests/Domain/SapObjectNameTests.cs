using FluentAssertions;
using Mignow.Atc.Domain.ValueObjects;

namespace Mignow.Atc.Tests.Domain;

public sealed class SapObjectNameTests
{
    [Theory]
    [InlineData("ZMIGFIX", true)]
    [InlineData("YLEGACY", true)]
    [InlineData("/ZNS/PKG", true)]
    [InlineData("zlower", true)]
    [InlineData("VKON", false)]
    [InlineData("SAPSTD", false)]
    [InlineData("$TMP", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void So_pacotes_do_cliente_sao_alteraveis(string? package, bool expected) => SapObjectName.IsCustomer(package).Should().Be(expected);
}
