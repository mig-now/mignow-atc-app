using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.SetRequest;

/// <summary>Define (ou troca) a request de transporte da sessão: exigida para aplicar no SAP, inclusive pelo play.</summary>
public sealed record SetSessionRequestCommand(Guid SessionId, string Request) : ICommand<Result<SessionResponse>>;
