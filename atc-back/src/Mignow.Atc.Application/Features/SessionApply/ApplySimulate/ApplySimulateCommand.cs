using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.RemediationApply;

namespace Mignow.Atc.Application.Features.SessionApply.ApplySimulate;

/// <summary>Run <c>ApplySimulate</c>: simulação (não grava no SAP) + revisão por IA dos itens Confirmado KB/IA. 202 com o run.</summary>
public sealed record ApplySimulateCommand(Guid SessionId, IReadOnlyList<Guid> ItemIds, bool AllowTableStructure, IReadOnlyList<DdicCreateParameter>? DdicCreate) : ICommand<Result<RunResponse>>;
