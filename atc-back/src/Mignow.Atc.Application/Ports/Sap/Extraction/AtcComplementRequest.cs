namespace Mignow.Atc.Application.Ports.Sap.Extraction;

/// <summary>Chaves dos complementos: itens ATC, objetos (TADIR), notas e o idioma dos textos (<c>EN</c>, <c>PT</c>…).</summary>
public sealed record AtcComplementRequest(
    IReadOnlyCollection<string> ItemIds, IReadOnlyCollection<string> ObjectNames, IReadOnlyCollection<string> Notes, string Language);
