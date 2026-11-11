namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Tipo do objeto referenciado (coluna RObT) → pasta da KB (<c>ROBT_CAT</c> do atc.py).</summary>
public static class KbCategories
{
    private static readonly Dictionary<string, string> ByRefType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TABL"] = "tabelas", ["VIEW"] = "tabelas", ["DDLS"] = "tabelas", ["STOB"] = "tabelas", ["FUNC"] = "funcoes", ["TRAN"] = "transacoes",
        ["PROG"] = "programas", ["CLAS"] = "classes", ["INTF"] = "classes", ["DTEL"] = "tipos", ["DOMA"] = "tipos", ["TTYP"] = "tipos", ["STRU"] = "tipos",
    };

    public static readonly IReadOnlyList<string> All = ["tabelas", "funcoes", "classes", "tipos", "transacoes", "programas"];

    /// <summary>Pasta da KB do tipo; sem mapeamento, <c>programas</c> (as categorias válidas do <c>registrar</c>).</summary>
    public static string For(string? refType) => refType is not null && ByRefType.TryGetValue(refType, out var c) ? c : "programas";
}
