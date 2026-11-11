namespace Mignow.Atc.Application.Ports.Sap.Ddic;

/// <summary>
/// Definição standard lida do sistema (porte de <c>ler_ddic.ps1</c>): <see cref="Kind"/> = tipo onde foi achada;
/// <see cref="Source"/> nulo = não encontrada em nenhum dos tipos tentados.
/// </summary>
public sealed record DdicDefinition(string Name, DdicKind? Kind, string? Source)
{
    public bool Found => Source is not null;
}
