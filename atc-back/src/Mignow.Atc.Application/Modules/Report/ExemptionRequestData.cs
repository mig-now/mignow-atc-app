using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Pedido de isenção ligado ao item (<c>n</c> do CSV) da sessão.</summary>
public sealed record ExemptionRequestData(ExemptionRequest Request, int ItemN);
