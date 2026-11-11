namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>Um processo da varredura: a classe temporária (<c>ZCL_MIGNOW_ATC_SCAN_n</c>) e os lotes que ela roda, em fila.</summary>
public sealed record ScanProcessRequest(string ClassName, IReadOnlyList<ScanLotRequest> Lots);
