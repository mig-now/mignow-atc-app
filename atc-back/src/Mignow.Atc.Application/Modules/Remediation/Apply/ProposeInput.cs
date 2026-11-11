using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Entrada do <see cref="ProposeEngine"/>: itens (entidades rastreadas, alteradas no lugar), fontes lidos do ambiente, colunas do CSV da sessão
/// (para achar a <c>location</c> do finding), autor/data dos marcadores e o modo (<c>kb</c> | <c>ia</c>). <see cref="Locations"/>: Finding ID →
/// <c>location</c> da extração (quando o CSV da sessão não tem a coluna).
/// </summary>
public sealed record ProposeInput(
    string Mode, string AuthorTag, DateOnly Date, IReadOnlyList<RemediationItem> Items, IReadOnlyList<SourceCache> Caches, IReadOnlyList<string> Columns,
    IReadOnlyDictionary<string, string>? Locations = null);
