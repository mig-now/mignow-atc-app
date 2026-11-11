using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// ÚNICO ponto de escrita no SAP. Todo método chama <see cref="EnsureWriteAllowedAsync"/> antes (porte de
/// <c>Enable-AdtEscrita</c>): bloqueia se o cadastro marca produção, se T000-CCCATEGORY = P ou se a categoria não pôde
/// ser lida. Escritas de módulo (DDIC, classe temporária, isenção) ficam em portas próprias em <c>Ports/Sap/&lt;Módulo&gt;/</c>
/// e seguem a mesma trava (helper <c>GuardWriteAsync</c> no adapter).
/// </summary>
public interface ISapWriter
{
    Task<Result<WriteGuardInfo>> EnsureWriteAllowedAsync(SapConnection connection, CancellationToken ct);

    /// <summary>LOCK → (confere a request do lock) → PUT com corrNr → UNLOCK em finally → ativação stateless.</summary>
    Task<Result<SourceWriteOutcome>> WriteSourceAsync(SapConnection connection, SourceWriteRequest request, CancellationToken ct);

    /// <summary>Ativação stateless; include com <c>?context=</c> do programa/grupo.</summary>
    Task<Result<ActivationResult>> ActivateAsync(SapConnection connection, string objectUri, string objectName, string? contextUri, CancellationToken ct);
}
