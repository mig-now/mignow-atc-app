namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Resultado do <see cref="ChangeMarker"/>: as linhas do bloco ou o código do erro (mais o argumento da mensagem).</summary>
public sealed record MarkerResult(IReadOnlyList<string> Lines, string? ErrorCode, string? ErrorArg)
{
    public bool IsOk => ErrorCode is null;

    public static MarkerResult Ok(IReadOnlyList<string> lines) => new(lines, null, null);

    public static MarkerResult Fail(string code, string? arg = null) => new([], code, arg);
}
