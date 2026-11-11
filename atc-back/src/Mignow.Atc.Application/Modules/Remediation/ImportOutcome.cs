namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Resultado de <see cref="SessionImporter.Import"/>: ou os itens + resumo, ou o código do erro (colunas faltando).</summary>
public sealed record ImportOutcome(IReadOnlyList<ImportedRow> Rows, ImportSummary? Summary, string? ErrorCode, IReadOnlyList<string> Header)
{
    public bool IsFailure => ErrorCode is not null;
}
