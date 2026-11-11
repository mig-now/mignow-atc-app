using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Scans.StartInventory;

/// <summary>Enfileira o run <c>ScanInventory</c> da varredura (202 com o run).</summary>
public sealed record StartInventoryCommand(Guid Id, bool? IncludeGenerated = null) : ICommand<Result<RunResponse>>;
