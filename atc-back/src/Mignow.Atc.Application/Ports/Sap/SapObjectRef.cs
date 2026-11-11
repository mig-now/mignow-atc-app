namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>Objeto do repositório: nome, tipo ADT (PROG/P, PROG/I, CLAS/OC, FUGR/F, FUGR/FF...), URI ADT e pacote.</summary>
public sealed record SapObjectRef(string Name, string Type, string Uri, string Package, string? Description = null);
