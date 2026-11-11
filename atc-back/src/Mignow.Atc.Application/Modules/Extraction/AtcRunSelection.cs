using Mignow.Atc.Application.Ports.Sap.Extraction;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>Resultado da escolha da execução: a última concluída, as mais novas ainda em andamento e quantas execuções da série existem.</summary>
public sealed record AtcRunSelection(AtcRunHeader? Latest, IReadOnlyList<AtcRunHeader> InProgress, int ExecutionsFound);
