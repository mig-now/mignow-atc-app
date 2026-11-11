using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Groups;

/// <summary>Grupos do lote (<c>cmd_grupos</c>); <c>Batch</c> padrão = <c>BatchSize</c> dos parâmetros.</summary>
public sealed record GetGroupsQuery(Guid SessionId, int? Batch) : IQuery<Result<GroupingResult>>;
