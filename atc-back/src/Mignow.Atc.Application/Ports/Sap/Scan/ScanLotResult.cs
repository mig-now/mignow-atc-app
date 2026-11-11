using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>
/// Resultado de um lote: <see cref="Status"/> = <see cref="ScanBatchStatus"/>; <see cref="Output"/> = texto do classrun
/// (linhas L/H/F/E/O/X/Z) quando houve execução; <see cref="Messages"/> = mensagens de ativação.
/// </summary>
public sealed record ScanLotResult(int Number, string Status, string Output, string? Message, IReadOnlyList<string> Messages, int ElapsedSeconds);
