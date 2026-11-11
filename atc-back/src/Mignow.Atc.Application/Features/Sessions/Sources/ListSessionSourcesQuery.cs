using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.Sources;

/// <summary>Objetos da sessão cujos fontes já foram lidos (a leitura é automática ao carregar os itens e no play; não há disparo manual).</summary>
public sealed record ListSessionSourcesQuery(Guid SessionId) : IQuery<Result<SessionSourcesResponse>>;
