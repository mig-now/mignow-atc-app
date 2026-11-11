namespace Mignow.Atc.Application.Ports.Sap.Ddic;

/// <summary>Tipo do objeto do dicionário: estrutura (<c>STRU</c>), tabela (<c>TABL</c>), módulo de função e elemento de dados (só leitura).</summary>
public enum DdicKind
{
    Structure,
    Table,
    Function,
    DataElement,
}
