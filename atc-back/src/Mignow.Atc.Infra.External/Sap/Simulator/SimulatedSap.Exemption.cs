using System.Security;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Infra.External.Sap.Adt;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Módulo Exemption: <see cref="IAtcExemptions"/> simulada (as mesmas regras do <c>AdtSap.Exemption.cs</c>: política,
/// modelo por markerId, escopo <c>thisFinding</c>, aprovador diferente do solicitante, "Exemption already requested" depois
/// do pedido, trava de escrita). O modelo e a resposta saem em XML no formato do SAP, e o corpo do pedido é montado pelo
/// mesmo <c>ExemptionXml</c> do adapter ADT. Falha injetável: <c>InjectFailure("EXEMPT_POST", markerId)</c>.
/// </summary>
public sealed partial class SimulatedSap : IAtcExemptions
{
    public const string AlreadyRequestedMessage = "Exemption already requested";

    public Task<Result<ExemptionPolicy>> GetPolicyAsync(SapConnection connection, CancellationToken ct) =>
        With(connection, s =>
        {
            s.Record("EXEMPTION_POLICY");
            return Result.Success(ExemptionPolicy.From(s.ExemptionConfig.Policy, s.ExemptionConfig.Policy is not null));
        });

    public Task<Result<ExemptionCustomizing>> GetCustomizingAsync(SapConnection connection, CancellationToken ct) =>
        With(connection, s => Result.Success(Customizing(s.ExemptionConfig)));

    public Task<Result<ExemptionDiscovery>> DiscoverAsync(SapConnection connection, CancellationToken ct) =>
        With(connection, s =>
        {
            s.Record("EXEMPTION_DISCOVER");
            var cfg = s.ExemptionConfig;
            string[] collections = cfg.ApplyPublished
                ? [$"{AdtSession.AdtRoot}/atc/checkexemptionsview", $"{AdtSession.AdtRoot}/atc/exemptions/apply"]
                : [$"{AdtSession.AdtRoot}/atc/checkexemptionsview"];
            return Result.Success(new ExemptionDiscovery(cfg.ApplyPublished, collections, ExemptionPolicy.From(cfg.Policy, cfg.Policy is not null),
                Customizing(cfg), UserTotal(s, connection), cfg.Approvers.Count, cfg.Areas.Count, [], []));
        });

    public Task<Result<ExemptionTemplate>> GetTemplateAsync(SapConnection connection, string markerId, CancellationToken ct) =>
        With(connection, s => Result.Success(Template(s, markerId)));

    public Task<Result<ExemptionAttempt>> PreviewAsync(SapConnection connection, ExemptionApply apply, CancellationToken ct) =>
        With(connection, s => Result.Success(Preview(s, apply)));

    public Task<Result<ExemptionAttempt>> SubmitAsync(SapConnection connection, ExemptionApply apply, CancellationToken ct) =>
        With(connection, s =>
        {
            var guard = GuardWrite(s, connection);
            if (guard.IsFailure)
                return Result.Failure<ExemptionAttempt>(guard.Error);
            var preview = Preview(s, apply);
            if (preview.Status != ExemptionRequestStatus.Simulated)
                return Result.Success(preview);
            s.Record($"EXEMPTION_POST {apply.MarkerId}");
            if (s.ShouldFail("EXEMPT_POST", apply.MarkerId))
                return Result.Success(Answer(preview, 500, "E", "falha simulada"));
            // Princípio dos 4 olhos do SAP: o aprovador não pode ser quem pede.
            if (string.Equals(apply.Approver, connection.User, StringComparison.OrdinalIgnoreCase))
                return Result.Success(Answer(preview, 400, "E", "Approver must be different from the requester"));
            var cfg = s.ExemptionConfig;
            if (cfg.Reasons.FirstOrDefault(r => r.Id == apply.Reason) is not { } reason)
                return Result.Success(Answer(preview, 400, "E", $"Reason {apply.Reason} is not valid"));
            if (reason.JustificationMandatory && string.IsNullOrWhiteSpace(apply.Justification))
                return Result.Success(Answer(preview, 400, "E", "Justification is mandatory"));
            s.AddExemption(apply.MarkerId, new SimulatedExemption(connection.User, apply.Approver.ToUpperInvariant(), apply.Reason, apply.Justification,
                preview.RequestXml!));
            return Result.Success(Answer(preview, 200, "S", "Exemption request created"));
        });

    public Task<Result<ExemptionUserTotals>> GetUserTotalsAsync(SapConnection connection, CancellationToken ct) =>
        With(connection, s =>
        {
            var total = UserTotal(s, connection) ?? 0;
            return Result.Success(new ExemptionUserTotals(200, new Dictionary<string, int> { ["countTotal"] = total, ["countOpen"] = total }));
        });

    // ---------------------------------------------------------------- auxiliares (dentro do lock do estado)

    private static ExemptionCustomizing Customizing(ExemptionScenario cfg) =>
        new([.. cfg.Reasons.Select(r => new ExemptionReason(r.Id, r.JustificationMandatory, r.Title))], cfg.Validities);

    private static int? UserTotal(SimulatedSapState s, SapConnection connection) =>
        s.ExemptionConfig.FixedUserTotal ?? s.Exemptions.Values.Count(e => string.Equals(e.RequestedBy, connection.User, StringComparison.OrdinalIgnoreCase));

    private static ExemptionAttempt Answer(ExemptionAttempt preview, int http, string type, string message)
    {
        var body = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><atcexmpt:status xmlns:atcexmpt=\"{ExemptionXml.NsXmpt}\"><atcexmpt:message>{SecurityElement.Escape(message)}</atcexmpt:message><atcexmpt:type>{type}</atcexmpt:type></atcexmpt:status>";
        var (parsed, parsedType) = ExemptionXml.ParseStatus(body);
        return preview with { Status = ExemptionXml.SubmitStatus(http, parsedType), Message = parsed, ResponseXml = body, Http = http, SapType = parsedType };
    }

    private static ExemptionAttempt Preview(SimulatedSapState s, ExemptionApply apply)
    {
        var attempt = ExemptionXml.Evaluate(Template(s, apply.MarkerId));
        return attempt.Status == ExemptionRequestStatus.Simulated ? ExemptionXml.WithRequest(attempt, apply) : attempt;
    }

    /// <summary>Modelo do pedido no formato do SAP (<c>atcexmpt:exemptionApply</c>) ou só o status que impede o pedido.</summary>
    private static ExemptionTemplate Template(SimulatedSapState s, string markerId)
    {
        s.Record($"EXEMPTION_TEMPLATE {markerId}");
        var cfg = s.ExemptionConfig;
        var finding = cfg.Findings.FirstOrDefault(f => f.MarkerId == markerId);
        string? blocked = !cfg.ApplyPublished ? "Exemptions disabled by system setup"
            : cfg.Policy != "A" ? "Exemptions disabled by system setup"
            : s.HasExemption(markerId) ? AlreadyRequestedMessage
            : finding?.BlockMessage;
        string xml;
        if (blocked is not null)
            xml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><atcexmpt:exemptionApply xmlns:atcexmpt=\"{ExemptionXml.NsXmpt}\"><atcexmpt:status><atcexmpt:message>{SecurityElement.Escape(blocked)}</atcexmpt:message><atcexmpt:type>E</atcexmpt:type></atcexmpt:status></atcexmpt:exemptionApply>";
        else
        {
            var type = finding?.Type ?? "PROG/P";
            var checksum = finding?.Checksum ?? "123456";
            var name = SecurityElement.Escape(finding?.ObjectName ?? "ZPROG");
            var thisFinding = checksum != "-1" && type is not ("AQSG" or "AQQU" or "AQBG");
            xml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><atcexmpt:exemptionApply xmlns:atcexmpt=\"{ExemptionXml.NsXmpt}\" xmlns:atcfinding=\"{ExemptionXml.NsFinding}\" xmlns:adtcore=\"{ExemptionXml.NsCore}\">"
                  + "<atcexmpt:exemptionProposal>"
                  + $"<atcfinding:finding adtcore:name=\"{name}\" adtcore:type=\"{type}\" atcfinding:checksum=\"{checksum}\"/>"
                  + $"<atcexmpt:restriction><atcexmpt:thisFinding enabled=\"{(thisFinding ? "true" : "false")}\">false</atcexmpt:thisFinding><atcexmpt:rangeOfFindings enabled=\"true\"/></atcexmpt:restriction>"
                  + "<atcexmpt:approver/><atcexmpt:reason/><atcexmpt:justification/><atcexmpt:notify/></atcexmpt:exemptionProposal></atcexmpt:exemptionApply>";
        }
        return ExemptionXml.ParseTemplate(200, xml);
    }
}
