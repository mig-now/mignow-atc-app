namespace Mignow.Atc.Domain.Registry;

/// <summary>
/// Finalidade da chamada de IA. Não escolhe o modelo (um só modelo para tudo, decisão de 09.11.2026): define o esforço de
/// raciocínio e o teto de saída e fica registrada em cada chamada (histórico e custos).
/// </summary>
public enum AiUsage
{
    /// <summary>Tarefa mecânica: aplicar cartão da KB a grupo simples (esforço baixo).</summary>
    Mechanical,
    /// <summary>Revisão antes de gravar, grupo médio, textos do relatório (esforço médio).</summary>
    Review,
    /// <summary>Análise da IA e grupos complexos (esforço alto).</summary>
    Analysis,
}
