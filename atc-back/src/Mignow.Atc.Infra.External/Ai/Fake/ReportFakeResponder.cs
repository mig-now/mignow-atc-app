using System.Text.Json;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Application.Ports.Ai;

namespace Mignow.Atc.Infra.External.Ai.Fake;

/// <summary>
/// IA de desenvolvimento do módulo Report: responde "o que cada papel analisa" (análise MIGNOW) com os textos fixos do
/// <c>analise_mignow.py</c> (<see cref="MignowRoleTexts"/>). A categoria vem na primeira linha do pedido: <c>Categoria: X</c>.
/// </summary>
public sealed class ReportFakeResponder : IFakeAiResponder
{
    public IReadOnlyCollection<string> Purposes { get; } = [MignowRoleTexts.Purpose];

    public string Respond(AiRequest request)
    {
        var first = request.User.Split('\n', 2)[0];
        var category = first.StartsWith("Categoria:", StringComparison.Ordinal) ? first["Categoria:".Length..].Trim() : "TABELA";
        var texts = (MignowRoleTexts.ByCategory.TryGetValue(category, out var t) ? t : MignowRoleTexts.ByCategory["TABELA"]).Texts;
        return JsonSerializer.Serialize(new
        {
            consultorAbap = texts[MignowRoleTexts.Abap], funcional = texts[MignowRoleTexts.Functional], cliente = texts[MignowRoleTexts.Customer],
        });
    }
}
