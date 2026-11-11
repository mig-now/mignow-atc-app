using System.Text.RegularExpressions;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Infra.External.Report;

/// <summary>Modelo do relatório embutido (<see cref="ReportTemplateSource"/>, cópia do modelo da skill atc-resultado).</summary>
public sealed partial class EmbeddedReportTemplate : IReportTemplate
{
    private static readonly string StyleBlock = StyleRx().Match(ReportTemplateSource.Html) is { Success: true } m ? m.Groups[1].Value : string.Empty;

    public string Html => ReportTemplateSource.Html;

    public string Css => StyleBlock;

    [GeneratedRegex("<style>(.*?)</style>", RegexOptions.Singleline)]
    private static partial Regex StyleRx();
}
