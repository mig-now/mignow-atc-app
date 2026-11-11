using System.Text.RegularExpressions;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Objeto do cliente x standard e a fila de trabalho (<c>eh_standard</c>, <c>fila_pendentes</c>, <c>NO_CODE_TYPES</c> do atc.py). Objeto do
/// cliente: Z*, Y*, namespace /XXX/ ou programa gerado de cliente (SAPMZ*, SAPLZ*, LZ*, MZ*…); os demais (SAPMV45A…) vão para o fim da
/// fila e só são lidos com confirmação (regra R6).
/// </summary>
public static partial class StandardObjects
{
    /// <summary>Tipos sem código ABAP: o fonte não é lido (a triagem R1 trata esses casos).</summary>
    public static readonly IReadOnlySet<string> NoCodeTypes =
        new HashSet<string>(StringComparer.Ordinal) { "SHLP", "TABL", "VIEW", "DTEL", "DOMA", "TTYP", "MSAG", "TABU", "DEVC", "SHMA", "STRU" };

    public static bool IsStandard(string? objectName) => !string.IsNullOrEmpty(objectName) && !CustomerRegex().IsMatch(objectName.Trim());

    /// <summary>Pendentes na ordem de trabalho: objetos do cliente primeiro, standard no fim (cada grupo na ordem do CSV).</summary>
    public static List<RemediationItem> PendingQueue(IEnumerable<RemediationItem> items)
    {
        var pending = items.Where(i => i.Status.Length == 0).ToList();
        return [.. pending.Where(i => !IsStandard(i.ObjectName)), .. pending.Where(i => IsStandard(i.ObjectName))];
    }

    [GeneratedRegex(@"^(SAP[ML]|[LM])?(Z|Y|/[^/]+/)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CustomerRegex();
}
