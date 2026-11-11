namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Ocorrência do trecho atual no fonte lido (fonte e linha).</summary>
public sealed record ReuseOccurrence(string Source, int Line);

/// <summary>
/// Resultado do reaproveitamento de um item (<c>cmd_reaproveitar</c>). <see cref="Situation"/> = <c>ok</c> | <c>ok_trecho_repetido_use_linha</c> |
/// <c>sem_ficha_na_origem</c> | <c>ficha_sem_codigo</c> | <c>varios_blocos_revisar</c> | <c>codigo_atual_mudou</c>. Em <c>ok*</c> vêm o trecho atual,
/// o novo e o bloco com marcadores (os <c>antes/depois/bloco.abap</c> do script).
/// </summary>
public sealed record ReuseCandidate(
    int N, string ObjectName, string Ref, string Situation, int? OriginN = null, string? OriginStatus = null, string? OriginObs = null,
    IReadOnlyList<ReuseOccurrence>? Occurrences = null, int? LinesBefore = null, int? LinesAfter = null,
    IReadOnlyList<string>? Before = null, IReadOnlyList<string>? After = null, IReadOnlyList<string>? Block = null);
