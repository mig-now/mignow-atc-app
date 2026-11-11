namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// Regra do <c>regras.json</c> (campos que a skill usa). <see cref="Pcre"/> roda sobre a sentença em maiúsculas (grupo 1 =
/// elemento quando <see cref="FixedElement"/> é vazio); <see cref="Text"/> L = sentença com literais, C = literais trocados
/// por <c>''</c>; <see cref="Requires"/> = contexto exigido (sem ele a ocorrência é informativa); <see cref="SuppressedBy"/> =
/// regra que, achando o mesmo elemento na sentença, faz esta não contar. <see cref="Priority"/> = texto de <c>prioridade</c>
/// (o <c>cruzar.py</c> usa <c>str()</c>, padrão 1).
/// </summary>
public sealed record MignowRule(
    string Id, bool Active, string Priority, string Category, string Severity, string Description, string OriginalPcre, string Pcre, string Text,
    string FixedElement, string Requires, string SuppressedBy, string Robt);
