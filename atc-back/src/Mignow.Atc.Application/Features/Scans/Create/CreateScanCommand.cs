using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.Create;

/// <summary>Nova varredura do ambiente, cruzada com a extração informada (padrão: a mais recente do ambiente); já enfileira o inventário.</summary>
public sealed record CreateScanCommand(Guid SapSystemId, Guid? ExtractionId, bool IncludeGenerated) : ICommand<Result<ScanStartedResponse>>;
