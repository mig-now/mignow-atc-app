using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap.Ddic;

/// <summary>Leitura de DDIC standard (porte de <c>ler_ddic.ps1</c>, só a parte do SAP; o cache da KB é do módulo Knowledge).</summary>
public interface IDdicReader
{
    /// <summary>
    /// Procura a definição: com <paramref name="kind"/> só nesse tipo; sem ele, tenta estrutura, tabela, módulo de função e
    /// elemento de dados (nessa ordem). Não achar não é erro (<see cref="DdicDefinition.Found"/> falso).
    /// </summary>
    Task<Result<DdicDefinition>> ReadDefinitionAsync(SapConnection connection, string name, DdicKind? kind, CancellationToken ct);
}
