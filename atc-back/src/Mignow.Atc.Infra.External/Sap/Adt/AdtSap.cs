using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Application.Services;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// As portas SAP sobre a ADT via HTTP (adapter do migfix, validado no MI9, + porte dos <c>.psm1</c> das skills).
/// <c>partial</c>: cada módulo implementa as próprias portas em <c>AdtSap.&lt;Módulo&gt;.cs</c>. Regras: leitura e SQL
/// stateless; escrita só depois de <see cref="GuardWriteAsync"/> (produção/T000=P/ilegível bloqueiam); LOCK → PUT →
/// UNLOCK numa sessão stateful exclusiva com o UNLOCK em finally; ativação stateless; novas tentativas automáticas só em
/// GET; falha da ADT → Upstream (502).
/// </summary>
public sealed partial class AdtSap(
    AdtSessionFactory sessions, AdtThrottle throttle, IOptions<AdtOptions> options, IStringLocalizer<Errors> l, ILogger<AdtSap> logger)
    : ISapReader, ISapSql, ISapWriter
{
    private const string Root = AdtSession.AdtRoot;
    private const string Xml = "application/xml, */*;q=0.8";
    private const string DataPreviewAccept = "application/xml, application/vnd.sap.adt.datapreview.table.v1+xml";

    private readonly ConcurrentDictionary<Guid, (DateTimeOffset At, string? Category)> _categories = new();

    // ---------------------------------------------------------------- ISapReader

    public Task<Result<SapSystemInfo>> TestConnectionAsync(SapConnection connection, CancellationToken ct) =>
        Call(connection, "discovery", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Get, $"{Root}/discovery", ct, accept: "application/atomsvc+xml",
                headers: new Dictionary<string, string> { [CsrfDelegatingHandler.Header] = "fetch" });
            var xml = await Ok(response, "discovery", ct);
            var collections = AdtXml.All(AdtXml.Parse(xml), "collection")
                .Select(c => AdtXml.Attr(c, "href") ?? string.Empty)
                .Select(h => h.StartsWith(Root + "/", StringComparison.OrdinalIgnoreCase) ? h[(Root.Length + 1)..] : h.TrimStart('/'))
                .Where(h => h.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var release = await ReleaseAsync(s, ct);
            var category = await ClientCategoryAsync(s, connection, ct);
            var thumbprint = connection.BaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? CertificatePinning.LastSeen(s.Host) : null;
            return Result.Success(new SapSystemInfo(release, collections, category, thumbprint));
        });

    public Task<Result<IReadOnlyList<SapObjectRef>>> SearchAsync(SapConnection connection, string query, string? adtType, int maxResults, CancellationToken ct) =>
        Call(connection, "search", ct, async s => Result.Success(await Search(s, query, adtType, maxResults, ct)));

    public Task<Result<IReadOnlyList<SapSourcePart>>> GetObjectSourcesAsync(SapConnection connection, string name, string type, CancellationToken ct) =>
        Call(connection, "objectstructure", ct, async s =>
        {
            // Porte de Get-AdtObjectSources (SapObjeto.psm1).
            var n = Uri.EscapeDataString(name.ToLowerInvariant());
            var sources = new List<SapSourcePart>();
            switch (type.Trim().ToUpperInvariant())
            {
                case "CLAS":
                    sources.Add(new SapSourcePart(name, "main", $"{Root}/oo/classes/{n}/source/main"));
                    foreach (var p in new[] { "definitions", "implementations", "macros" })
                        sources.Add(new SapSourcePart(name, p, $"{Root}/oo/classes/{n}/includes/{p}"));
                    return Result.Success<IReadOnlyList<SapSourcePart>>(sources);
                case "INTF":
                    sources.Add(new SapSourcePart(name, "main", $"{Root}/oo/interfaces/{n}/source/main"));
                    return Result.Success<IReadOnlyList<SapSourcePart>>(sources);
            }
            var root = type.Trim().ToUpperInvariant() switch
            {
                "PROG" => $"{Root}/programs/programs/{n}",
                "FUGR" => $"{Root}/functions/groups/{n}",
                _ => null,
            };
            if (root is null)
            {
                var found = (await Search(s, name, null, 10, ct)).FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
                if (found is null)
                    return Result.Success<IReadOnlyList<SapSourcePart>>(sources);
                root = found.Uri;
            }
            using var o = await s.SendAsync(HttpMethod.Get, $"{root}/objectstructure", ct, accept: "application/vnd.sap.adt.objectstructure.v2+xml");
            if (o.StatusCode == HttpStatusCode.OK)
                sources.AddRange(AdtXml.StructureSources(await o.Content.ReadAsStringAsync(ct)));
            if (sources.All(x => x.Uri != $"{root}/source/main"))
                sources.Insert(0, new SapSourcePart(name, "main", $"{root}/source/main"));
            return Result.Success<IReadOnlyList<SapSourcePart>>(sources);
        });

    public Task<Result<string>> GetSourceAsync(SapConnection connection, string sourceUri, bool activeVersion, CancellationToken ct) =>
        Call(connection, "source", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Get, activeVersion ? $"{sourceUri}?version=active" : sourceUri, ct, accept: "text/plain");
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result.Failure<string>(SapErrors.ObjectNotFound(l, AdtXml.ObjectPath(sourceUri).Split('/').Last().ToUpperInvariant()));
            return Result.Success(await Ok(response, "source", ct));
        });

    public Task<Result<IReadOnlyList<SapObjectRef>>> GetStructureAsync(SapConnection connection, string objectUri, CancellationToken ct) =>
        Call(connection, "objectstructure", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Get, $"{AdtXml.ObjectPath(objectUri)}/objectstructure", ct,
                accept: "application/vnd.sap.adt.objectstructure.v2+xml, application/xml;q=0.8");
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result.Failure<IReadOnlyList<SapObjectRef>>(SapErrors.ObjectNotFound(l, objectUri));
            var xml = await Ok(response, "objectstructure", ct);
            IReadOnlyList<SapObjectRef> refs = AdtXml.All(AdtXml.Parse(xml), "objectStructureElement")
                .Select(e =>
                {
                    var href = AdtXml.All(e, "link").Select(k => AdtXml.Attr(k, "href")).FirstOrDefault(h => h?.Contains("/source/main", StringComparison.Ordinal) == true);
                    return new SapObjectRef((AdtXml.Attr(e, "name") ?? string.Empty).ToUpperInvariant(), AdtXml.Attr(e, "type") ?? string.Empty,
                        href is null ? string.Empty : AdtXml.ObjectPath(href), string.Empty);
                })
                .Where(r => r.Name.Length > 0)
                .DistinctBy(r => (r.Name, r.Type))
                .ToArray();
            return Result.Success(refs);
        });

    public Task<Result<IReadOnlyList<SyntaxMessage>>> CheckSyntaxAsync(SapConnection connection, string objectUri, CancellationToken ct) =>
        Call(connection, "checkruns", ct, async s =>
        {
            var path = AdtXml.ObjectPath(objectUri);
            using var response = await s.SendAsync(HttpMethod.Post, $"{Root}/checkruns?reporters=abapCheckRun", ct,
                content: XmlContent(AdtXml.CheckActiveBody(path), "application/vnd.sap.adt.checkobjects+xml"),
                accept: "application/vnd.sap.adt.checkmessages+xml");
            return Result.Success(AdtXml.CheckMessages(await Ok(response, "checkruns", ct)));
        });

    public Task<Result<IReadOnlyList<SyntaxMessage>>> CheckSourceAsync(SapConnection connection, string objectUri, string source, string? contextUri,
        CancellationToken ct) =>
        Call(connection, "checkruns", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Post, $"{Root}/checkruns?reporters=abapCheckRun", ct,
                content: XmlContent(AdtXml.CheckSourceBody(objectUri, source, contextUri), "application/vnd.sap.adt.checkobjects+xml"),
                accept: "application/vnd.sap.adt.checkmessages+xml");
            return Result.Success(AdtXml.CheckMessages(await Ok(response, "checkruns", ct)));
        });

    // ---------------------------------------------------------------- ISapSql

    public Task<Result<SqlResult>> SelectAsync(SapConnection connection, string sql, int maxRows, CancellationToken ct)
    {
        if (!AdtSql.IsSelect(sql))
            return Task.FromResult(Result.Failure<SqlResult>(SapErrors.OnlySelect(l)));
        return Call(connection, "datapreview", ct, s => SelectCoreAsync(s, sql, maxRows, ct));
    }

    public async Task<Result<SqlResult>> SelectInAsync(SapConnection connection, string sql, IReadOnlyCollection<string> values, int blockSize,
        CancellationToken ct)
    {
        List<string> columns = [];
        var rows = new List<IReadOnlyDictionary<string, string>>();
        foreach (var list in AdtSql.InLists(values, Math.Max(1, blockSize)))
        {
            var part = await SelectAsync(connection, sql.Replace("{IN}", list, StringComparison.Ordinal), 100_000, ct);
            if (part.IsFailure)
                return part;
            if (columns.Count == 0)
                columns = [.. part.Value.Columns];
            rows.AddRange(part.Value.Rows);
        }
        return new SqlResult(columns, rows);
    }

    /// <summary>POST stateless no data preview (porte de <c>Invoke-AdtSqlRaw</c>); sem throttle (quem chama já está dentro).</summary>
    private async Task<Result<SqlResult>> SelectCoreAsync(AdtSession s, string sql, int maxRows, CancellationToken ct)
    {
        var text = AdtSql.Format(sql);
        using var content = new StringContent(text, Encoding.UTF8);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "text/plain");
        using var response = await s.SendAsync(HttpMethod.Post, $"{Root}/datapreview/freestyle?rowNumber={maxRows.ToString(CultureInfo.InvariantCulture)}", ct,
            content: content, stateful: false, accept: DataPreviewAccept);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var start = text.Length > 300 ? text[..300] : text;
            var msg = AdtXml.Message((int)response.StatusCode, body);
            logger.LogWarning("ADT: SQL devolveu {Status}: {Message} | SQL: {Sql}", (int)response.StatusCode, msg, start.Replace('\n', ' '));
            return Result.Failure<SqlResult>(SapErrors.SqlFailed(l, msg));
        }
        return AdtSql.Parse(body);
    }

    // ---------------------------------------------------------------- ISapWriter

    public Task<Result<WriteGuardInfo>> EnsureWriteAllowedAsync(SapConnection connection, CancellationToken ct) =>
        Call(connection, "t000", ct, s => GuardWriteCoreAsync(s, connection, ct));

    /// <summary>
    /// Trava de escrita para os métodos de escrita dos módulos (partials): chame no início, fora de outro <c>Call</c>.
    /// </summary>
    internal Task<Result<WriteGuardInfo>> GuardWriteAsync(SapConnection connection, CancellationToken ct) => EnsureWriteAllowedAsync(connection, ct);

    private async Task<Result<WriteGuardInfo>> GuardWriteCoreAsync(AdtSession s, SapConnection connection, CancellationToken ct)
    {
        if (connection.IsProduction)
            return SapWriteGuard.Evaluate(connection, null, l);
        string? category;
        if (_categories.TryGetValue(connection.SystemId, out var cached)
            && DateTimeOffset.UtcNow - cached.At < TimeSpan.FromSeconds(options.Value.ClientCategoryCacheSeconds) && cached.Category is not null)
            category = cached.Category;
        else
        {
            category = await ClientCategoryAsync(s, connection, ct);
            _categories[connection.SystemId] = (DateTimeOffset.UtcNow, category);
        }
        var result = SapWriteGuard.Evaluate(connection, category, l);
        if (result.IsFailure)
            logger.LogWarning("ADT: escrita bloqueada em {System}/{Client}: {Code}", connection.SystemId, connection.Client, result.Error.Code);
        return result;
    }

    public async Task<Result<SourceWriteOutcome>> WriteSourceAsync(SapConnection connection, SourceWriteRequest request, CancellationToken ct)
    {
        var guard = await EnsureWriteAllowedAsync(connection, ct);
        if (guard.IsFailure)
            return guard.Error;
        return await Call(connection, "write", ct, read => WriteSourceCoreAsync(connection, read, request, ct));
    }

    /// <summary>Porte do trecho de gravação do <c>aplicar_adt.ps1</c>: lock → PUT → unlock → ativação.</summary>
    private async Task<Result<SourceWriteOutcome>> WriteSourceCoreAsync(SapConnection connection, AdtSession read, SourceWriteRequest r, CancellationToken ct)
    {
        var s = sessions.ForWrite(connection);
        var objectUri = AdtXml.ObjectPath(r.ObjectUri);
        string? unlock = null;
        try
        {
            // Entre o LOCK e o UNLOCK nada é cancelado (CancellationToken.None): nunca deixar o objeto preso no SM12.
            string lockBody;
            using (var lockResponse = await s.SendAsync(HttpMethod.Post, $"{objectUri}?_action=LOCK&accessMode=MODIFY", ct, stateful: true,
                       accept: "application/*,application/vnd.sap.as+xml;charset=UTF-8;dataname=com.sap.adt.lock.result"))
                lockBody = await lockResponse.Content.ReadAsStringAsync(CancellationToken.None);
            var handle = AdtXml.LockHandle(lockBody);
            if (string.IsNullOrEmpty(handle))
                return Outcome(SourceWriteOutcome.LockError, AdtXml.FirstMessage(lockBody) ?? string.Empty, null, null, null, []);
            var corrNr = AdtXml.LockCorrNr(lockBody);
            if (!string.IsNullOrEmpty(corrNr) && !string.Equals(corrNr, r.TransportRequest, StringComparison.OrdinalIgnoreCase))
            {
                await UnlockAsync(s, objectUri, handle, r.ObjectName);
                return Outcome(SourceWriteOutcome.RequestError, $"objeto já está na request {corrNr} (sessão usa {r.TransportRequest})", null, null, null, []);
            }
            int putStatus;
            string putBody;
            try
            {
                using var content = new StringContent(r.Source, Encoding.UTF8);
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", "text/plain; charset=utf-8");
                var query = $"lockHandle={Uri.EscapeDataString(handle)}";
                if (!string.IsNullOrEmpty(r.TransportRequest))
                    query += $"&corrNr={Uri.EscapeDataString(r.TransportRequest)}";
                // Validado no Moovi: sem Accept, o PUT do fonte de classe volta 400 "Accept header missing".
                using var put = await s.SendAsync(HttpMethod.Put, $"{r.SourceUri}?{query}", CancellationToken.None, content: content, stateful: true,
                    accept: "text/plain, application/xml, */*");
                putStatus = (int)put.StatusCode;
                putBody = await put.Content.ReadAsStringAsync(CancellationToken.None);
            }
            finally
            {
                unlock = await UnlockAsync(s, objectUri, handle, r.ObjectName);
            }
            if (putStatus >= 300)
                return Outcome(SourceWriteOutcome.WriteError, AdtXml.FirstMessage(putBody) ?? string.Empty,
                    "nada foi gravado; a versão ativa continua a anterior", unlock, null, []);

            var activation = await ActivateCoreAsync(read, objectUri, r.ObjectName, r.ActivationContextUri, ct);
            var messages = activation.Messages.Select(m => $"{m.Severity}: {m.Text}").ToList();
            if (activation.Error)
                return Outcome(r.Rollback ? SourceWriteOutcome.RollbackActivationError : SourceWriteOutcome.ActivationError, null,
                    "fonte gravado como versão INATIVA; a versão ativa continua a anterior", unlock, r.ActivationContextUri, messages);
            logger.LogInformation("ADT: {Object} gravado e ativado na request {Request} ({System})", r.ObjectName, r.TransportRequest, connection.SystemId);
            return Outcome(r.Rollback ? SourceWriteOutcome.RollbackActivated : SourceWriteOutcome.Activated, null, null, unlock, r.ActivationContextUri, messages);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException)
        {
            logger.LogWarning(ex, "ADT: gravação de {Object} falhou", r.ObjectName);
            return Outcome(SourceWriteOutcome.GenericError, ex.GetBaseException().Message, null, unlock, null, []);
        }
        finally
        {
            await LogoffAsync(s);
        }
    }

    public async Task<Result<ActivationResult>> ActivateAsync(SapConnection connection, string objectUri, string objectName, string? contextUri,
        CancellationToken ct)
    {
        var guard = await EnsureWriteAllowedAsync(connection, ct);
        if (guard.IsFailure)
            return guard.Error;
        return await Call(connection, "activation", ct, async s => Result.Success(await ActivateCoreAsync(s, AdtXml.ObjectPath(objectUri), objectName, contextUri, ct)));
    }

    /// <summary>
    /// Ativação stateless (porte de <c>Invoke-AdtActivate</c>): ativação que falha dentro da sessão stateful deixa o enqueue
    /// preso (SM12). HTTP de erro sem <c>&lt;msg&gt;</c> vira uma mensagem E com o texto.
    /// </summary>
    private static async Task<ActivationResult> ActivateCoreAsync(AdtSession s, string objectUri, string name, string? contextUri, CancellationToken ct)
    {
        using var response = await s.SendAsync(HttpMethod.Post, $"{Root}/activation?method=activate&preauditRequested=true", ct,
            content: XmlContent(AdtXml.ActivationBody(objectUri, name, contextUri), "application/xml"), stateful: false, accept: "application/xml");
        var status = (int)response.StatusCode;
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        var messages = AdtXml.ActivationMessages(body).ToList();
        if (status >= 300 && messages.Count == 0)
            messages.Add(new SyntaxMessage("E", AdtXml.Message(status, body), null, null));
        var error = status >= 300 || messages.Any(m => m.Severity is "E" or "A" or "X");
        return new ActivationResult(status, error, messages);
    }

    // ---------------------------------------------------------------- auxiliares

    private async Task<string> UnlockAsync(AdtSession s, string objectUri, string handle, string name)
    {
        // Sem o token da chamada: o UNLOCK sai mesmo se a execução foi cancelada no meio.
        try
        {
            using var unlock = await s.SendAsync(HttpMethod.Post, $"{objectUri}?_action=UNLOCK&lockHandle={Uri.EscapeDataString(handle)}",
                CancellationToken.None, stateful: true);
            if (!unlock.IsSuccessStatusCode)
                logger.LogWarning("ADT: UNLOCK de {Object} devolveu {Status}", name, (int)unlock.StatusCode);
            return ((int)unlock.StatusCode).ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "ADT: UNLOCK de {Object} falhou", name);
            return $"erro: {ex.GetBaseException().Message}";
        }
    }

    /// <summary>Encerra a sessão stateful no servidor (libera enqueues que tenham ficado nela); erros são ignorados.</summary>
    private static async Task LogoffAsync(AdtSession s)
    {
        try
        {
            using var _ = await s.SendAsync(HttpMethod.Get, "/sap/public/bc/icf/logoff", CancellationToken.None);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Melhor esforço, como o Close-AdtSession.
        }
    }

    private static SourceWriteOutcome Outcome(string result, string? error, string? observation, string? unlock, string? context, IReadOnlyList<string> messages) =>
        new(result, error, observation, unlock, context, messages);

    /// <summary>T000-CCCATEGORY do mandante pelo data preview; nula se a consulta falhar (= ilegível).</summary>
    private async Task<string?> ClientCategoryAsync(AdtSession s, SapConnection connection, CancellationToken ct)
    {
        try
        {
            var result = await SelectCoreAsync(s, SapWriteGuard.T000Query(connection.Client), 1, ct);
            if (result.IsFailure || result.Value.Rows.Count == 0)
                return null;
            var value = result.Value.Rows[0].Values.FirstOrDefault()?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }
        catch (Exception ex) when (ex is HttpRequestException or XmlException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "ADT: T000 de {System} ilegível", connection.SystemId);
            return null;
        }
    }

    private async Task<IReadOnlyList<SapObjectRef>> Search(AdtSession s, string pattern, string? type, int maxResults, CancellationToken ct)
    {
        var query = $"{Root}/repository/informationsystem/search?operation=quickSearch&query={Uri.EscapeDataString(pattern.Trim().ToLowerInvariant())}&maxResults={Math.Max(1, maxResults)}";
        if (!string.IsNullOrEmpty(type))
            query += $"&objectType={Uri.EscapeDataString(type)}";
        using var response = await s.SendAsync(HttpMethod.Get, query, ct, accept: "application/xml");
        return AdtXml.ObjectReferences(await Ok(response, "search", ct));
    }

    private static async Task<string> ReleaseAsync(AdtSession s, CancellationToken ct)
    {
        // Componentes do sistema (S4CORE). Sem eles, o release fica "unknown".
        using var response = await s.SendAsync(HttpMethod.Get, $"{Root}/system/components", ct, accept: "application/atom+xml;type=feed");
        if (!response.IsSuccessStatusCode)
            return "unknown";
        try
        {
            var doc = AdtXml.Parse(await response.Content.ReadAsStringAsync(ct));
            var core = AdtXml.All(doc, "entry").FirstOrDefault(e => AdtXml.ChildText(e, "id") == "S4CORE");
            if (core is null)
                return "unknown";
            // Título real (MI9): "108;SAPK-10801INS4CORE;0001;S/4HANA Core Applications 1" = release;pacote;SP;descrição.
            var parts = (AdtXml.ChildText(core, "title") ?? string.Empty).Split(';');
            return parts.Length >= 3 ? $"S4CORE {parts[0]} SP{parts[2]}" : $"S4CORE {parts[0]}".Trim();
        }
        catch (XmlException)
        {
            return "unknown";
        }
    }

    internal static StringContent XmlContent(string xml, string mediaType)
    {
        var content = new StringContent(xml, Encoding.UTF8);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", mediaType);
        return content;
    }

    internal static async Task<string> Ok(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new AdtHttpException(operation, response.StatusCode, AdtXml.Message((int)response.StatusCode, body));
        return body;
    }

    /// <summary>Chamada de leitura numa sessão stateless, com throttle por ambiente e erros convertidos em Upstream.</summary>
    internal Task<Result<T>> Call<T>(SapConnection connection, string operation, CancellationToken ct, Func<AdtSession, Task<Result<T>>> body) =>
        Guarded(connection, operation, ct, async () => await body(sessions.ForRead(connection)), Result.Failure<T>);

    internal Task<Result> Call(SapConnection connection, string operation, CancellationToken ct, Func<AdtSession, Task<Result>> body) =>
        Guarded(connection, operation, ct, async () => await body(sessions.ForRead(connection)), Result.Failure);

    private async Task<TResult> Guarded<TResult>(
        SapConnection connection, string operation, CancellationToken ct, Func<Task<TResult>> body, Func<Error, TResult> fail)
    {
        using var _ = await throttle.EnterAsync(connection.SystemId, ct);
        try
        {
            return await body();
        }
        catch (AdtHttpException ex)
        {
            logger.LogWarning("ADT: {Operation} em {System} devolveu HTTP {Status}: {SapMessage}", operation, connection.SystemId, (int)ex.Status, ex.SapMessage);
            return fail(SapErrors.Upstream(l, ex.SapMessage is null ? $"{operation} (HTTP {(int)ex.Status})" : $"{operation} ({ex.SapMessage})"));
        }
        catch (HttpRequestException ex) when (ex.InnerException is System.Security.Authentication.AuthenticationException)
        {
            logger.LogWarning("ADT: certificado de {System} recusado em {Operation}", connection.SystemId, operation);
            return fail(SapErrors.CertificateRejected(l));
        }
        catch (Exception ex) when (ex is HttpRequestException or XmlException or AdtProtocolException
                                       || ex is TaskCanceledException or Polly.Timeout.TimeoutRejectedException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "ADT: {Operation} em {System} falhou", operation, connection.SystemId);
            return fail(SapErrors.Upstream(l, $"{operation}: {ex.GetBaseException().Message}"));
        }
    }
}
