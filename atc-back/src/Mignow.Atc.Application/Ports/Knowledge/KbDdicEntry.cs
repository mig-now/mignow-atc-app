namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Item do cache DDIC da KB (<c>ddic/&lt;tipo&gt;/&lt;NOME&gt;.abap</c>); tipo = elementos_dados, estruturas, funcoes ou tabelas.</summary>
public sealed record KbDdicEntry(string Kind, string Name);
