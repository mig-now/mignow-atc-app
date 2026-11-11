namespace Mignow.Atc.Application.Dtos.Extractions;

/// <summary>CSV gerado (UTF-8 com BOM, <c>;</c>) e o nome do arquivo da skill.</summary>
public sealed record ExtractionFileResponse(string FileName, byte[] Content);
