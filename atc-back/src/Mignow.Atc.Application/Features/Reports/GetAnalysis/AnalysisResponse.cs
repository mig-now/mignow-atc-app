namespace Mignow.Atc.Application.Features.Reports.GetAnalysis;

/// <summary>Resumo de um relatório de análise (equivale ao JSON de saída dos <c>analise_*.py</c>); o HTML vem em <c>.html</c>.</summary>
public sealed record AnalysisResponse(string Kind, string Title, int Items, int Objects, int Groups, int WithCode, IReadOnlyDictionary<string, int> ByGroup,
    IReadOnlyDictionary<string, int> Extra);
