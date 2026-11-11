using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.Monitor;

/// <summary>Monitor de remediação: sessões (de um cliente, opcional; arquivadas só a pedido), mais recentes primeiro, com a última execução.</summary>
public sealed record GetRemediationMonitorQuery(Guid? CustomerId = null, bool IncludeArchived = false) : IQuery<Result<IReadOnlyList<RemediationMonitorRow>>>;
