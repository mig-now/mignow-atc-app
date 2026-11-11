namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>Finding do CSV total da extração reduzido ao que o cruzamento usa: <c>location</c> e <c>tag_REF_OBJ_NAME</c>.</summary>
public sealed record ScanAtcFinding(string Location, string RefObjName);
