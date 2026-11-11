namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Um trecho a substituir (<c>--antes</c>/<c>--bloco</c>/<c>--linha</c> do <c>preparar</c>): <see cref="Before"/> = trecho contíguo
/// atual, <see cref="Block"/> = o que entra no lugar (já com marcadores), <see cref="Line"/> desempata trecho repetido.
/// </summary>
public sealed record PatchSpec(IReadOnlyList<string> Before, IReadOnlyList<string> Block, int? Line = null);
