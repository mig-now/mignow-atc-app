using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.History;

public sealed record GetSessionHistoryQuery(Guid SessionId) : IQuery<Result<SessionHistory>>;
