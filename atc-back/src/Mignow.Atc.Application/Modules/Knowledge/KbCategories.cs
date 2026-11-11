namespace Mignow.Atc.Application.Modules.Knowledge;

/// <summary>Categorias (pastas) da KB e o mapa RObT → categoria (porte de <c>ROBT_CAT</c>/<c>CATS</c> do atc.py).</summary>
public static class KbCategories
{
    public static readonly IReadOnlyList<string> All = ["tabelas", "funcoes", "classes", "tipos", "transacoes", "programas"];

    private static readonly Dictionary<string, string> RobtToCategory = new(StringComparer.Ordinal)
    {
        ["TABL"] = "tabelas", ["VIEW"] = "tabelas", ["DDLS"] = "tabelas", ["STOB"] = "tabelas",
        ["FUNC"] = "funcoes", ["TRAN"] = "transacoes", ["PROG"] = "programas",
        ["CLAS"] = "classes", ["INTF"] = "classes",
        ["DTEL"] = "tipos", ["DOMA"] = "tipos", ["TTYP"] = "tipos", ["STRU"] = "tipos",
    };

    /// <summary>Categorias na ordem de busca do <c>kb_lookup</c>: a do RObT (se houver) e depois as demais.</summary>
    public static IReadOnlyList<string> SearchOrder(string? robt)
    {
        var cats = new List<string>();
        if (robt is not null && RobtToCategory.TryGetValue(robt, out var first))
            cats.Add(first);
        cats.AddRange(All.Where(c => !cats.Contains(c)));
        return cats;
    }

    public static bool IsValid(string category) => All.Contains(category);
}
