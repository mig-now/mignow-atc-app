using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Application.Features.Knowledge.Index;

public sealed record KbIndexResponse(IReadOnlyList<KbIndexEntry> Elements, IReadOnlyList<KbPatternEntry> Patterns);
