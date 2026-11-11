using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// A "ficha" da correção (<c>correcoes/item_NNNN_&lt;obj&gt;.md</c> do <c>cmd_status</c>): cabeçalho do item, código atual e código corrigido.
/// O histórico de aplicação no SAP (<c>**Aplicação no SAP:**</c>) de uma ficha anterior é preservado.
/// </summary>
public static class CorrectionSheet
{
    public const string ApplicationPrefix = "**Aplicação no SAP:**";

    public static string Render(RemediationItem it, string status, string obs, string before, string after, string? previousSheet)
    {
        var applied = previousSheet is null
            ? []
            : previousSheet.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.StartsWith(ApplicationPrefix, StringComparison.Ordinal)).ToList();
        List<string> lines =
        [
            $"# Item {it.N} – {it.ObjectType} {it.ObjectName}", string.Empty,
            $"**Status:** {status}  ", $"**Check:** {it.Check} – {it.Message}  ",
            $"**Objeto referenciado:** {it.RefObject} ({it.RefType})  ", $"**Nota SAP:** {it.Note}  ",
            $"**Observação:** {obs}", string.Empty,
            "## Código atual", string.Empty, "```abap", before.TrimEnd(), "```", string.Empty,
            "## Código corrigido", string.Empty, "```abap", after.TrimEnd(), "```", string.Empty,
            .. applied.Select(x => x + "\n"),
        ];
        return string.Join('\n', lines);
    }

    /// <summary>Código atual e corrigido (com marcadores) da proposta do item; nulo sem proposta com bloco.</summary>
    public static (string Before, string After)? FromProposal(ItemProposal? proposal)
    {
        if (proposal is null || proposal.Blocks.Count == 0)
            return null;
        var before = string.Join('\n', proposal.Blocks.SelectMany(b => b.Before));
        var after = proposal.MarkedBlock ?? string.Join('\n', proposal.Blocks.SelectMany(b => b.After));
        return before.Length == 0 && after.Length == 0 ? null : (before, after);
    }
}
