namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>Linha da TADIR do inventário (<c>R3TR</c> PROG/FUGR/CLAS/INTF com nome ou pacote Z*/Y*); flags vazios = não marcado.</summary>
public sealed record ScanTadirRow(string Type, string Name, string Package, string Author, string GenFlag, string DelFlag);
