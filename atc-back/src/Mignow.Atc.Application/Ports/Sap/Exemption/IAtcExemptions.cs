using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>
/// Pedido de isenção ATC pelo ADT (skill atc-isencao). Leituras stateless; <see cref="SubmitAsync"/> é ESCRITA: começa com
/// a trava do núcleo (produção no cadastro, <c>T000-CCCATEGORY = P</c> ou categoria ilegível bloqueiam). O idioma é o da
/// conexão (<c>sap-language</c>).
/// </summary>
public interface IAtcExemptions
{
    /// <summary>Política <c>EXEMPTION.POLICY</c> (SQL em <c>SCA_LOCAL_CONFIG</c>).</summary>
    Task<Result<ExemptionPolicy>> GetPolicyAsync(SapConnection connection, CancellationToken ct);

    /// <summary>Motivos e validades (<c>/sap/bc/adt/atc/customizing</c>).</summary>
    Task<Result<ExemptionCustomizing>> GetCustomizingAsync(SapConnection connection, CancellationToken ct);

    /// <summary>Discovery ADT + política + customizing + aprovadores (tabelas SATC_CI_*) + totais do usuário.</summary>
    Task<Result<ExemptionDiscovery>> DiscoverAsync(SapConnection connection, CancellationToken ct);

    /// <summary>Modelo do pedido do finding (<c>GET exemptions/apply?markerId=</c>, Accept <c>application/atc.xmptapp.v1+xml</c>).</summary>
    Task<Result<ExemptionTemplate>> GetTemplateAsync(SapConnection connection, string markerId, CancellationToken ct);

    /// <summary>Simulação (somente leitura): lê o modelo e monta o XML que seria enviado. Sem escopo <c>thisFinding</c> = SEM_ESCOPO.</summary>
    Task<Result<ExemptionAttempt>> PreviewAsync(SapConnection connection, ExemptionApply apply, CancellationToken ct);

    /// <summary>ESCRITA: trava do núcleo; lê o modelo de novo, monta e envia <c>POST exemptions/apply</c> (<c>application/atc.xmpt.v1+xml</c>).</summary>
    Task<Result<ExemptionAttempt>> SubmitAsync(SapConnection connection, ExemptionApply apply, CancellationToken ct);

    /// <summary>Totais do usuário em <c>checkexemptionsview?requestedBy=</c>.</summary>
    Task<Result<ExemptionUserTotals>> GetUserTotalsAsync(SapConnection connection, CancellationToken ct);
}
