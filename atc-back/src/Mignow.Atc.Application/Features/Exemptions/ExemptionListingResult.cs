using Mignow.Atc.Application.Dtos.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions;

public sealed record ExemptionListingResult(IReadOnlyList<ExemptionListingEntryData> Entries, ExemptionListingReport Report);
