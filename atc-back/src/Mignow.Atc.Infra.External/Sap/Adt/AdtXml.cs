using System.Globalization;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mignow.Atc.Application.Ports.Sap;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Leitura e escrita do XML da ADT. Só o adapter conhece XML: daqui saem só os DTOs das portas. Os nomes são lidos pelo
/// nome local (sem depender do prefixo do namespace), porque variam entre releases.
/// </summary>
internal static partial class AdtXml
{
    public const string AdtCoreNs = "http://www.sap.com/adt/core";

    public static XDocument Parse(string xml) => XDocument.Parse(xml, LoadOptions.None);

    public static IEnumerable<XElement> All(XContainer root, string localName) =>
        root.Descendants().Where(e => e.Name.LocalName == localName);

    public static string? Attr(XElement e, string localName) =>
        e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value;

    public static string? ChildText(XElement e, string localName) =>
        e.Descendants().FirstOrDefault(d => d.Name.LocalName == localName)?.Value;

    /// <summary>adtcore:objectReference (busca rápida): nome, tipo, URI, pacote e descrição.</summary>
    public static IReadOnlyList<SapObjectRef> ObjectReferences(string xml, string referenceElement = "objectReference") =>
        All(Parse(xml), referenceElement)
            .Select(e => new SapObjectRef(
                (Attr(e, "name") ?? string.Empty).ToUpperInvariant(), Attr(e, "type") ?? string.Empty,
                Attr(e, "uri") ?? string.Empty, (Attr(e, "packageName") ?? string.Empty).ToUpperInvariant(), Attr(e, "description")))
            .Where(r => r.Name.Length > 0)
            .ToArray();

