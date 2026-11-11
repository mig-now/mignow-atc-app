namespace Mignow.Atc.Application.Ports.Sap.Ddic;

/// <summary>
/// Gravação de estrutura/tabela (porte do trecho de gravação do <c>aplicar_ddic.ps1</c>): [criação com
/// <see cref="Package"/>/<see cref="Description"/>] → lock → PUT com a request → unlock → ativação.
/// <see cref="Rollback"/> = gravação do backup (resultados <c>ROLLBACK_ATIVADO</c>/<c>ERRO_ROLLBACK_ATIVACAO</c>).
/// </summary>
public sealed record DdicWriteRequest(
    DdicKind Kind, string Name, string Source, string TransportRequest, bool Create, string? Package, string? Description, string? Responsible,
    bool Rollback = false);
