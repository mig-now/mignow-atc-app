using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Sessions.StartReadSources;

/// <summary>Run <c>ReadSources</c>: lote = N primeiros pendentes da fila (padrão: <c>BatchSize</c> dos parâmetros); standard só com <c>IncludeStandard</c>.</summary>
public sealed record StartReadSourcesCommand(Guid SessionId, int? Batch, bool IncludeStandard, bool Force) : ICommand<Result<RunResponse>>;
