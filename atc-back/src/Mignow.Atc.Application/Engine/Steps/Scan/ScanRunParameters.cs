namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Parâmetros do run <c>ScanExecute</c>: lotes (<c>1</c>, <c>2-9</c>, <c>1,3</c>; vazio = todos), refazer os já OK, paralelo (padrão = configuração) e CSV com ATC_OUTRA_LINHA.</summary>
public sealed record ScanExecuteParameters(string? Lots = null, bool Redo = false, int? Parallel = null, bool IncludeOtherLine = false, bool CompareOriginal = true);

/// <summary>Parâmetros do run <c>ScanInventory</c>.</summary>
public sealed record ScanInventoryParameters(bool? IncludeGenerated = null);
