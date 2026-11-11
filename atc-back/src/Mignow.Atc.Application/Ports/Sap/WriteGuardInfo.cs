namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>Escrita liberada: a categoria lida em T000-CCCATEGORY (nunca P).</summary>
public sealed record WriteGuardInfo(string ClientCategory);
