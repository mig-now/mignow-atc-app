namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>
/// Regra ativa como a classe a aplica (o que o <c>gerar.py</c> embute no ABAP). O adapter ADT ignora (o código vai no
/// <see cref="ScanLotRequest.Source"/>); o simulador executa estas regex sobre os fontes do cenário.
/// <see cref="Pcre"/> = refinada (sem o prefixo <c>(?i)</c>); <see cref="OriginalPcre"/> = a original do cliente.
/// </summary>
public sealed record ScanRuleSpec(
    string Id, string Pcre, string Text, string FixedElement, string Requires, string SuppressedBy, string OriginalPcre);
