namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>
/// Simulação da gravação (nada é gravado): categoria do mandante (T000-CCCATEGORY), indicador de alteração
/// (T000-CCNOCLIIND; 2 e 3 = repositório bloqueado) e a verificação de cada classe temporária. <see cref="ClientOk"/> =
/// categoria diferente de P e legível, sem bloqueio de alteração do repositório.
/// </summary>
public sealed record ScanSimulation(string ClientCategory, string NoChangeIndicator, bool ClientOk, IReadOnlyList<ScanClassCheck> Classes);
