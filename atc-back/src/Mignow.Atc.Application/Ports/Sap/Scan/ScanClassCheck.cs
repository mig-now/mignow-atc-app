namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>
/// Verificação de uma classe temporária (somente leitura): se já existe (e em que pacote) e, se não existe, o resultado da
/// validação do nome no ADT (<c>/oo/validation/objectname</c>, pacote <c>$TMP</c>).
/// </summary>
public sealed record ScanClassCheck(string ClassName, bool Exists, string Package, bool ValidationOk, string? ValidationMessage);
