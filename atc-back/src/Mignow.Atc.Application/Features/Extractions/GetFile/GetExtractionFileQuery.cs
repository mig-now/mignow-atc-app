using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Modules.Extraction;

namespace Mignow.Atc.Application.Features.Extractions.GetFile;

/// <summary>
/// CSV da extração: total, <c>_novos</c> (sem baseline) ou <c>_remediacao</c> (layout da atc-remediacao, só os novos).
/// <see cref="Priorities"/> (ex.: <c>1,2</c>) restringe as linhas; vazio = todas (arquivo idêntico ao da skill).
/// </summary>
public sealed record GetExtractionFileQuery(Guid ExtractionId, ExtractionFileKind Kind, string? Priorities) : IQuery<Result<ExtractionFileResponse>>;
