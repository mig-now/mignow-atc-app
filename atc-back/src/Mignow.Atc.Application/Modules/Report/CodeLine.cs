namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Linha de código de um trecho das análises (número da linha no fonte e texto).</summary>
public sealed record CodeLine(int Line, string Text);
