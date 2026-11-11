using System.Text.Json.Nodes;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>Resumo (<c>resumo.json</c>), o texto do <c>resumo.md</c> e o nome do CSV P1; as ocorrências já saem com <c>Cross</c> preenchido.</summary>
public sealed record ScanCrossResult(JsonObject Summary, string Markdown, string P1FileName, int P1Rows);
