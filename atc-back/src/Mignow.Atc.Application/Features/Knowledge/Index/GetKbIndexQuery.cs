using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Knowledge.Index;

/// <summary>Índices da KB: por elemento (filtráveis por categoria e texto) e por padrão.</summary>
public sealed record GetKbIndexQuery(string? Category = null, string? Search = null) : IQuery<Result<KbIndexResponse>>;