    /// <summary>Linha de uma URI com fragmento <c>#start=linha,coluna</c>.</summary>
    public static int? LineOf(string? uri)
    {
        if (uri is null)
            return null;
        var i = uri.IndexOf("#start=", StringComparison.Ordinal);
        if (i < 0)
            return null;
        var part = uri[(i + 7)..].Split(',', ';')[0];
        return int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var line) ? line : null;
    }

    /// <summary>Caminho da URI sem <c>/source/main</c>, sem fragmento e sem query.</summary>
    public static string ObjectPath(string uri)
    {
        var path = uri.Split('#', '?')[0].TrimEnd('/');
        return path.EndsWith("/source/main", StringComparison.OrdinalIgnoreCase) ? path[..^"/source/main".Length] : path;
    }

    /// <summary>
    /// Fontes do objectstructure (porte de <c>Get-AdtObjectSources</c>): todo <c>href</c> que termina em
    /// <c>/source/main</c>; o nome é o antepenúltimo segmento.
    /// </summary>
    public static IReadOnlyList<SapSourcePart> StructureSources(string xml)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var parts = new List<SapSourcePart>();
        foreach (Match m in SourceHrefRegex().Matches(xml))
        {
            var u = m.Groups[1].Value;
            if (!seen.Add(u))
                continue;
            var segments = u.Split('/');
            parts.Add(new SapSourcePart(Uri.UnescapeDataString(segments[^3].ToUpperInvariant()), "main", u));
        }
        return parts;
    }

    /// <summary>Verificação da versão ativa (porte de <c>Invoke-AdtSyntaxCheck</c>).</summary>
    public static string CheckActiveBody(string objectUri) =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><chkrun:checkObjectList xmlns:chkrun=\"http://www.sap.com/adt/checkrun\" xmlns:adtcore=\"http://www.sap.com/adt/core\">" +
        $"<chkrun:checkObject adtcore:uri=\"{Escape(objectUri)}\" chkrun:version=\"active\"/></chkrun:checkObjectList>";

    /// <summary>Verificação do fonte informado (base64) sem gravar; include vai com o contexto do principal.</summary>
    public static string CheckSourceBody(string objectUri, string source, string? contextUri)
    {
        var path = ObjectPath(objectUri);
        var context = contextUri is null ? string.Empty : $"?context={Uri.EscapeDataString(contextUri)}";
        return $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <chkrun:checkObjectList xmlns:chkrun="http://www.sap.com/adt/checkrun" xmlns:adtcore="{AdtCoreNs}">
          <chkrun:checkObject adtcore:uri="{Escape(path + context)}" chkrun:version="inactive">
            <chkrun:artifacts>
              <chkrun:artifact chkrun:contentType="text/plain; charset=utf-8" chkrun:uri="{Escape(path + "/source/main" + context)}">
                <chkrun:content>{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(source))}</chkrun:content>
              </chkrun:artifact>
            </chkrun:artifacts>
          </chkrun:checkObject>
        </chkrun:checkObjectList>
        """;
    }

    /// <summary>Mensagens do checkruns: severidade, texto, linha e URI relativa ao <c>/sap/bc/adt/</c>.</summary>
    public static IReadOnlyList<SyntaxMessage> CheckMessages(string xml) =>
        string.IsNullOrWhiteSpace(xml)
            ? []
            : All(Parse(xml), "checkMessage")
                .Select(m =>
                {
                    var uri = Attr(m, "uri") ?? string.Empty;
                    return new SyntaxMessage(Attr(m, "type") ?? string.Empty, Attr(m, "shortText") ?? string.Empty, LineOf(uri),
                        uri.Replace("/sap/bc/adt/", string.Empty, StringComparison.Ordinal));
                })
                .ToArray();

    /// <summary>Ativação de um objeto (porte de <c>Invoke-AdtActivate</c>): include com <c>?context=</c>.</summary>
    public static string ActivationBody(string objectUri, string name, string? contextUri)
    {
        var uri = string.IsNullOrEmpty(contextUri) ? objectUri : $"{objectUri}?context={Uri.EscapeDataString(contextUri)}";
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><adtcore:objectReferences xmlns:adtcore=\"http://www.sap.com/adt/core\">" +
               $"<adtcore:objectReference adtcore:uri=\"{WebUtility.HtmlEncode(uri)}\" adtcore:name=\"{name.ToUpperInvariant()}\"/></adtcore:objectReferences>";
    }

    /// <summary>Mensagens <c>&lt;msg type="E|W|A|X"&gt;…&lt;txt&gt;</c> da ativação (mesma regex da skill).</summary>
    public static IReadOnlyList<SyntaxMessage> ActivationMessages(string body) =>
        [.. ActivationMsgRegex().Matches(body).Select(m => new SyntaxMessage(m.Groups[1].Value, WebUtility.HtmlDecode(m.Groups[2].Value), null, null))];

    public static string? LockHandle(string body) => FirstGroup(LockHandleRegex(), body);

    public static string? LockCorrNr(string body) => FirstGroup(CorrNrRegex(), body);

    /// <summary>Texto de erro de uma resposta ADT (message / SHORT_TEXT / localizedMessage / corpo sem tags), até 400 caracteres.</summary>
    public static string Message(int status, string body)
    {
        var t = FirstGroup(MessageRegex(), body);
        if (string.IsNullOrEmpty(t))
            t = FirstGroup(ShortTextRegex(), body);
        if (string.IsNullOrEmpty(t))
            t = TagsRegex().Replace(body, " ").Trim();
        var s = $"HTTP {status}: {WebUtility.HtmlDecode(SpacesRegex().Replace(t, " "))}";
        return s.Length > 400 ? s[..400] : s;
    }

    /// <summary>Só o texto do primeiro <c>&lt;message&gt;</c> (como a skill grava em <c>erro</c>).</summary>
    public static string? FirstMessage(string body) => FirstGroup(MessageRegex(), body);

    private static string? FirstGroup(Regex regex, string body)
    {
        var m = regex.Match(body);
        return m.Success ? m.Groups[1].Value : null;
    }

    private static string Escape(string value) => SecurityElement.Escape(value) ?? string.Empty;

    [GeneratedRegex("href=\"(/sap/bc/adt/[^\"#?]+/source/main)")]
    private static partial Regex SourceHrefRegex();

    [GeneratedRegex(@"<msg [^>]*type=""([EWAX])""[^>]*>[\s\S]*?<txt>([^<]*)</txt>")]
    private static partial Regex ActivationMsgRegex();

    [GeneratedRegex("<LOCK_HANDLE>([^<]+)</LOCK_HANDLE>")]
    private static partial Regex LockHandleRegex();

    [GeneratedRegex("<CORRNR>([^<]*)</CORRNR>")]
    private static partial Regex CorrNrRegex();

    [GeneratedRegex("<message[^>]*>([^<]+)</message>")]
    private static partial Regex MessageRegex();

    [GeneratedRegex("<(?:SHORT_TEXT|localizedMessage)[^>]*>([^<]+)<")]
    private static partial Regex ShortTextRegex();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex TagsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();
}
