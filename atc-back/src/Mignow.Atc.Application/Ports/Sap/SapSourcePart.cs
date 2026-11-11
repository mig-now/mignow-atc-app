namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// Um fonte físico que compõe o objeto (porte de <c>Get-AdtObjectSources</c>): nome (programa/include), parte
/// (<c>main</c>, <c>definitions</c>, <c>implementations</c>, <c>macros</c>) e a URI ADT do fonte (<c>…/source/main</c>).
/// </summary>
public sealed record SapSourcePart(string Name, string Part, string Uri);
