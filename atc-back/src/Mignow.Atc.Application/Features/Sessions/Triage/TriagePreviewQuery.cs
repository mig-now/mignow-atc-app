using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.Triage;

/// <summary>Prévia da triagem R1..R9 + produtos MIGNOW: contagens por regra e itens afetados, sem gravar.</summary>
public sealed record TriagePreviewQuery(Guid SessionId) : IQuery<Result<TriageResponse>>;
