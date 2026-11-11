namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Modelo HTML do relatório (cópia do <c>modelo/relatorio.html</c> da skill atc-resultado); implementado na Infra.External.</summary>
public interface IReportTemplate
{
    string Html { get; }

    /// <summary>Folha de estilo do modelo (conteúdo do primeiro <c>&lt;style&gt;</c>), reutilizada pelas análises.</summary>
    string Css { get; }
}
