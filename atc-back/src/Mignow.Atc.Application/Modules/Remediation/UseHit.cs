namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Uso do objeto referenciado num fonte (<c>locate</c> do atc.py): nome do fonte (include), linha e o trecho numerado com contexto.</summary>
public sealed record UseHit(string Source, int Line, string Excerpt);
