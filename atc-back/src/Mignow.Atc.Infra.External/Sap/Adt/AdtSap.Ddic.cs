using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Ddic;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Módulo Ddic: implementação ADT das portas de <c>Application/Ports/Sap/Ddic/</c> — porte de <c>ler_ddic.ps1</c> (leitura de
/// estrutura/tabela/função/elemento de dados) e do trecho SAP do <c>aplicar_ddic.ps1</c> (validação do nome, criação, lock, PUT,
/// unlock, ativação). Toda escrita começa pela trava (<c>GuardWriteAsync</c>); as regras (estrutura da tabela, backup) são do serviço.
/// O simulador correspondente fica em <c>Simulator/SimulatedSap.Ddic.cs</c>.
/// </summary>
public sealed partial class AdtSap : IDdicReader, IDdicWriter
{
    private static readonly DdicKind[] ReadOrder = [DdicKind.Structure, DdicKind.Table, DdicKind.Function, DdicKind.DataElement];

    private static (string Collection, string ObjType, string Mime) DdicInfo(DdicKind kind) => kind switch
    {
        DdicKind.Structure => ($"{Root}/ddic/structures", "TABL/DS", "application/vnd.sap.adt.structures.v2+xml"),
        DdicKind.Table => ($"{Root}/ddic/tables", "TABL/DT", "application/vnd.sap.adt.tables.v2+xml"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Só estrutura e tabela têm DDL gravável."),
    };

    private static string DdicObjectUri(DdicKind kind, string name) => $"{DdicInfo(kind).Collection}/{name.ToLowerInvariant().Replace("/", "%2f", StringComparison.Ordinal)}";

    // ---------------------------------------------------------------- IDdicReader

    public Task<Result<DdicDefinition>> ReadDefinitionAsync(SapConnection connection, string name, DdicKind? kind, CancellationToken ct) =>
        Call(connection, "ddic-read", ct, async s =>
        {
            var n = name.Trim().ToUpperInvariant();
            foreach (var k in kind is null ? ReadOrder : [kind.Value])
            {
                var body = await ReadDefinitionAsync(s, n, k, ct);
                if (body is not null)
                    return Result.Success(new DdicDefinition(n, k, body));
            }
            return Result.Success(new DdicDefinition(n, null, null));
        });

    private async Task<string?> ReadDefinitionAsync(AdtSession s, string n, DdicKind kind, CancellationToken ct)
    {
        var e = Uri.EscapeDataString(n.ToLowerInvariant());
        switch (kind)
        {
            case DdicKind.Structure:
                return await TextOrNull(s, $"{Root}/ddic/structures/{e}/source/main", "text/plain", ct);
            case DdicKind.Table:
                return await TextOrNull(s, $"{Root}/ddic/tables/{e}/source/main", "text/plain", ct);
            case DdicKind.Function:
            {
                using var search = await s.SendAsync(HttpMethod.Get,
                    $"{Root}/repository/informationsystem/search?operation=quickSearch&query={e}&maxResults=10&objectType=FUGR/FF", ct, accept: "application/xml");
                if (!search.IsSuccessStatusCode)
                    return null;
                var found = AdtXml.ObjectReferences(await search.Content.ReadAsStringAsync(ct)).FirstOrDefault(r => r.Name == n);
                return found is null ? null : await TextOrNull(s, $"{found.Uri}/source/main", "text/plain", ct);
            }
            default:
            {
                using var response = await s.SendAsync(HttpMethod.Get, $"{Root}/ddic/dataelements/{e}", ct, accept: "application/vnd.sap.adt.dataelements.v2+xml");
                if (!response.IsSuccessStatusCode)
                    return null;
                var xml = await response.Content.ReadAsStringAsync(ct);
                var values = DdicDataElementRegex().Matches(xml).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
                var description = DdicDescriptionRegex().Match(xml).Groups[1].Value;
                string V(string key) => values.GetValueOrDefault(key, string.Empty);
                static int I(string v) => int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : 0;
                return $"* {description}\ndata element {n.ToLowerInvariant()} : {V("typeName")} ({V("dataType")} {I(V("dataTypeLength"))} dec {I(V("dataTypeDecimals"))}) kind={V("typeKind")} label={V("mediumFieldLabel")}";
            }
        }
    }

    private static async Task<string?> TextOrNull(AdtSession s, string path, string accept, CancellationToken ct)
    {
        using var response = await s.SendAsync(HttpMethod.Get, path, ct, accept: accept);
        return response.StatusCode == HttpStatusCode.OK ? await response.Content.ReadAsStringAsync(ct) : null;
    }

    // ---------------------------------------------------------------- IDdicWriter

    public Task<Result<DdicObjectState>> ReadObjectAsync(SapConnection connection, DdicKind kind, string name, CancellationToken ct) =>
        Call(connection, "ddic-state", ct, async s =>
        {
            var (_, _, mime) = DdicInfo(kind);
            var objectUri = DdicObjectUri(kind, name);
            using var meta = await s.SendAsync(HttpMethod.Get, objectUri, ct, accept: mime);
            if (meta.StatusCode == HttpStatusCode.NotFound)
                return Result.Success(new DdicObjectState(false, null));
            await Ok(meta, "ddic-state", ct);
            using var source = await s.SendAsync(HttpMethod.Get, $"{objectUri}/source/main", ct, accept: "text/plain");
            return Result.Success(new DdicObjectState(true, await Ok(source, "ddic-source", ct)));
        });

    public async Task<Result<DdicNameValidation>> ValidateNameAsync(SapConnection connection, DdicKind kind, string name, string package, string description,
        CancellationToken ct)
    {
        var guard = await GuardWriteAsync(connection, ct);
        if (guard.IsFailure)
            return guard.Error;
        return await Call(connection, "ddic-validate", ct, async _ =>
        {
            var s = sessions.ForWrite(connection);
            try
            {
                var (collection, objType, _) = DdicInfo(kind);
                var query = $"objtype={Uri.EscapeDataString(objType)}&objname={name.ToUpperInvariant()}&packagename={package.ToUpperInvariant()}&description={Uri.EscapeDataString(description)}";
                using var response = await s.SendAsync(HttpMethod.Post, $"{collection}/validation?{query}", ct, stateful: true,
                    accept: "application/vnd.sap.as+xml, application/xml");
                var body = await response.Content.ReadAsStringAsync(ct);
                var check = DdicFirstGroup(DdicCheckResultRegex(), body);
                var severity = DdicFirstGroup(DdicSeverityRegex(), body) ?? string.Empty;
                var text = DdicFirstGroup(DdicShortTextRegex(), body);
                if (string.IsNullOrEmpty(text))
                    text = AdtXml.FirstMessage(body) ?? string.Empty;
                var ok = (int)response.StatusCode < 300 && (check == "X" || severity == "OK");
                return Result.Success(new DdicNameValidation(ok, ok ? "OK" : $"ERRO {severity}".Trim(), WebUtility.HtmlDecode(text)));
            }
            finally
            {
                await LogoffAsync(s);
            }
        });
    }

    public async Task<Result<DdicWriteOutcome>> WriteAsync(SapConnection connection, DdicWriteRequest request, CancellationToken ct)
    {
        var guard = await GuardWriteAsync(connection, ct);
        if (guard.IsFailure)
            return guard.Error;
        return await Call(connection, "ddic-write", ct, read => WriteDdicCoreAsync(connection, read, request, ct));
    }

    /// <summary>Porte do trecho de gravação do <c>aplicar_ddic.ps1</c>: [criação] → lock → PUT → unlock → ativação.</summary>
    private async Task<Result<DdicWriteOutcome>> WriteDdicCoreAsync(SapConnection connection, AdtSession read, DdicWriteRequest r, CancellationToken ct)
    {
        var s = sessions.ForWrite(connection);
        var (collection, objType, mime) = DdicInfo(r.Kind);
        var objectUri = DdicObjectUri(r.Kind, r.Name);
        var sourceUri = $"{objectUri}/source/main";
        var name = r.Name.ToUpperInvariant();
        try
        {
            if (r.Create)
            {
                var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><blue:blueSource xmlns:blue=\"http://www.sap.com/wbobj/blue\" xmlns:adtcore=\"http://www.sap.com/adt/core\"" +
                          $" adtcore:description=\"{SecurityElement.Escape(r.Description ?? string.Empty)}\" adtcore:name=\"{name}\" adtcore:type=\"{objType}\" adtcore:responsible=\"{(r.Responsible ?? string.Empty).ToUpperInvariant()}\">" +
                          $"<adtcore:packageRef adtcore:name=\"{(r.Package ?? string.Empty).ToUpperInvariant()}\"/></blue:blueSource>";
                var path = string.IsNullOrEmpty(r.TransportRequest) ? collection : $"{collection}?corrNr={r.TransportRequest}";
                using var create = await s.SendAsync(HttpMethod.Post, path, ct, content: XmlContent(xml, mime), stateful: true, accept: mime);
                var createBody = await create.Content.ReadAsStringAsync(ct);
                if ((int)create.StatusCode >= 300)
                    return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.CreationError, WebUtility.HtmlDecode(AdtXml.FirstMessage(createBody) ?? string.Empty), null, []));
            }
            string lockBody;
            using (var lockResponse = await s.SendAsync(HttpMethod.Post, $"{objectUri}?_action=LOCK&accessMode=MODIFY", ct, stateful: true,
                       accept: "application/*,application/vnd.sap.as+xml;charset=UTF-8;dataname=com.sap.adt.lock.result"))
                lockBody = await lockResponse.Content.ReadAsStringAsync(CancellationToken.None);
            var handle = AdtXml.LockHandle(lockBody);
            if (string.IsNullOrEmpty(handle))
                return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.LockError, AdtXml.FirstMessage(lockBody) ?? string.Empty, null, []));
            var corrNr = AdtXml.LockCorrNr(lockBody);
            if (!string.IsNullOrEmpty(corrNr) && !string.Equals(corrNr, r.TransportRequest, StringComparison.OrdinalIgnoreCase))
            {
                await UnlockAsync(s, objectUri, handle, name);
                return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.RequestError, $"objeto já está na request {corrNr} (sessão usa {r.TransportRequest})", null, []));
            }
            int putStatus;
            string putBody;
            try
            {
                using var content = new StringContent(r.Source, System.Text.Encoding.UTF8);
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", "text/plain; charset=utf-8");
                var query = $"lockHandle={Uri.EscapeDataString(handle)}";
                if (!string.IsNullOrEmpty(r.TransportRequest))
                    query += $"&corrNr={Uri.EscapeDataString(r.TransportRequest)}";
                using var put = await s.SendAsync(HttpMethod.Put, $"{sourceUri}?{query}", CancellationToken.None, content: content, stateful: true,
                    accept: "text/plain, application/xml, */*");
                putStatus = (int)put.StatusCode;
                putBody = await put.Content.ReadAsStringAsync(CancellationToken.None);
            }
            finally
            {
                await UnlockAsync(s, objectUri, handle, name);
            }
            if (putStatus >= 300)
                return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.WriteError, WebUtility.HtmlDecode(AdtXml.FirstMessage(putBody) ?? string.Empty),
                    r.Create ? "objeto criado vazio/inativo; corrija o DDL e rode de novo" : "nada foi gravado; a versão ativa continua a anterior", []));
            var activation = await ActivateCoreAsync(read, objectUri, name, null, ct);
            var messages = activation.Messages.Select(m => $"{m.Severity}: {m.Text}").ToList();
            if (activation.Error)
                return Result.Success(new DdicWriteOutcome(r.Rollback ? DdicWriteOutcome.RollbackActivationError : DdicWriteOutcome.ActivationError, null,
                    "DDL gravado como versão INATIVA; a versão ativa continua a anterior", messages));
            logger.LogInformation("ADT: DDIC {Object} gravado e ativado na request {Request} ({System})", name, r.TransportRequest, connection.SystemId);
            return Result.Success(new DdicWriteOutcome(r.Rollback ? DdicWriteOutcome.RollbackActivated : DdicWriteOutcome.Activated, null, null, messages));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException)
        {
            logger.LogWarning(ex, "ADT: gravação DDIC de {Object} falhou", name);
            return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.GenericError, ex.GetBaseException().Message, null, []));
        }
        finally
        {
            await LogoffAsync(s);
        }
    }

    private static string? DdicFirstGroup(Regex regex, string body)
    {
        var m = regex.Match(body);
        return m.Success ? m.Groups[1].Value : null;
    }

    [GeneratedRegex("<dtel:(typeKind|typeName|dataType|dataTypeLength|dataTypeDecimals|shortFieldLabel|mediumFieldLabel)>([^<]*)<")]
    private static partial Regex DdicDataElementRegex();

    [GeneratedRegex("adtcore:description=\"([^\"]*)\"")]
    private static partial Regex DdicDescriptionRegex();

    [GeneratedRegex("<CHECK_RESULT>([^<]*)</CHECK_RESULT>")]
    private static partial Regex DdicCheckResultRegex();

    [GeneratedRegex("<SEVERITY>([^<]*)</SEVERITY>")]
    private static partial Regex DdicSeverityRegex();

    [GeneratedRegex("<SHORT_TEXT>([^<]*)</SHORT_TEXT>")]
    private static partial Regex DdicShortTextRegex();
}
