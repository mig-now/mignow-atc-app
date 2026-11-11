using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Módulo Exemption (skill atc-isencao): implementação ADT de <see cref="IAtcExemptions"/>. Leitura stateless; o POST do
/// pedido é a única escrita e começa com <c>GuardWriteAsync</c> (produção/T000=P/ilegível bloqueiam). O simulador
/// correspondente fica em <c>Simulator/SimulatedSap.Exemption.cs</c>.
/// </summary>
public sealed partial class AdtSap : IAtcExemptions
{
    private const string PolicySql = "SELECT item_key, value FROM sca_local_config WHERE grp = 'ATC' AND item_key = 'EXEMPTION.POLICY'";

    public async Task<Result<ExemptionPolicy>> GetPolicyAsync(SapConnection connection, CancellationToken ct)
    {
        var rows = await SelectAsync(connection, PolicySql, 10, ct);
        if (rows.IsFailure)
            return rows.Error;
        var first = rows.Value.Rows.FirstOrDefault();
        return ExemptionPolicy.From(first is not null && first.TryGetValue("VALUE", out var v) ? v : string.Empty, first is not null);
    }

    public Task<Result<ExemptionCustomizing>> GetCustomizingAsync(SapConnection connection, CancellationToken ct) =>
        Call(connection, "atc/customizing", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Get, $"{Root}/atc/customizing?sap-language={ExemptionXml.Language(connection)}", ct, accept: Xml);
            return Result.Success(ExemptionXml.ParseCustomizing(await Ok(response, "atc/customizing", ct)));
        });

    public async Task<Result<ExemptionDiscovery>> DiscoverAsync(SapConnection connection, CancellationToken ct)
    {
        var lang = ExemptionXml.Language(connection);
        var collections = await Call(connection, "discovery", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Get, $"{Root}/discovery?sap-language={lang}", ct, accept: "application/atomsvc+xml");
            return Result.Success(ExemptionXml.Collections(await Ok(response, "discovery", ct)));
        });
        if (collections.IsFailure)
            return collections.Error;
        var policy = await GetPolicyAsync(connection, ct);
        if (policy.IsFailure)
            return policy.Error;
        var customizing = await GetCustomizingAsync(connection, ct);
        if (customizing.IsFailure)
            return customizing.Error;
        var totals = await GetUserTotalsAsync(connection, ct);
        if (totals.IsFailure)
            return totals.Error;
        // Tabelas do customizing e informativos: ilegíveis não derrubam a descoberta (o resultado diz o que não deu para ler).
        var approvers = await CountAsync(connection, "SELECT * FROM satc_ci_approver", ct);
        var areas = await CountAsync(connection, "SELECT * FROM satc_ci_area", ct);
        var odata = await ColumnAsync(connection,
            "SELECT object, obj_name, devclass FROM tadir WHERE pgmid = 'R3TR' AND object IN ('IWSV','IWSG','SRVD','SRVB') AND ( obj_name LIKE '%ATC%EXEMP%' OR obj_name LIKE '%ATC%XMPT%' OR obj_name LIKE '%SATC%CI%' )",
            r => $"{r.GetValueOrDefault("OBJECT")} {r.GetValueOrDefault("OBJ_NAME")}".Trim(), ct);
        var rfc = await ColumnAsync(connection,
            "SELECT funcname, pname FROM tfdir WHERE fmode = 'R' AND ( funcname LIKE '%ATC%EXEMP%' OR funcname LIKE '%ATC%XMPT%' OR funcname LIKE '%SATC%XMPT%' )",
            r => r.GetValueOrDefault("FUNCNAME") ?? string.Empty, ct);
        return new ExemptionDiscovery(collections.Value.Contains($"{Root}/atc/exemptions/apply"), collections.Value, policy.Value, customizing.Value,
            totals.Value.Totals.TryGetValue("countTotal", out var total) ? total : null, approvers, areas, odata, rfc);
    }

    public Task<Result<ExemptionTemplate>> GetTemplateAsync(SapConnection connection, string markerId, CancellationToken ct) =>
        Call(connection, "atc/exemptions/apply", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Get, ExemptionXml.ApplyPath(markerId, ExemptionXml.Language(connection)), ct,
                accept: ExemptionXml.AcceptTemplate);
            return Result.Success(ExemptionXml.ParseTemplate((int)response.StatusCode, await response.Content.ReadAsStringAsync(ct)));
        });

    public async Task<Result<ExemptionAttempt>> PreviewAsync(SapConnection connection, ExemptionApply apply, CancellationToken ct)
    {
        var template = await GetTemplateAsync(connection, apply.MarkerId, ct);
        if (template.IsFailure)
            return template.Error;
        var attempt = ExemptionXml.Evaluate(template.Value);
        return attempt.Status == ExemptionRequestStatus.Simulated ? ExemptionXml.WithRequest(attempt, apply) : attempt;
    }

    public async Task<Result<ExemptionAttempt>> SubmitAsync(SapConnection connection, ExemptionApply apply, CancellationToken ct)
    {
        var guard = await GuardWriteAsync(connection, ct);
        if (guard.IsFailure)
            return guard.Error;
        var preview = await PreviewAsync(connection, apply, ct);
        if (preview.IsFailure || preview.Value.Status != ExemptionRequestStatus.Simulated)
            return preview;
        var request = preview.Value.RequestXml!;
        return await Call(connection, "atc/exemptions/apply", ct, async s =>
        {
            // POST stateless (o CsrfDelegatingHandler busca o token); o corpo é lido mesmo em HTTP de erro: traz o motivo do SAP.
            using var content = XmlContent(request, ExemptionXml.ContentTypeRequest);
            using var response = await s.SendAsync(HttpMethod.Post, ExemptionXml.PostPath(apply.MarkerId), ct, content: content, stateful: false,
                accept: ExemptionXml.ContentTypeRequest);
            var http = (int)response.StatusCode;
            var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
            var (message, type) = ExemptionXml.ParseStatus(body);
            return Result.Success(preview.Value with
            {
                Status = ExemptionXml.SubmitStatus(http, type), Message = message, ResponseXml = body, Http = http, SapType = type,
            });
        });
    }

    public Task<Result<ExemptionUserTotals>> GetUserTotalsAsync(SapConnection connection, CancellationToken ct) =>
        Call(connection, "atc/checkexemptionsview", ct, async s =>
        {
            using var response = await s.SendAsync(HttpMethod.Get,
                $"{Root}/atc/checkexemptionsview?requestedBy={Uri.EscapeDataString(connection.User)}&sap-language={ExemptionXml.Language(connection)}", ct, accept: Xml);
            var body = await response.Content.ReadAsStringAsync(ct);
            return Result.Success(new ExemptionUserTotals((int)response.StatusCode, ExemptionXml.ParseTotals(body)));
        });

    private async Task<int?> CountAsync(SapConnection connection, string sql, CancellationToken ct)
    {
        var result = await SelectAsync(connection, sql, 1000, ct);
        return result.IsSuccess ? result.Value.Rows.Count : null;
    }

    private async Task<IReadOnlyList<string>> ColumnAsync(SapConnection connection, string sql, Func<IReadOnlyDictionary<string, string>, string> pick,
        CancellationToken ct)
    {
        var result = await SelectAsync(connection, sql, 1000, ct);
        return result.IsSuccess ? [.. result.Value.Rows.Select(pick)] : [];
    }
}
