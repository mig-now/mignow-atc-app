using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Report;

/// <summary>
/// Complementos manuais do relatório final de um cliente (equivale ao <c>resultado/complementos.json</c> da skill
/// atc-resultado): destaques e pendências em texto, com fonte, e as premissas da comparação manual × IA.
/// Uma linha por cliente.
/// </summary>
public sealed class ReportHighlights : EntityBase
{
    private ReportHighlights() { }

    public int CustomerId { get; private set; }

    public IReadOnlyList<HighlightText> Highlights { get; private set; } = [];

    public IReadOnlyList<PendingText> Pending { get; private set; } = [];

    public ManualPremise? Premise { get; private set; }

    public static ReportHighlights Create(int customerId)
    {
        if (customerId <= 0)
            throw new DomainException("ReportHighlights.CustomerRequired", "Os complementos pertencem a um cliente.");
        return new ReportHighlights { CustomerId = customerId };
    }

    public void Update(IEnumerable<HighlightText> highlights, IEnumerable<PendingText> pending, ManualPremise? premise)
    {
        var h = highlights.Select(x => new HighlightText(Guard.Required(x.Texto, "ReportHighlights.TextRequired", 2000), Guard.Optional(x.Fonte, 300))).ToList();
        var p = pending.Select(x => new PendingText(Guard.Required(x.Titulo, "ReportHighlights.TitleRequired", 200),
            Guard.Required(x.Texto, "ReportHighlights.TextRequired", 2000), Guard.Optional(x.Fonte, 300))).ToList();
        if (premise is not null && (premise.ObjectsPerDay is < 1 || premise.HoursPerDay is < 1 or > 24))
            throw new DomainException("ReportHighlights.InvalidPremise", "Premissas fora da faixa (objetos/dia ≥ 1, horas/dia entre 1 e 24).");
        Highlights = h;
        Pending = p;
        Premise = premise is null ? null : premise with { Source = Guard.Optional(premise.Source, 300) };
    }
}
