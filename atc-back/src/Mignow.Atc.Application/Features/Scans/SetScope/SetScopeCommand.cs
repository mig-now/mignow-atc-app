using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.SetScope;

/// <summary>Regras ativas da varredura (as demais ficam <c>ativa = false</c> no snapshot). O gerador usa só as ativas.</summary>
public sealed record SetScopeCommand(Guid Id, IReadOnlyList<string> RuleIds) : ICommand<Result<ScanResponse>>;
