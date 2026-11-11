using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Infra.External.Sap;

/// <summary>Erros dos adapters SAP (código estável + texto localizado). Falha de comunicação = Upstream (HTTP 502).</summary>
internal static class SapErrors
{
    public static Error ObjectNotFound(IStringLocalizer<Errors> l, string name) => l.NotFound("Sap.ObjectNotFound", name);

    public static Error CertificateRejected(IStringLocalizer<Errors> l) => l.Upstream("Sap.CertificateRejected");

    public static Error Upstream(IStringLocalizer<Errors> l, string detail) => l.Upstream("Sap.Upstream", detail);

    public static Error SqlFailed(IStringLocalizer<Errors> l, string detail) => l.Upstream("Sap.SqlFailed", detail);

    public static Error OnlySelect(IStringLocalizer<Errors> l) => l.Validation("Sap.OnlySelect");

    public static Error SimulatorNotAllowed(IStringLocalizer<Errors> l) => l.Validation("Sap.SimulatorNotAllowed");

    public static Error ScenarioNotFound(IStringLocalizer<Errors> l, string name) => l.NotFound("Sap.ScenarioNotFound", name);

    public static Error NotImplemented(IStringLocalizer<Errors> l, string operation) => l.Failure("Sap.NotImplemented", operation);
}
