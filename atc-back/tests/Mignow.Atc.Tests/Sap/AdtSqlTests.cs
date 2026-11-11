using FluentAssertions;
using Mignow.Atc.Infra.External.Sap.Adt;

namespace Mignow.Atc.Tests.Sap;

/// <summary>Porte de Format-AdtSql / Invoke-AdtSqlRaw / Invoke-AdtSqlIn (SapSql.psm1).</summary>
public sealed class AdtSqlTests
{
    [Fact]
    public void Quebra_antes_das_palavras_chave()
    {
        AdtSql.Format("SELECT cccategory FROM t000 WHERE mandt = '400' AND x = 'y'")
            .Should().Be("SELECT cccategory\nFROM t000\nWHERE mandt = '400'\nAND x = 'y'");
    }

    [Fact]
    public void Linha_longa_quebra_nas_virgulas_ate_240()
    {
        var list = string.Join(", ", Enumerable.Range(1, 80).Select(i => $"'OBJ{i:D4}'"));
        var lines = AdtSql.Format($"SELECT obj_name FROM tadir WHERE obj_name IN ({list})").Split('\n');
        lines.Should().OnlyContain(l => l.Length <= 241);
        string.Concat(lines.Skip(2)).Should().Contain("'OBJ0080'");
    }

    [Theory]
    [InlineData("SELECT * FROM t000", true)]
    [InlineData("  select mandt from t000", true)]
    [InlineData("DELETE FROM t000", false)]
    [InlineData("SELECT * FROM t000 WHERE x = 1; UPDATE t000 SET", false)]
    public void Somente_select(string sql, bool ok) => AdtSql.IsSelect(sql).Should().Be(ok);

    [Fact]
    public void Le_colunas_e_linhas_do_data_preview()
    {
        const string xml = """
            <dataPreview:tableData xmlns:dataPreview="http://www.sap.com/adt/dataPreview">
              <dataPreview:columns><dataPreview:metadata dataPreview:name="MANDT"/><dataPreview:dataSet><dataPreview:data>400 </dataPreview:data><dataPreview:data>100</dataPreview:data></dataPreview:dataSet></dataPreview:columns>
              <dataPreview:columns><dataPreview:metadata dataPreview:name="CCCATEGORY"/><dataPreview:dataSet><dataPreview:data>T</dataPreview:data><dataPreview:data> P</dataPreview:data></dataPreview:dataSet></dataPreview:columns>
            </dataPreview:tableData>
            """;
        var r = AdtSql.Parse(xml);
        r.Columns.Should().Equal("MANDT", "CCCATEGORY");
        r.Rows.Should().HaveCount(2);
        r.Rows[1]["CCCATEGORY"].Should().Be("P");
        r.Rows[0]["mandt"].Should().Be("400");
    }

    [Fact]
    public void Listas_do_in_em_blocos_distintos_e_escapados()
    {
        var blocks = AdtSql.InLists(["B", "A", "A", "O'X", ""], 2).ToList();
        blocks.Should().Equal("'A', 'B'", "'O''X'");
    }
}
