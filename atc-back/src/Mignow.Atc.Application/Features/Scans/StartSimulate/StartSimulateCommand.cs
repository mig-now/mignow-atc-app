using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Scans.StartSimulate;

/// <summary>Enfileira o run <c>ScanSimulate</c> da varredura (202 com o run).</summary>
public sealed record StartSimulateCommand(Guid Id, string? Lots = null, int? Parallel = null) : ICommand<Result<RunResponse>>;
