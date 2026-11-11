using Microsoft.AspNetCore.Mvc;

namespace Mignow.Atc.Api.Contracts;

/// <summary>Upload do CSV do ATC (multipart): arquivo, ambiente, prioridades ("1,2"; vazio = todas) e request.</summary>
public sealed class ImportCsvForm
{
    [FromForm(Name = "sapSystemId")]
    public Guid SapSystemId { get; set; }

    [FromForm(Name = "file")]
    public IFormFile? File { get; set; }

    [FromForm(Name = "priorities")]
    public string? Priorities { get; set; }

    [FromForm(Name = "request")]
    public string? Request { get; set; }
}
