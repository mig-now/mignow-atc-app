using System.Text;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Leitura e escrita do CSV do ATC (porte de <c>read_csv</c>/<c>col</c> e do <c>csv.writer</c> de <c>cmd_exportar</c>).</summary>
public static class AtcCsv
{
    public const string Utf8Sig = "utf-8-sig";
    public const string Cp1252 = "cp1252";

    /// <summary>Tenta utf-8-sig (estrito) e cai para cp1252; delimitador <c>;</c> salvo se a 1ª linha tiver mais vírgulas. Vazio = nulo.</summary>
    public static CsvDocument? Read(byte[] raw)
    {
        string text;
        string encoding;
        try
        {
            text = new UTF8Encoding(false, true).GetString(raw);
            if (text.Length > 0 && text[0] == '﻿')
                text = text[1..];
            encoding = Utf8Sig;
        }
        catch (DecoderFallbackException)
        {
            text = Cp1252Encoding().GetString(raw);
            encoding = Cp1252;
        }
        if (text.Length == 0)
            return null;
        var end = text.IndexOfAny(['\r', '\n']);
        var first = end < 0 ? text : text[..end];
        var delimiter = first.Count(c => c == ';') >= first.Count(c => c == ',') ? ';' : ',';
        var rows = Parse(text, delimiter);
        if (rows.Count == 0)
            return null;
        return new CsvDocument(encoding, delimiter.ToString(), rows[0], rows.Skip(1).ToList());
    }

    /// <summary>Índice da 1ª coluna cujo nome (sem espaços nas pontas, sem diferenciar maiúsculas) é um dos <paramref name="names"/>.</summary>
    public static int? Col(IReadOnlyList<string> header, params string[] names)
    {
        var low = header.Select(h => h.Trim().ToLowerInvariant()).ToList();
        foreach (var n in names)
        {
            var i = low.IndexOf(n.ToLowerInvariant());
            if (i >= 0)
                return i;
        }
        return null;
    }

    /// <summary>Máquina de estados do <c>csv.reader</c> do Python (aspas duplas, campo com quebra de linha, linha em branco = lista vazia).</summary>
    public static List<IReadOnlyList<string>> Parse(string text, char delimiter)
    {
        var rows = new List<IReadOnlyList<string>>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var state = State.StartRecord;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var newline = c is '\n' or '\r';
            switch (state)
            {
                case State.StartRecord:
                    if (newline)
                    {
                        SkipPairedNewline(text, ref i);
                        rows.Add([]);
                        break;
                    }
                    state = State.StartField;
                    goto case State.StartField;
                case State.StartField:
                    if (newline)
                    {
                        EndRecord(rows, fields, field, text, ref i);
                        state = State.StartRecord;
                    }
                    else if (c == '"')
                        state = State.InQuoted;
                    else if (c == delimiter)
                        SaveField(fields, field);
                    else
                    {
                        field.Append(c);
                        state = State.InField;
                    }
                    break;
                case State.InField:
                    if (newline)
                    {
                        EndRecord(rows, fields, field, text, ref i);
                        state = State.StartRecord;
                    }
                    else if (c == delimiter)
                    {
                        SaveField(fields, field);
                        state = State.StartField;
                    }
                    else
                        field.Append(c);
                    break;
                case State.InQuoted:
                    if (c == '"')
                        state = State.QuoteInQuoted;
                    else
                        field.Append(c);
                    break;
                case State.QuoteInQuoted:
                    if (c == '"')
                    {
                        field.Append('"');
                        state = State.InQuoted;
                    }
                    else if (c == delimiter)
                    {
                        SaveField(fields, field);
                        state = State.StartField;
                    }
                    else if (newline)
                    {
                        EndRecord(rows, fields, field, text, ref i);
                        state = State.StartRecord;
                    }
                    else
                    {
                        field.Append(c);
                        state = State.InField;
                    }
                    break;
            }
        }
        if (state != State.StartRecord)
        {
            SaveField(fields, field);
            rows.Add([.. fields]);
        }
        return rows;
    }

    /// <summary>CSV de saída como o <c>csv.writer</c> (aspas só quando preciso, fim de linha CRLF) na codificação de origem (utf-8-sig com BOM).</summary>
    public static byte[] Write(string encoding, string delimiter, IReadOnlyList<string> header, IEnumerable<IReadOnlyList<string>> rows)
    {
        var d = delimiter.Length == 0 ? ';' : delimiter[0];
        var sb = new StringBuilder();
        AppendRow(sb, header, d);
        foreach (var row in rows)
            AppendRow(sb, row, d);
        var text = sb.ToString();
        if (encoding == Cp1252)
            return Cp1252Encoding().GetBytes(text);
        return [.. new UTF8Encoding(true).GetPreamble(), .. new UTF8Encoding(false).GetBytes(text)];
    }

    private static void AppendRow(StringBuilder sb, IReadOnlyList<string> row, char d)
    {
        for (var k = 0; k < row.Count; k++)
        {
            if (k > 0)
                sb.Append(d);
            var f = row[k];
            var quote = f.Length == 0 ? row.Count == 1 : f.IndexOfAny([d, '"', '\r', '\n']) >= 0;
            if (quote)
                sb.Append('"').Append(f.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"');
            else
                sb.Append(f);
        }
        sb.Append("\r\n");
    }

    private static void SaveField(List<string> fields, StringBuilder field)
    {
        fields.Add(field.ToString());
        field.Clear();
    }

    private static void EndRecord(List<IReadOnlyList<string>> rows, List<string> fields, StringBuilder field, string text, ref int i)
    {
        SaveField(fields, field);
        rows.Add([.. fields]);
        fields.Clear();
        SkipPairedNewline(text, ref i);
    }

    private static void SkipPairedNewline(string text, ref int i)
    {
        if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            i++;
    }

    private static Encoding Cp1252Encoding() => CodePagesEncodingProvider.Instance.GetEncoding(1252)!;

    private enum State
    {
        StartRecord,
        StartField,
        InField,
        InQuoted,
        QuoteInQuoted,
    }
}
