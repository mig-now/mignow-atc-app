namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>Parâmetros do run <c>ConfirmR1</c> (<c>conferir-r1</c>): itens (padrão: os marcados pela R1) e se grava (<c>--aplicar</c>) ou só propõe.</summary>
public sealed record ConfirmR1Parameters(Guid SessionId, IReadOnlyList<int>? ItemNs, bool Apply);
