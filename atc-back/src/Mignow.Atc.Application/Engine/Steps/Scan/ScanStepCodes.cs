namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Códigos dos passos da varredura MIGNOW (o front traduz <c>steps.&lt;código&gt;</c>).</summary>
public static class ScanStepCodes
{
    public const string Inventory = "SCAN_INVENTORY";
    public const string Simulate = "SCAN_SIMULATE";
    public const string Prepare = "SCAN_PREPARE";
    public const string Calibrate = "SCAN_CALIBRATE";
    public const string Lots = "SCAN_LOTS";
    public const string Cross = "SCAN_CROSS";
    public const string Cleanup = "SCAN_CLEANUP";
}
