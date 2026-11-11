using System.Globalization;
using System.Text.RegularExpressions;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>Regras do texto do pedido e das prioridades (porte do <c>solicitar.ps1</c> e do <c>listar.py</c>).</summary>
public static partial class ExemptionRules
{
    public static readonly IReadOnlyList<string> ValidPriorities = ["1", "2", "3"];

    public static readonly IReadOnlyList<string> ValidValidities = ["U", "D", "P"];

    /// <summary>
    /// Justificativa do pedido: <c>&lt;prefixo do lote&gt;S/4 MIGRATION MIG-NOW dd.MM.yyyy AUT-XX: &lt;obs do item&gt;</c>. A obs perde o
    /// "Triagem Rn:" do início; o prefixo do lote (<c>{MOTIVO} - POS - ATC - MIGNOW | </c> por padrão) troca <c>{MOTIVO}</c>
    /// pelo código do motivo e <c>''</c> = sem prefixo.
    /// </summary>
    public static string Justification(ExemptionBatch batch, string reason, string? obs, string author, DateTimeOffset date)
    {
        var text = TriagePrefixRegex().Replace(obs ?? string.Empty, string.Empty);
        var justification = $"S/4 MIGRATION MIG-NOW {date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)} {author}: {text}";
        var prefix = batch.EffectivePrefix(reason);
        return prefix.Length > 0 ? prefix + justification : justification;
    }

    /// <summary>"1,2" → {1,2}; vazio ou valor fora de 1..3 → nulo.</summary>
    public static IReadOnlyList<string>? ParsePriorities(IEnumerable<int>? priorities)
    {
        if (priorities is null)
            return null;
        var list = priorities.Distinct().Order().Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList();
        return list.Count == 0 || list.Any(p => !ValidPriorities.Contains(p)) ? null : list;
    }

    /// <summary>"1,2" gravado no lote → conjunto de prioridades.</summary>
    public static IReadOnlyList<string> PrioritiesOf(string stored) =>
        [.. stored.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    /// <summary>O aprovador é o próprio solicitante (usuário SAP do ambiente)? O SAP exige aprovador diferente (princípio dos 4 olhos).</summary>
    public static bool IsRequester(string? approver, string? requester) =>
        !string.IsNullOrWhiteSpace(approver) && !string.IsNullOrWhiteSpace(requester) &&
        string.Equals(approver.Trim(), requester.Trim(), StringComparison.OrdinalIgnoreCase);

    public static bool IsExemptionObservation(string? obs) => obs is not null && ExemptionObsRegex().IsMatch(obs);

    [GeneratedRegex(@"^Triagem R\d+:\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TriagePrefixRegex();

    [GeneratedRegex("isen[çc][ãa]o", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ExemptionObsRegex();
}
