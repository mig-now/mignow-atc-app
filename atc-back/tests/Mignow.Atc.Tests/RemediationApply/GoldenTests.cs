using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Tests.RemediationApply;

/// <summary>Paridade com o <c>atc.py</c> original (<c>marcadores</c> e <c>preparar</c>): massa em <c>Golden/remediation-apply</c>, gerada por <c>gerar.py</c>.</summary>
public sealed partial class GoldenTests
{
    private static string Root => Path.Combine(AppContext.BaseDirectory, "Golden", "remediation-apply");

    private static string Read(params string[] parts) => File.ReadAllText(Path.Combine([Root, .. parts]));

    private static DateOnly DateIn(string text) => DateOnly.ParseExact(DateRegex().Match(text).Value, "dd.MM.yyyy", null);

    public static IEnumerable<object[]> MarkerCases() => Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "Golden", "remediation-apply", "marcadores")).Select(d => new object[] { Path.GetFileName(d) });

    public static IEnumerable<object[]> PatchCases() => Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "Golden", "remediation-apply", "preparar")).Select(d => new object[] { Path.GetFileName(d) });

    [Theory]
    [MemberData(nameof(MarkerCases))]
    public void Marcadores_igual_ao_script(string name)
    {
        var expected = Read("marcadores", name, "esperado.abap");
        var result = ChangeMarker.Build(Read("marcadores", name, "tipo.txt"), Lines(Read("marcadores", name, "antes.abap")), Lines(Read("marcadores", name, "depois.abap")), "AUT-ML", DateIn(expected));
        result.IsOk.Should().BeTrue();
        (string.Join("\n", result.Lines) + "\n").Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(PatchCases))]
    public void Preparar_igual_ao_script(string name)
    {
        var meta = JsonDocument.Parse(Read("preparar", name, "casos.json")).RootElement;
        var pairs = meta.GetProperty("pares").GetInt32();
        var lines = meta.GetProperty("linhas");
        var date = DateOnly.ParseExact("06.11.2026", "dd.MM.yyyy", null);
        List<PatchSpec> specs = [];
        for (var k = 1; k <= pairs; k++)
        {
            var before = Lines(Read("preparar", name, $"antes_{k}.abap"));
            var after = Lines(Read("preparar", name, $"depois_{k}.abap"));
            var block = Lines(Read("preparar", name, $"bloco_{k}.abap"));
            // o bloco esperado do script também prova o ChangeMarker
            ChangeMarker.Build(Read("preparar", name, $"tipo_{k}.txt"), before, after, "AUT-ML", date).Lines.Should().Equal(block);
            specs.Add(new PatchSpec(before, block, lines.ValueKind == JsonValueKind.Array ? lines[k - 1].GetInt32() : null));
        }
        var source = Read("preparar", name, "fonte.abap");
        var expectedJson = JsonDocument.Parse(Read("preparar", name, "esperado_saida.json")).RootElement;
        var outcome = SourcePatcher.Patch(source, specs, expectedJson.TryGetProperty("fonte", out var f) ? f.GetString()! : "ZPROG.abap");
        if (!expectedJson.GetProperty("ok").GetBoolean())
        {
            outcome.IsOk.Should().BeFalse();
            if (expectedJson.TryGetProperty("ocorrencias", out var occ))
                outcome.Occurrences.Should().Equal(occ.EnumerateArray().Select(o => o.GetInt32()));
            return;
        }
        outcome.IsOk.Should().BeTrue();
        // o Python de fora do Windows lê em modo texto (CRLF vira LF); o porte preserva o fim de linha do fonte
        outcome.NewSource!.Replace("\r\n", "\n").Should().Be(Read("preparar", name, "esperado_novo_fonte.abap"));
        outcome.Diff.Should().Be(Read("preparar", name, "esperado_diff.txt"));
        string.Join(", ", outcome.Ranges.Select(r => $"{r.Start}-{r.End}")).Should().Be(expectedJson.GetProperty("linhas_substituidas").GetString());
        outcome.NewLines.Should().Be(expectedJson.GetProperty("linhas_novas").GetInt32());
        if (source.Contains("\r\n", StringComparison.Ordinal))
            outcome.NewSource.Should().Contain("\r\n").And.NotMatch("*[^\r]\n*");
    }

    private static List<string> Lines(string text) => PyText.LinesOfFile(text);

    [GeneratedRegex(@"\d\d\.\d\d\.\d{4}")]
    private static partial Regex DateRegex();
}
