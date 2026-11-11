namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Códigos dos passos de proposta e aplicação (o front traduz <c>steps.&lt;código&gt;</c>).</summary>
public static class RemediationApplyStepCodes
{
    public const string Propose = "PROPOSE";
    public const string AutoConfirm = "AUTO_CONFIRM";
    public const string AutoSimulate = "AUTO_SIMULATE";
    public const string AutoReview = "AUTO_REVIEW";
    public const string AutoApply = "AUTO_APPLY";
    public const string BatchReport = "BATCH_REPORT";
    public const string Simulate = "SIMULATE";
    public const string Review = "REVIEW";
    public const string Apply = "APPLY";
    public const string Rollback = "ROLLBACK";
}
