using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Scans.StartCleanup;

/// <summary>Enfileira o run <c>ScanCleanup</c> da varredura (202 com o run).</summary>
public sealed record StartCleanupCommand(Guid Id) : ICommand<Result<RunResponse>>;
