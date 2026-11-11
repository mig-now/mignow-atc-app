namespace Mignow.Atc.Application.Ports.Sap.Ddic;

/// <summary>Estado de uma estrutura/tabela no sistema: existe? e o DDL ativo (<c>source/main</c>).</summary>
public sealed record DdicObjectState(bool Exists, string? Source);
