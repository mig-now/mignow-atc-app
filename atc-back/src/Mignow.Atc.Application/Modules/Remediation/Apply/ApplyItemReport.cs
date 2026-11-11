namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Resultado de um item na simulação/aplicação/rollback.</summary>
public sealed record ApplyItemReport(int N, string ObjectName, string Result, string? Detail);
