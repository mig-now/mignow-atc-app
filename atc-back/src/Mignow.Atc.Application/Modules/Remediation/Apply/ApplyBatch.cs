namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Itens processados e, se a execução precisa parar (trava de escrita, falha do SAP), o código do motivo.</summary>
public sealed record ApplyBatch(IReadOnlyList<ApplyItemReport> Reports, string? StopCode, string? StopMessage);
