namespace Mignow.Atc.Domain.Remediation;

/// <summary>
/// Preparação do item (<c>preparacao</c> do atc.py): fonte físico, faixas de linhas alteradas, novo fonte, diff e
/// objetos relacionados preparados junto (ex.: interface cuja assinatura muda com a classe). Coluna jsonb.
/// </summary>
public sealed record ItemPreparation(
    string? Source,
    string? SourceUri,
    IReadOnlyList<LineRange>? Ranges,
    string? NewSource,
    string? Diff,
    IReadOnlyList<RelatedPreparation>? Related,
    DateTimeOffset? PreparedAt);

public sealed record LineRange(int Start, int End);

/// <summary>Preparação de um objeto relacionado ao item.</summary>
public sealed record RelatedPreparation(string ObjectName, string ObjectType, string? Source, string? SourceUri, string? NewSource, string? Diff);
