namespace Mignow.Atc.Api.Contracts;

/// <summary>Corpo de <c>POST sessions/{id}/propose</c>: itens e/ou grupos (<c>C|REF|ROBT|forma</c>), modo <c>kb</c>|<c>ia</c> e confirmação automática (sessão em modo sem confirmação).</summary>
public sealed record SessionApplyProposeRequest(IReadOnlyList<Guid>? ItemIds, IReadOnlyList<string>? GroupKeys, string Mode, bool AutoConfirm = false);
