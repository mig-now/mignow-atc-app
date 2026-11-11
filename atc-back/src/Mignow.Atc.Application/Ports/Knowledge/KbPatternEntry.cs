namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Linha de <c>indices/por_padrao.md</c>: padrão (P01..P99), descrição, nº de soluções e de ocorrências.</summary>
public sealed record KbPatternEntry(string Id, string Description, int Solutions, int Occurrences);
