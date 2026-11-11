namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>Resultado de um SELECT pelo data preview: colunas (na ordem) e linhas (coluna → valor, sem espaços nas pontas).</summary>
public sealed record SqlResult(IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyDictionary<string, string>> Rows)
{
    public static readonly SqlResult Empty = new([], []);
}
