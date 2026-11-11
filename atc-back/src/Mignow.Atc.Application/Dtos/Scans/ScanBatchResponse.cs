using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Dtos.Scans;

public sealed record ScanBatchResponse(int Number, int Weight, int ObjectCount, string ClassName, string? Status, bool ClassRemoved, ScanBatchStats? Stats);
