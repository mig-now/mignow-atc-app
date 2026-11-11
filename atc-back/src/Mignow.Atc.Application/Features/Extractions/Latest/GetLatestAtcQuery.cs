using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;

namespace Mignow.Atc.Application.Features.Extractions.Latest;

/// <summary>Verifica no SAP a última execução concluída do ATC (série ZMIG_/Z_MIG_) e se já foi extraída (síncrono, só leitura).</summary>
public sealed record GetLatestAtcQuery(Guid SapSystemId) : IQuery<Result<AtcLatestResponse>>;
