using FluentAssertions;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Tests.Sap;

public sealed class SimulatedSqlTests
{
    private static SimulatedSapState State(string? category = "T") => new(new SapScenario
    {
        ClientCategory = category,
        Tables = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>
        {
            ["TADIR"] = [
                new Dictionary<string, string> { ["OBJ_NAME"] = "ZA", ["DEVCLASS"] = "ZP" },
                new Dictionary<string, string> { ["OBJ_NAME"] = "ZB", ["DEVCLASS"] = "ZQ" },
                new Dictionary<string, string> { ["OBJ_NAME"] = "ZC", ["DEVCLASS"] = "ZP" },
            ],
        },
    });

    [Fact]
    public void T000_sai_da_categoria_do_cenario()
    {
        var r = State().Select("SELECT cccategory FROM t000 WHERE mandt = '400'", "400", 1)!;
        r.Rows.Should().ContainSingle().Which["CCCATEGORY"].Should().Be("T");
        State(null).Select("SELECT cccategory FROM t000 WHERE mandt = '400'", "400", 1)!.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Filtra_por_igualdade_e_in()
    {
        State().Select("SELECT obj_name FROM tadir WHERE devclass = 'ZP'", "100", 10)!.Rows.Select(r => r["OBJ_NAME"]).Should().Equal("ZA", "ZC");
        State().Select("SELECT obj_name FROM tadir\nWHERE obj_name IN ('ZB', 'ZC')", "100", 10)!.Rows.Select(r => r["OBJ_NAME"]).Should().Equal("ZB", "ZC");
    }

    [Fact]
    public void Sql_fora_do_formato_nao_e_suportado() =>
        State().Select("SELECT a FROM t WHERE x > 1", "100", 1).Should().BeNull();
}
