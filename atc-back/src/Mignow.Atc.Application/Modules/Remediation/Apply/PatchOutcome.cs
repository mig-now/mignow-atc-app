using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Saída do <see cref="SourcePatcher"/>: fonte novo, diff unificado (como <c>difflib</c>, n=2), faixas das linhas substituídas
/// (no fonte original), linha inicial/final e quantas linhas entraram. <see cref="ErrorCode"/>: <c>Apply.Patch.NotUnique</c>
/// (<see cref="Occurrences"/> = as linhas onde o trecho aparece), <c>Apply.Patch.Overlap</c>, <c>Apply.Patch.CountMismatch</c>, <c>Apply.Patch.Empty</c>.
/// </summary>
public sealed record PatchOutcome(
    string? NewSource, string? Diff, IReadOnlyList<LineRange> Ranges, int StartLine, int EndLine, int NewLines, string? ErrorCode,
    IReadOnlyList<int> Occurrences, string? ErrorArg = null)
{
    public bool IsOk => ErrorCode is null;

    public static PatchOutcome Fail(string code, IReadOnlyList<int>? occurrences = null, string? arg = null) =>
        new(null, null, [], 0, 0, 0, code, occurrences ?? [], arg);
}
