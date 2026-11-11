namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>
/// Linha do índice da KB (por elemento): categoria (pasta: tabelas, funcoes, ... ou <c>fatos</c> quando só há fatos),
/// nome de exibição (título do arquivo), slug (nome do arquivo, usado na rota), nº de cartões da base, contribuições e fatos.
/// </summary>
public sealed record KbIndexEntry(string Category, string Element, string Slug, int Cards, int Contributions, int Facts);
