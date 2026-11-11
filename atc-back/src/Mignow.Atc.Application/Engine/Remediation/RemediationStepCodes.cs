namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>Códigos dos passos de leitura de fontes e conferência da R1 (o front traduz <c>steps.&lt;código&gt;</c>).</summary>
public static class RemediationStepCodes
{
    public const string ReadSourcesPlan = "READ_SOURCES_PLAN";
    public const string ReadSourcesFetch = "READ_SOURCES_FETCH";
    public const string ConfirmR1Check = "CONFIRM_R1_CHECK";
    public const string ConfirmR1Apply = "CONFIRM_R1_APPLY";
}
