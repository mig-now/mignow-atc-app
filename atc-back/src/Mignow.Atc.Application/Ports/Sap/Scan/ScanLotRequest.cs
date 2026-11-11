namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>
/// Um lote a executar: nº (3 dígitos no ABAP), o código ABAP completo da classe (<see cref="Source"/>, o que o ADT grava) e,
/// só para o simulador (que não executa ABAP), os objetos, o gatilho, as regras e se compara com as originais.
/// </summary>
public sealed record ScanLotRequest(
    int Number, string Source, IReadOnlyList<ScanLotObject> Objects, string Trigger, IReadOnlyList<ScanRuleSpec> Rules, bool CompareOriginal);
