namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// "O que cada papel analisa" por categoria de regra: texto fixo do <c>analise_mignow.py</c> (<c>PAPEIS</c>). É recomendação
/// da IA (MIGNOW), não decisão registrada. Serve de resposta do Fake e de reserva quando a IA real falha.
/// </summary>
public static class MignowRoleTexts
{
    public const string Purpose = "report.mignow.roles";
    public const string Abap = "Consultor ABAP";
    public const string Functional = "Funcional";
    public const string Customer = "Cliente";

    public static readonly IReadOnlyDictionary<string, (string Main, IReadOnlyDictionary<string, string> Texts)> ByCategory =
        new Dictionary<string, (string, IReadOnlyDictionary<string, string>)>
        {
            ["TABELA"] = (Abap, Roles(
                "Confirmar no sistema se a tabela/visão ainda é a fonte correta no S/4HANA (ver a descrição da regra) e, se não for, propor a troca pela CDS View, BAPI ou API equivalente, mantendo os mesmos campos lidos.",
                "Confirmar se o processo que usa este acesso continua ativo, que dados ele precisa ler e qual o cenário de teste.",
                "Indicar se o objeto está em uso produtivo (prioridade) e liberar o teste do processo.")),
            ["CAMPO"] = (Abap, Roles(
                "Analisar o uso do campo no novo modelo de dados indicado na regra e ajustar a leitura, se necessário.",
                "Confirmar se o processo usa condições / configuração de variantes neste ponto e definir o teste.",
                "Indicar se o objeto está em uso produtivo.")),
            ["CONVERSION_EXIT"] = (Abap, Roles(
                "Verificar se o campo convertido é número de material (MATNR, 40 posições no S/4HANA) ou outro campo (fornecedor, cliente, documento…): só o material exige ajuste da conversão.",
                "Informar se o processo trata materiais com numeração longa/externa.",
                "Indicar se o objeto está em uso produtivo.")),
            ["BAPI"] = (Abap, Roles(
                "Conferir a interface da BAPI no S/4HANA (parâmetros, estruturas, campos de material de 40 posições) e ajustar a chamada.",
                "Validar o resultado da chamada no processo (material, compras, vendas, classificação) com um caso de teste.",
                "Liberar dados e ambiente de teste.")),
            ["FUNCAO"] = (Abap, Roles(
                "Conferir a interface da função no S/4HANA e ajustar a chamada ou trocar pela API indicada na regra.",
                "Validar o resultado da chamada (classificação de objetos) com um caso de teste.",
                "Liberar dados e ambiente de teste.")),
            ["TRANSACTION"] = (Functional, Roles(
                "Trocar a chamada da transação pelo Business Partner (transação BP ou API) onde o programa chama a transação.",
                "Definir o fluxo no BP (papel de fornecedor/cliente) que substitui a transação e o teste.",
                "Aprovar a mudança de processo para o BP e orientar os usuários.")),
        };

    public static readonly IReadOnlyList<string> CategoryOrder = [.. ByCategory.Keys];

    private static IReadOnlyDictionary<string, string> Roles(string abap, string functional, string customer) =>
        new Dictionary<string, string> { [Abap] = abap, [Functional] = functional, [Customer] = customer };
}
