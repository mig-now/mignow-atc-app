namespace Mignow.Atc.Application.Dtos.Scans;

public sealed record ScanFileResponse(string FileName, string ContentType, byte[] Content);
