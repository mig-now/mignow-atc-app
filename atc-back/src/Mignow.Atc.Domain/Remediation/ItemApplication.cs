namespace Mignow.Atc.Domain.Remediation;

/// <summary>
/// Aplicação no SAP (<c>aplicacao</c> do atc.py / aplicar_adt): resultado (<c>ATIVADO</c>, <c>ERRO_ATIVACAO</c>,
/// <c>ERRO_GRAVACAO</c>, <c>ERRO_LOCK</c>, <c>ERRO_REQUEST</c>, <c>ABORTADO_FONTE_MUDOU</c>…), request, backup do fonte
/// ativo, decisão sobre erro de ativação (manter inativo/rollback), backups dos objetos relacionados (interface cuja assinatura muda com a
/// classe; <c>-Objeto</c> do <c>aplicar_adt.ps1</c>) e histórico. Coluna jsonb.
/// </summary>
public sealed record ItemApplication(
    string? Result,
    string? Request,
    string? BackupSource,
    string? ErrorDecision,
    IReadOnlyList<string>? Messages,
    IReadOnlyList<ApplicationAttempt> History,
    IReadOnlyList<RelatedBackup>? RelatedBackups = null);

/// <summary><see cref="DdicAction"/> = <c>criar</c>|<c>alterar</c> (só estrutura/tabela do DDIC; criar não tem rollback).</summary>
public sealed record ApplicationAttempt(DateTimeOffset At, string Mode, string Result, string? Request, string? Detail, string? DdicAction = null)
{
    public const string DdicCreate = "criar";
    public const string DdicAlter = "alterar";
}

/// <summary>Backup do fonte ativo de um objeto relacionado, tirado antes da aplicação (base do rollback do relacionado).</summary>
public sealed record RelatedBackup(string ObjectName, string ObjectType, string? SourceUri, string BackupSource);
