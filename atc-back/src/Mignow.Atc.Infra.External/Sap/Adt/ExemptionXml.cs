using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// XML do pedido de isenção do ATC (porte do <c>AtcIsencao.psm1</c>): leitura do modelo
/// (<c>atcexmpt:exemptionApply</c>, ST <c>SATC_XMPT_GET_TEMPLATE_RES</c>), montagem do corpo do POST (<c>SATC_EXEMPTION</c>)
/// e leitura da resposta (<c>SATC_XMPT_STATUS</c>: type S|I|E). Compartilhado pelo adapter ADT e pelo simulador.
/// </summary>
internal static partial class ExemptionXml
{
    public const string NsXmpt = "http://www.sap.com/adt/atc/exemption";
    public const string NsFinding = "http://www.sap.com/adt/atc/finding";
    public const string NsCore = "http://www.sap.com/adt/core";

    /// <summary>GET do modelo: o handler aceita só xmptapp/xmptprop (com <c>atc.xmpt.v1+xml</c> responde HTTP 406).</summary>
    public const string AcceptTemplate = "application/atc.xmptapp.v1+xml";

    /// <summary>POST do pedido (handler <c>create_atc_exmpt_apply_req_ch</c>).</summary>
    public const string ContentTypeRequest = "application/atc.xmpt.v1+xml";

    public const string Declaration = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

    private static readonly XNamespace Xmpt = NsXmpt;
    private static readonly XNamespace Finding = NsFinding;
    private static readonly XNamespace Core = NsCore;

    public static string ApplyPath(string markerId, string language) =>
        $"{AdtSession.AdtRoot}/atc/exemptions/apply?markerId={Uri.EscapeDataString(markerId)}&sap-language={language}";

    public static string PostPath(string markerId) => $"{AdtSession.AdtRoot}/atc/exemptions/apply?markerId={Uri.EscapeDataString(markerId)}";

    public static string Language(Mignow.Atc.Application.Ports.Sap.SapConnection c) =>
        string.IsNullOrWhiteSpace(c.Language) ? "EN" : c.Language.Trim().ToUpperInvariant();

    // ---------------------------------------------------------------- modelo (GET)

    /// <summary>Porte de <c>Get-XmptTemplate</c> (HTTP diferente de 200: mensagem <c>HTTP n</c>, tipo E).</summary>
    public static ExemptionTemplate ParseTemplate(int http, string body)
    {
        if (http != 200)
            return new ExemptionTemplate(http, false, $"HTTP {http}", "E", false, string.Empty, string.Empty, string.Empty, body);
        var doc = XDocument.Parse(body);
        var proposal = doc.Descendants(Xmpt + "exemptionProposal").FirstOrDefault();
        var status = doc.Descendants(Xmpt + "status").FirstOrDefault();
        var message = status?.Element(Xmpt + "message")?.Value ?? string.Empty;
        var type = status?.Element(Xmpt + "type")?.Value ?? string.Empty;
        if (proposal is null)
            return new ExemptionTemplate(http, false, message, type, false, string.Empty, string.Empty, string.Empty, body);
        var scope = Scope(proposal);
        return new ExemptionTemplate(http, true, message, type, scope.ThisFinding, scope.Checksum, scope.Type, scope.Reason, body);
    }

    public static XElement? Proposal(string templateXml) =>
        XDocument.Parse(templateXml).Descendants(Xmpt + "exemptionProposal").FirstOrDefault();

    /// <summary>
    /// Porte de <c>Get-XmptEscopo</c>: o modelo diz se o pedido pode ficar restrito a este finding
    /// (<c>restriction/thisFinding enabled</c>). Visto no EC2/300: desativado em finding sem checksum (-1) e em objeto
    /// ABAP Query (AQSG/AQQU).
    /// </summary>
    public static (bool ThisFinding, string Checksum, string Type, string Reason) Scope(XElement proposal)
    {
        var thisFinding = proposal.Element(Xmpt + "restriction")?.Element(Xmpt + "thisFinding");
        var finding = proposal.Element(Finding + "finding");
        var checksum = finding?.Attribute(Finding + "checksum")?.Value ?? string.Empty;
        var type = finding?.Attribute(Core + "type")?.Value ?? string.Empty;
        var ok = thisFinding?.Attribute("enabled")?.Value == "true";
        var reason = ok ? string.Empty
            : checksum == "-1" ? "finding sem checksum (-1)"
            : type is "AQSG" or "AQQU" or "AQBG" ? $"objeto ABAP Query ({type})"
            : $"tipo {type}, checksum {checksum}";
        return (ok, checksum, type, reason);
    }

    // ---------------------------------------------------------------- pedido (POST)

    /// <summary>
    /// Porte de <c>New-XmptRequest</c>: parte do modelo do SAP (mantém pacote, objeto, escopos permitidos) e preenche só o
    /// que cabe ao consultor. Escopo = somente este finding; <c>validUntil</c> 9999-12-31 quando a validade é U
    /// (99991231 dá HTTP 400 <c>ExceptionInvalidData</c>).
    /// </summary>
    public static string BuildRequest(XElement proposal, ExemptionApply apply)
    {
        var node = new XElement(proposal);
        // O modelo declara os prefixos (atcexmpt, atcfinding, adtcore) no elemento raiz; o ImportNode do script os mantém no nó importado.
        foreach (var declaration in proposal.Ancestors().Reverse().Attributes().Where(a => a.IsNamespaceDeclaration))
            if (node.Attribute(declaration.Name) is null)
                node.Add(new XAttribute(declaration.Name, declaration.Value));
        var restriction = node.Element(Xmpt + "restriction");
        var thisFinding = restriction?.Element(Xmpt + "thisFinding");
        if (thisFinding is null || thisFinding.Attribute("enabled")?.Value != "true")
            throw new InvalidOperationException("O modelo do SAP não permite isenção restrita a este finding (thisFinding)");
        thisFinding.Value = "true";
        restriction!.Element(Xmpt + "rangeOfFindings")?.SetAttributeValue("enabled", "false");
        SetValue(node, "approver", apply.Approver.ToUpperInvariant());
        SetValue(node, "reason", apply.Reason);
        SetValue(node, "justification", apply.Justification);
        SetValue(node, "notify", apply.Notify);
        if (apply.Validity.Length > 0)
            SetValue(node, "validity", apply.Validity);
        if (apply.Validity == "U")
            SetValue(node, "validUntil", "9999-12-31");
        return Declaration + node.ToString(SaveOptions.DisableFormatting);
    }

