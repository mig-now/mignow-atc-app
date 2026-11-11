using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.RemediationApply;

namespace Mignow.Atc.Application.Features.SessionApply.Apply;

/// <summary>Run <c>Apply</c>: grava no SAP (objeto por objeto, em fila, request da sessão). Só depois da confirmação do consultor; nunca em produção. 202 com o run.</summary>
public sealed record ApplyCommand(Guid SessionId, IReadOnlyList<Guid> ItemIds, bool AllowTableStructure, IReadOnlyList<DdicCreateParameter>? DdicCreate) : ICommand<Result<RunResponse>>;
