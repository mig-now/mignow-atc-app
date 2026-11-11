namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// Gravação de um fonte (porte de <c>aplicar_adt.ps1</c>): <see cref="ObjectUri"/> recebe LOCK/UNLOCK e ativação,
/// <see cref="SourceUri"/> recebe o PUT. <see cref="ActivationContextUri"/> = programa/grupo do include (<c>?context=</c>).
/// <see cref="Rollback"/> = gravação do backup (resultados <c>ROLLBACK_ATIVADO</c>/<c>ERRO_ROLLBACK_ATIVACAO</c>).
/// </summary>
public sealed record SourceWriteRequest(
    string ObjectUri, string ObjectName, string SourceUri, string Source, string TransportRequest, string? ActivationContextUri, bool Rollback = false);
