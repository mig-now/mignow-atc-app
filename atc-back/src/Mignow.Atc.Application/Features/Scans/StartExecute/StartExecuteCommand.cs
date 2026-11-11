using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Scans.StartExecute;

/// <summary>Enfileira o run <c>ScanExecute</c> da varredura (202 com o run).</summary>
public sealed record StartExecuteCommand(Guid Id, string? Lots = null, bool Redo = false, int? Parallel = null, bool IncludeOtherLine = false, bool CompareOriginal = true) : ICommand<Result<RunResponse>>;
