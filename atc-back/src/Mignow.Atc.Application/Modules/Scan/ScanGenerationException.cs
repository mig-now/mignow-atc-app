namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>Linha ABAP gerada com mais de 255 caracteres (o ADT/ABAP recusa); mesma checagem do <c>gerar.py</c>.</summary>
public sealed class ScanGenerationException(int lot, IReadOnlyList<int> lines)
    : Exception($"Lote {lot}: linhas ABAP com mais de 255 caracteres ({string.Join(", ", lines)}).")
{
    public int Lot { get; } = lot;

    public IReadOnlyList<int> Lines { get; } = lines;
}
