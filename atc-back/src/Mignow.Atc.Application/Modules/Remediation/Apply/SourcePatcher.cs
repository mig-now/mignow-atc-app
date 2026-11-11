using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Monta o fonte completo com os blocos no lugar dos trechos atuais e o diff (porte do núcleo do <c>cmd_preparar</c>, sem acessar o SAP):
/// cada <see cref="PatchSpec.Before"/> precisa aparecer exatamente uma vez no fonte (comparando sem espaços no fim) — se se repetir,
/// <see cref="PatchSpec.Line"/> escolhe a ocorrência mais próxima; trechos sobrepostos são erro; o fim de linha do fonte (<c>\r\n</c> ou
/// <c>\n</c>) é preservado e o fonte termina com um terminador.
/// </summary>
public static class SourcePatcher
{
    public static PatchOutcome Patch(string source, IReadOnlyList<PatchSpec> specs, string sourceName)
    {
        if (specs.Count == 0)
            return PatchOutcome.Fail("Apply.Patch.Empty");
        var eol = source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = PyText.SplitLines(source);
        var normalized = PyText.Normalize(lines);
        List<(int Pos, int Len, IReadOnlyList<string> Block)> patches = [];
        foreach (var spec in specs)
        {
            var na = PyText.Normalize(spec.Before);
            List<int> positions = [];
            if (na.Count > 0)
                for (var i = 0; i + na.Count <= normalized.Count; i++)
                    if (Matches(normalized, i, na))
                        positions.Add(i);
            if (positions.Count > 1 && spec.Line is { } line)
            {
                var target = line - 1;
                positions = [positions.MinBy(p => Math.Abs(p - target))];
            }
            if (positions.Count != 1)
                return PatchOutcome.Fail("Apply.Patch.NotUnique", [.. positions.Select(p => p + 1)]);
            patches.Add((positions[0], spec.Before.Count, spec.Block));
        }
        patches = [.. patches.OrderBy(p => p.Pos)];
        for (var i = 0; i + 1 < patches.Count; i++)
            if (patches[i].Pos + patches[i].Len > patches[i + 1].Pos)
                return PatchOutcome.Fail("Apply.Patch.Overlap", [patches[i].Pos + 1, patches[i + 1].Pos + 1]);
        List<string> result = [];
        var last = 0;
        foreach (var (pos, len, block) in patches)
        {
            result.AddRange(lines.Skip(last).Take(pos - last));
            result.AddRange(block);
            last = pos + len;
        }
        result.AddRange(lines.Skip(last));
        var diff = UnifiedDiff.Compute(lines, result, $"{sourceName} (atual)", $"{sourceName} (corrigido)");
        var ranges = patches.Select(p => new LineRange(p.Pos + 1, p.Pos + p.Len)).ToList();
        return new PatchOutcome(string.Join(eol, result) + eol, string.Join("\n", diff) + "\n", ranges, ranges[0].Start, ranges[^1].End,
            patches.Sum(p => p.Block.Count), null, []);
    }

    private static bool Matches(List<string> haystack, int at, List<string> needle)
    {
        for (var k = 0; k < needle.Count; k++)
            if (haystack[at + k] != needle[k])
                return false;
        return true;
    }
}
