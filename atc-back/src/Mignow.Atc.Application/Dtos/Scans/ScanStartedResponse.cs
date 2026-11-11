using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Dtos.Scans;

/// <summary>Varredura criada e o run <c>ScanInventory</c> enfileirado (202).</summary>
public sealed record ScanStartedResponse(ScanResponse Scan, RunResponse Run);
