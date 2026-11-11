using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.Rules;

/// <summary>Regras do arquivo configurado em <c>GeneralSettings.MignowRulesPath</c> (somente leitura).</summary>
public sealed record ListScanRulesQuery : IQuery<Result<ScanRulesResponse>>;
