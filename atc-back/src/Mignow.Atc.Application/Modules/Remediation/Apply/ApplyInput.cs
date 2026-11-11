using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Entrada do <see cref="ApplyEngine"/>: itens e fontes lidos (entidades rastreadas, alteradas no lugar), request da sessão, autor dos
/// marcadores, responsável (usuário SAP, na criação DDIC), <see cref="AllowTableStructure"/> (<c>-PermitirEstrutura</c>, só com aval do consultor)
/// e a criação DDIC por item.
/// </summary>
public sealed record ApplyInput(
    string? Request, string AuthorTag, string Responsible, IReadOnlyList<RemediationItem> Items, IReadOnlyList<SourceCache> Caches, bool AllowTableStructure,
    IReadOnlyDictionary<int, DdicCreation> DdicCreate);
