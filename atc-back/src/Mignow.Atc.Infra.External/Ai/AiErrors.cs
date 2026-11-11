using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Infra.External.Ai;

internal static class AiErrors
{
    public static Error NotConfigured(IStringLocalizer<Errors> l, string usage) => l.Conflict("Ai.NotConfigured", usage);

    public static Error Failed(IStringLocalizer<Errors> l, string detail) => l.Upstream("Ai.Failed", detail);

    public static Error Invalid(IStringLocalizer<Errors> l, string detail) => l.Upstream("Ai.InvalidResponse", detail);
}
