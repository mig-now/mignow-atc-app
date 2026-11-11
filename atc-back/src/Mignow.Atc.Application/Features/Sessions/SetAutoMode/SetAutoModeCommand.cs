using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.SetAutoMode;

/// <summary>Liga/desliga o modo automático (sem confirmação do consultor) da sessão.</summary>
public sealed record SetAutoModeCommand(Guid SessionId, bool Enabled) : ICommand<Result<SessionResponse>>;
