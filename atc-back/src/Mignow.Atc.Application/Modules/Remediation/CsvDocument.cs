namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// CSV do ATC lido como o <c>read_csv</c> do atc.py: <see cref="Encoding"/> = <c>utf-8-sig</c> ou <c>cp1252</c> (o que decodificou),
/// <see cref="Delimiter"/> = <c>;</c> ou <c>,</c> pela primeira linha, <see cref="Rows"/> sem o cabeçalho (linha em branco = lista vazia, como o
/// <c>csv.reader</c>; o número do item é a posição da linha, a partir de 1).
/// </summary>
public sealed record CsvDocument(string Encoding, string Delimiter, IReadOnlyList<string> Header, IReadOnlyList<IReadOnlyList<string>> Rows);
