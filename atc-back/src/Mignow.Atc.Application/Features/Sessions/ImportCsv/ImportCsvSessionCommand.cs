using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.ImportCsv;

/// <summary>Sessão a partir de um CSV enviado (multipart). Exige as colunas Object name e Obj.</summary>
public sealed record ImportCsvSessionCommand(Guid SapSystemId, string FileName, byte[] Content, string? Priorities, string? Request) : ICommand<Result<SessionResponse>>;
