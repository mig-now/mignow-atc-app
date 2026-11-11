namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>Parâmetros do run <c>ReadSources</c> (<c>ler_fontes_lote.ps1</c>): lote = os N primeiros pendentes da fila; standard só com confirmação; <c>Force</c> relê o que já está em cache.</summary>
public sealed record ReadSourcesParameters(Guid SessionId, int Batch, bool IncludeStandard, bool Force);
