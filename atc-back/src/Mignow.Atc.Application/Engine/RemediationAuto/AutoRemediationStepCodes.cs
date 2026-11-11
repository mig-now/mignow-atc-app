namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>Códigos dos passos próprios da remediação automática (o front traduz <c>steps.&lt;código&gt;</c>); os demais passos são reaproveitados.</summary>
public static class AutoRemediationStepCodes
{
    public const string Plan = "AUTO_SOURCES_PLAN";
    public const string Triage = "AUTO_TRIAGE";
    public const string ProposeAi = "AUTO_PROPOSE_AI";
    public const string Confirm = "AUTO_REMEDIATION_CONFIRM";
    public const string Summary = "AUTO_REMEDIATION_SUMMARY";
}
