using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.Sources;

/// <summary>Um fonte lido de um objeto da sessão, para visualizar.</summary>
public sealed record GetSessionSourceQuery(Guid SessionId, Guid SourceId) : IQuery<Result<SessionSourceContent>>;
