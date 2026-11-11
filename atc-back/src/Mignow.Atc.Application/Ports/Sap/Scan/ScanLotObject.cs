namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>Objeto de um lote (tipo TADIR + nome).</summary>
public sealed record ScanLotObject(string Type, string Name);
