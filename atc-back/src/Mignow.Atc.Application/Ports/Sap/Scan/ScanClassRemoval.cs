namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>
/// Remoção de uma classe temporária (lock + DELETE; se falhar, a API <c>/deletion</c>) conferida na TADIR. Só apaga classe
/// do pacote <c>$TMP</c>. <see cref="Situation"/>: <c>removida</c>, <c>nao_existia</c> ou <c>NÃO REMOVIDA: …</c>.
/// </summary>
public sealed record ScanClassRemoval(string ClassName, bool Removed, string Situation, IReadOnlyList<string> Attempts);
