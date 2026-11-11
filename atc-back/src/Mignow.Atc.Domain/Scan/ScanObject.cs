namespace Mignow.Atc.Domain.Scan;

/// <summary>Objeto do escopo da varredura (linha do <c>escopo.csv</c>): tipo TADIR (PROG/FUGR/CLAS/INTF), nome, pacote e autor.</summary>
public sealed record ScanObject(string Type, string Name, string Package, string Author);
