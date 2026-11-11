namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>
/// Linha do <c>_novos.csv</c> da extração mais recente (colunas <c>quickfixInfo</c>, <c>obj_name</c>, <c>obj_type</c>,
/// <c>tag_REF_OBJ_NAME</c>, <c>messageTitle</c>, <c>priority</c>, <c>checksum</c>, <c>location</c>, <c>exemptionApproval</c>).
/// </summary>
public sealed record ExemptionListingFinding(
    string MarkerId, string ObjectName, string ObjectType, string RefObjectName, string MessageTitle, string Priority, string Checksum, string Location,
    string ExemptionApproval);