    private static void SetValue(XElement node, string name, string value)
    {
        var element = node.Element(Xmpt + name);
        if (element is null)
        {
            element = new XElement(Xmpt + name);
            node.Add(element);
        }
        element.Value = value;
    }

    /// <summary>
    /// Simulação de um finding (o que o <c>solicitar.ps1</c> faz antes do POST): sem modelo = BLOQUEADO (mensagem do SAP);
    /// sem escopo <c>thisFinding</c> = SEM_ESCOPO; senão SIMULADO com o corpo do pedido.
    /// </summary>
    public static ExemptionAttempt Evaluate(ExemptionTemplate template)
    {
        if (!template.HasProposal)
            return new ExemptionAttempt(ExemptionRequestStatus.Blocked, template.Message, template.Xml, null, null, null, null, null);
        if (!template.ThisFinding)
            return new ExemptionAttempt(ExemptionRequestStatus.NoScope,
                $"SAP não permite isenção só deste finding ({template.ScopeReason}); só intervalo objeto/mensagem", template.Xml, null, null, null, null,
                template.ScopeReason);
        return new ExemptionAttempt(ExemptionRequestStatus.Simulated, null, template.Xml, null, null, null, null, null);
    }

    public static ExemptionAttempt WithRequest(ExemptionAttempt attempt, ExemptionApply apply) =>
        attempt with { RequestXml = BuildRequest(Proposal(attempt.ModelXml!)!, apply) };

    // ---------------------------------------------------------------- resposta (POST)

    /// <summary>Mensagem e tipo da resposta; exceção ADT (<c>exc:exception</c>) = mensagem + caminho XML do erro, tipo E.</summary>
    public static (string Message, string Type) ParseStatus(string body)
    {
        var message = MessageRegex().Match(body).Groups[1].Value;
        var type = TypeRegex().Match(body).Groups[1].Value;
        if (message.Length == 0 && body.Contains("<exc:exception", StringComparison.Ordinal))
        {
            message = LocalizedMessageRegex().Match(body).Groups[1].Value;
            var path = XmlPathRegex().Match(body).Groups[1].Value;
            if (path.Length > 0)
                message = $"{message} ({path})";
            type = "E";
        }
        return (WebUtility.HtmlDecode(message), type);
    }

    /// <summary>PEDIDO (HTTP &lt; 300 e S) | INFO (I) | ERRO.</summary>
    public static string SubmitStatus(int http, string type) =>
        http < 300 && type == "S" ? ExemptionRequestStatus.Requested : type == "I" ? ExemptionRequestStatus.Info : ExemptionRequestStatus.Error;

    // ---------------------------------------------------------------- customizing e totais

    public static ExemptionCustomizing ParseCustomizing(string body)
    {
        var reasons = ReasonRegex().Matches(body)
            .Select(m => new ExemptionReason(m.Groups[1].Value, m.Groups[2].Value == "true", WebUtility.HtmlDecode(m.Groups[3].Value))).ToArray();
        var validities = ValidityRegex().Matches(body).Select(m => $"{m.Groups[1].Value} = {WebUtility.HtmlDecode(m.Groups[2].Value)}").ToArray();
        return new ExemptionCustomizing(reasons, validities);
    }

    public static IReadOnlyDictionary<string, int> ParseTotals(string body)
    {
        var totals = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (Match m in TotalRegex().Matches(body))
            totals[m.Groups[1].Value] = int.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        return totals;
    }

    /// <summary>Coleções ADT de isenção no discovery (<c>href</c> em <c>/sap/bc/adt/atc/(exemptions|checkexemptions)…</c>), únicas e ordenadas.</summary>
    public static IReadOnlyList<string> Collections(string discovery) =>
        [.. CollectionRegex().Matches(discovery).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

    [GeneratedRegex("<atcexmpt:message>([^<]*)</atcexmpt:message>")]
    private static partial Regex MessageRegex();

    [GeneratedRegex("<atcexmpt:type>([^<]*)</atcexmpt:type>")]
    private static partial Regex TypeRegex();

    [GeneratedRegex("<localizedMessage[^>]*>([^<]*)<")]
    private static partial Regex LocalizedMessageRegex();

    [GeneratedRegex("<entry key=\"XML_PATH\">([^<]*)<")]
    private static partial Regex XmlPathRegex();

    [GeneratedRegex("<reason id=\"([^\"]+)\" justificationMandatory=\"([^\"]+)\"(?: title=\"([^\"]*)\")?")]
    private static partial Regex ReasonRegex();

    [GeneratedRegex("<validity id=\"([^\"]+)\" value=\"([^\"]*)\"")]
    private static partial Regex ValidityRegex();

    [GeneratedRegex(@"<atcexempt:(count\w+|num\w+)>(\d+)<")]
    private static partial Regex TotalRegex();

    [GeneratedRegex("href=\"(/sap/bc/adt/atc/(?:exemptions|checkexemptions)[^\"]*)\"")]
    private static partial Regex CollectionRegex();
}
