namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>
/// Parâmetros do run <c>Propose</c>: itens e/ou grupos (<c>C|REF|ROBT|forma</c>; sem os dois = todos os pendentes), modo <c>kb</c>|<c>ia</c> e
/// <see cref="AutoConfirm"/> (modo sem confirmação). <see cref="Reprocess"/>: itens confirmados cuja gravação falhou e que o play reaplica
/// (simular → revisar → aplicar), mesmo sem terem sido confirmados pelo modo automático.
/// </summary>
public sealed record ProposeParameters(Guid SessionId, IReadOnlyList<Guid>? ItemIds, IReadOnlyList<string>? GroupKeys, string Mode, bool AutoConfirm,
    IReadOnlyList<Guid>? Reprocess = null);
