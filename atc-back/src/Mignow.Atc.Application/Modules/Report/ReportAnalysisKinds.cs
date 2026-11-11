namespace Mignow.Atc.Application.Modules.Report;

public static class ReportAnalysisKinds
{
    public static ReportAnalysisKind? Parse(string? route) => route?.ToLowerInvariant() switch
    {
        "isencao" => ReportAnalysisKind.Exemption,
        "pendencias" => ReportAnalysisKind.Pending,
        "mignow" => ReportAnalysisKind.Mignow,
        _ => null,
    };

    public static string Route(ReportAnalysisKind kind) => kind switch
    {
        ReportAnalysisKind.Exemption => "isencao",
        ReportAnalysisKind.Pending => "pendencias",
        _ => "mignow",
    };
}
