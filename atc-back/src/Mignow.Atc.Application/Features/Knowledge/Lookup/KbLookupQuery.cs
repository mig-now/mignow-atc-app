using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Application.Features.Knowledge.Lookup;

/// <summary>Porte do <c>kb_lookup</c>: os <c>Top</c> melhores cartões para o objeto referenciado (<c>Robt</c> = tipo do RObT do ATC).</summary>
public sealed record KbLookupQuery(string Ref, string? Robt = null, int Top = 3) : IQuery<Result<KbLookupResult>>;
