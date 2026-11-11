namespace Mignow.Atc.Application.Features.Sessions.Sources;

/// <summary>
/// Fontes lidos dos objetos da sessão (cache do ambiente): <c>Objects</c> = objetos da sessão com código (total), <c>Read</c> = quantos têm fonte
/// lido; cada objeto lido traz os includes com linhas e data da leitura.
/// </summary>
public sealed record SessionSourcesResponse(int Objects, int Read, IReadOnlyList<SessionSourceObject> Items);

/// <summary>Objeto da sessão com fonte lido: itens da sessão nele (total e pendentes) e os includes lidos.</summary>
public sealed record SessionSourceObject(string ObjectType, string ObjectName, int ItemCount, int Pending, IReadOnlyList<SessionSourceInclude> Includes);

public sealed record SessionSourceInclude(Guid Id, string IncludeName, int Lines, DateTimeOffset ReadAt);

/// <summary>Conteúdo de um fonte lido (somente leitura).</summary>
public sealed record SessionSourceContent(Guid Id, string ObjectName, string IncludeName, string AdtUri, DateTimeOffset ReadAt, string Source);
