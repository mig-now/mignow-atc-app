using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.Get;

public sealed record GetSessionQuery(Guid Id) : IQuery<Result<SessionResponse>>;
