using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.Create;

/// <summary>Sessão a partir de uma extração (<c>_novos</c>: findings sem baseline) e das prioridades ("1", "1,2"; vazio = todas).</summary>
public sealed record CreateSessionCommand(Guid ExtractionId, string? Priorities, string? Request) : ICommand<Result<SessionResponse>>;
