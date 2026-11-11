using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Módulo Scan: implementação ADT de <see cref="IMignowScanner"/> (portes de <c>inventario.ps1</c> e <c>executar.ps1</c>). Leitura: TADIR
/// e T000 pelo data preview, validação do nome no ADT. Escrita (única da skill): classe temporária em <c>$TMP</c> sem request —
/// POST da classe, LOCK → PUT do fonte → UNLOCK, ativação stateless, classrun (stateless) e remoção (LOCK + DELETE; se falhar, a API
/// <c>/deletion</c>), sempre depois da trava de produção do núcleo. Nunca altera objeto do cliente.
/// </summary>
public sealed partial class AdtSap : IMignowScanner
{
    private const string ScanClassDescription = "MIGNOW ATC varredura temporaria";

    public Task<Result<IReadOnlyList<ScanTadirRow>>> InventoryAsync(SapConnection connection, IReadOnlyList<string> types, CancellationToken ct) =>
        Call(connection, "scan-inventory", ct, async s =>
        {
            var inTypes = string.Join(',', types.Select(t => $"'{t.ToUpperInvariant().Replace("'", "''", StringComparison.Ordinal)}'"));
            var sql = "SELECT object, obj_name, devclass, author, genflag, delflag FROM tadir WHERE pgmid = 'R3TR' AND object IN (" + inTypes + ") " +
                      "AND ( obj_name LIKE 'Z%' OR obj_name LIKE 'Y%' OR devclass LIKE 'Z%' OR devclass LIKE 'Y%' )";
            var result = await SelectCoreAsync(s, sql, 200000, ct);
            if (result.IsFailure)
                return Result.Failure<IReadOnlyList<ScanTadirRow>>(result.Error);
            IReadOnlyList<ScanTadirRow> rows =
            [
                .. result.Value.Rows.Select(r => new ScanTadirRow(Col(r, "OBJECT"), Col(r, "OBJ_NAME"), Col(r, "DEVCLASS"), Col(r, "AUTHOR"), Col(r, "GENFLAG"), Col(r, "DELFLAG"))),
            ];
            return Result.Success(rows);
        });

    public Task<Result<ScanSimulation>> SimulateAsync(SapConnection connection, IReadOnlyList<string> classNames, CancellationToken ct) =>
        Call(connection, "scan-simulate", ct, async s =>
        {
            var t000 = await SelectCoreAsync(s, $"SELECT mandt, cccategory, ccnocliind, cccoractiv FROM t000 WHERE mandt = '{connection.Client.Replace("'", "''", StringComparison.Ordinal)}'", 1, ct);
            if (t000.IsFailure)
                return Result.Failure<ScanSimulation>(t000.Error);
            var row = t000.Value.Rows.FirstOrDefault();
            var category = row is null ? "?" : Col(row, "CCCATEGORY");
            var noChange = row is null ? "?" : Col(row, "CCNOCLIIND");
            if (category.Length == 0)
                category = "?";
            var clientOk = !connection.IsProduction && category is not ("P" or "?") && noChange is not ("2" or "3");
            var classes = new List<ScanClassCheck>();
            foreach (var name in classNames.Select(n => n.ToUpperInvariant()))
            {
                var tadir = await ClassInTadirAsync(s, name, ct);
                if (tadir.IsFailure)
                    return Result.Failure<ScanSimulation>(tadir.Error);
                if (tadir.Value is { } package)
                {
                    classes.Add(new ScanClassCheck(name, true, package, false,
                        $"classe já existe (pacote {package}): rode a remoção de resíduos se for resíduo da varredura anterior"));
                    continue;
                }
                var q = $"objtype=CLAS%2FOC&objname={name}&packagename=%24TMP&description={Uri.EscapeDataString(ScanClassDescription)}";
                using var response = await s.SendAsync(HttpMethod.Post, $"{Root}/oo/validation/objectname?{q}", ct, accept: "application/vnd.sap.as+xml, application/xml");
                var body = await response.Content.ReadAsStringAsync(ct);
                var check = ScanCheckResultRegex().Match(body).Groups[1].Value;
                var ok = response.IsSuccessStatusCode && check == "X";
                classes.Add(new ScanClassCheck(name, false, string.Empty, ok, ok ? null : HttpText((int)response.StatusCode, body)));
            }
            return Result.Success(new ScanSimulation(category, noChange, clientOk, classes));
        });

    public async Task<Result<ScanProcessResult>> ExecuteProcessAsync(
        SapConnection connection, ScanProcessRequest request, Func<ScanLotResult, Task<bool>> onLot, CancellationToken ct)
    {
        var guard = await GuardWriteAsync(connection, ct);
        if (guard.IsFailure)
            return guard.Error;
        var cls = request.ClassName.ToUpperInvariant();
        var uri = $"{Root}/oo/classes/{cls.ToLowerInvariant()}";
        using var slot = await throttle.EnterAsync(connection.SystemId, ct);
        var s = sessions.ForWrite(connection);
        var read = sessions.ForRead(connection);
        var created = false;
        string? error = null;
        ScanClassRemoval removal;
        try
        {
            var body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><class:abapClass xmlns:class=\"http://www.sap.com/adt/oo/classes\" " +
                       $"xmlns:adtcore=\"http://www.sap.com/adt/core\" adtcore:description=\"{ScanClassDescription}\" adtcore:name=\"{cls}\" adtcore:type=\"CLAS/OC\" " +
                       $"adtcore:responsible=\"{SecurityElementEscape(connection.User.ToUpperInvariant())}\" class:final=\"true\" class:visibility=\"public\">" +
                       "<adtcore:packageRef adtcore:name=\"$TMP\"/><class:superClassRef/></class:abapClass>";
            const string classType = "application/vnd.sap.adt.oo.classes.v4+xml";
            using var create = await s.SendAsync(HttpMethod.Post, $"{Root}/oo/classes", ct, content: XmlContent(body, classType), stateful: true, accept: classType);
            var createBody = await create.Content.ReadAsStringAsync(CancellationToken.None);
            if (!create.IsSuccessStatusCode)
                error = "Criação da classe falhou: " + HttpText((int)create.StatusCode, createBody);
            else
            {
                created = true;
                foreach (var lot in request.Lots)
                {
                    if (ct.IsCancellationRequested)
                        break;
                    var result = await RunLotAsync(s, read, uri, cls, lot, ct);
                    if (!await onLot(result) || result.Status != ScanBatchStatus.Ok)
                        break;
                }
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException or Polly.Timeout.TimeoutRejectedException)
        {
            error = ex.GetBaseException().Message;
            logger.LogWarning(ex, "ADT: varredura {Class} falhou", cls);
        }
        finally
        {
            removal = await RemoveTemporaryClassAsync(s, cls, CancellationToken.None);
            await LogoffAsync(s);
        }
        return new ScanProcessResult(cls, created, error, removal);
    }

    public async Task<Result<IReadOnlyList<ScanClassRemoval>>> RemoveResiduesAsync(SapConnection connection, IReadOnlyList<string> classNames, CancellationToken ct)
    {
        var guard = await GuardWriteAsync(connection, ct);
        if (guard.IsFailure)
            return guard.Error;
        using var slot = await throttle.EnterAsync(connection.SystemId, ct);
        var s = sessions.ForWrite(connection);
        var list = new List<ScanClassRemoval>();
        try
        {
            foreach (var name in classNames.Select(n => n.ToUpperInvariant()))
                list.Add(await RemoveTemporaryClassAsync(s, name, ct));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException or Polly.Timeout.TimeoutRejectedException)
        {
            return Result.Failure<IReadOnlyList<ScanClassRemoval>>(SapErrors.Upstream(l, $"scan-cleanup: {ex.GetBaseException().Message}"));
        }
        finally
        {
            await LogoffAsync(s);
        }
        return list;
    }

    /// <summary>Um lote (porte do laço de <c>executar.ps1</c>): lock → PUT → unlock, ativação, classrun. Para no primeiro erro, com o status do script.</summary>
    private async Task<ScanLotResult> RunLotAsync(AdtSession s, AdtSession read, string uri, string cls, ScanLotRequest lot, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        ScanLotResult Fail(string status, string message, IReadOnlyList<string>? messages = null, string output = "") =>
            new(lot.Number, status, output, message, messages ?? [], (int)sw.Elapsed.TotalSeconds);
        try
        {
            string lockBody;
            using (var lockResponse = await s.SendAsync(HttpMethod.Post, $"{uri}?_action=LOCK&accessMode=MODIFY", ct, stateful: true,
                       accept: "application/*,application/vnd.sap.as+xml;charset=UTF-8;dataname=com.sap.adt.lock.result"))
                lockBody = await lockResponse.Content.ReadAsStringAsync(CancellationToken.None);
            var handle = AdtXml.LockHandle(lockBody);
            if (string.IsNullOrEmpty(handle))
                return Fail(ScanBatchStatus.LockError, AdtXml.FirstMessage(lockBody) ?? "LOCK sem handle");
            int putStatus;
            string putBody;
            try
            {
                using var content = new StringContent(lot.Source, Encoding.UTF8);
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", "text/plain; charset=utf-8");
                using var put = await s.SendAsync(HttpMethod.Put, $"{uri}/source/main?lockHandle={Uri.EscapeDataString(handle)}", CancellationToken.None,
                    content: content, stateful: true, accept: "text/plain, application/xml, */*");
                putStatus = (int)put.StatusCode;
                putBody = await put.Content.ReadAsStringAsync(CancellationToken.None);
            }
            finally
            {
                await UnlockAsync(s, uri, handle, cls);
            }
            if (putStatus >= 300)
                return Fail(ScanBatchStatus.WriteError, HttpText(putStatus, putBody));
            var activation = await ActivateCoreAsync(read, uri, cls, null, ct);
            if (activation.Error)
                return Fail(ScanBatchStatus.ActivationError, "ativação da classe gerada falhou",
                    [.. activation.Messages.Take(15).Select(m => $"{m.Severity}: {m.Text}")]);
            using var run = await s.SendAsync(HttpMethod.Post, $"{Root}/oo/classrun/{cls}", ct, stateful: false, accept: "text/plain", longTimeout: true);
            var output = await run.Content.ReadAsStringAsync(CancellationToken.None);
            if (!run.IsSuccessStatusCode)
                return Fail(ScanBatchStatus.ExecutionError, HttpText((int)run.StatusCode, output), output: output);
            return new ScanLotResult(lot.Number, ScanBatchStatus.Ok, output, null, [], (int)sw.Elapsed.TotalSeconds);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException or Polly.Timeout.TimeoutRejectedException)
        {
            return Fail(ScanBatchStatus.ExecutionError, ex.GetBaseException().Message);
        }
    }

    /// <summary>Porte de <c>Remove-Classe</c>: só apaga classe do pacote <c>$TMP</c>; lock + DELETE, depois a API de deleção; confere na TADIR.</summary>
    private async Task<ScanClassRemoval> RemoveTemporaryClassAsync(AdtSession s, string cls, CancellationToken ct)
    {
        var attempts = new List<string>();
        try
        {
            var tadir = await ClassInTadirAsync(s, cls, ct);
            if (tadir.IsFailure)
                return new ScanClassRemoval(cls, false, "NÃO REMOVIDA: " + tadir.Error.Message, attempts);
            if (tadir.Value is null)
                return new ScanClassRemoval(cls, true, "nao_existia", attempts);
            if (tadir.Value != "$TMP")
                return new ScanClassRemoval(cls, false, $"NÃO REMOVIDA: classe existe no pacote {tadir.Value}, não é a temporária", attempts);
            var uri = $"{Root}/oo/classes/{cls.ToLowerInvariant()}";
            string lockBody;
            using (var lockResponse = await s.SendAsync(HttpMethod.Post, $"{uri}?_action=LOCK&accessMode=MODIFY", ct, stateful: true,
                       accept: "application/*,application/vnd.sap.as+xml;charset=UTF-8;dataname=com.sap.adt.lock.result"))
                lockBody = await lockResponse.Content.ReadAsStringAsync(CancellationToken.None);
            var handle = AdtXml.LockHandle(lockBody);
            if (!string.IsNullOrEmpty(handle))
            {
                using var delete = await s.SendAsync(HttpMethod.Delete, $"{uri}?lockHandle={Uri.EscapeDataString(handle)}", ct, stateful: true);
                attempts.Add($"DELETE: HTTP {(int)delete.StatusCode}");
                if (!delete.IsSuccessStatusCode)
                    await UnlockAsync(s, uri, handle, cls);
            }
            else
                attempts.Add("LOCK: sem handle");
            if ((await ClassInTadirAsync(s, cls, ct)).Value is not null)
            {
                var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><del:deletionRequest xmlns:del=\"http://www.sap.com/adt/deletion\" " +
                          $"xmlns:adtcore=\"http://www.sap.com/adt/core\"><del:object adtcore:uri=\"{uri}\"><del:transportNumber/></del:object></del:deletionRequest>";
                using var d = await s.SendAsync(HttpMethod.Post, $"{Root}/deletion/delete", ct, content: XmlContent(xml, "application/vnd.sap.adt.deletion.request.v1+xml"),
                    accept: "application/*");
                attempts.Add($"deletion/delete: HTTP {(int)d.StatusCode}");
            }
            var gone = (await ClassInTadirAsync(s, cls, ct)).Value is null;
            return new ScanClassRemoval(cls, gone, gone ? "removida" : $"NÃO REMOVIDA: apague a classe {cls} (SE24/Eclipse, pacote $TMP)", attempts);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException or Polly.Timeout.TimeoutRejectedException)
        {
            logger.LogWarning(ex, "ADT: remoção de {Class} falhou", cls);
            return new ScanClassRemoval(cls, false, $"NÃO REMOVIDA: {ex.GetBaseException().Message}", attempts);
        }
    }

    /// <summary>Pacote da classe na TADIR (R3TR CLAS); nulo se não existe.</summary>
    private async Task<Result<string?>> ClassInTadirAsync(AdtSession s, string cls, CancellationToken ct)
    {
        var r = await SelectCoreAsync(s, $"SELECT obj_name, devclass, author FROM tadir WHERE pgmid = 'R3TR' AND object = 'CLAS' AND obj_name = '{cls.Replace("'", "''", StringComparison.Ordinal)}'", 1, ct);
        if (r.IsFailure)
            return Result.Failure<string?>(r.Error);
        return Result.Success<string?>(r.Value.Rows.Count == 0 ? null : Col(r.Value.Rows[0], "DEVCLASS"));
    }

    private static string Col(IReadOnlyDictionary<string, string> row, string name) => row.TryGetValue(name, out var v) ? v.Trim() : string.Empty;

    private static string SecurityElementEscape(string s) => System.Security.SecurityElement.Escape(s) ?? string.Empty;

    private static string HttpText(int status, string body) => AdtXml.Message(status, body);

    [GeneratedRegex("<CHECK_RESULT>([^<]*)</CHECK_RESULT>")]
    private static partial Regex ScanCheckResultRegex();
}
