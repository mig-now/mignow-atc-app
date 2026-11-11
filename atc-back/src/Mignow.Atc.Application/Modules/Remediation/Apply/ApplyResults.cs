using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Ddic;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Códigos de resultado da aplicação (<c>resultado</c> do <c>aplicar_adt.ps1</c>/<c>aplicar_ddic.ps1</c>), mais os do porte
/// (<see cref="PreparationError"/>, <see cref="NotReady"/>). Os de gravação vêm do SAP (<see cref="SourceWriteOutcome"/>/<see cref="DdicWriteOutcome"/>).
/// </summary>
public static class ApplyResults
{
    /// <summary>
    /// Resultados que não são falha (ativado, só simulado, nada a fazer). Qualquer outro resultado = o código no SAP não ficou como a proposta
    /// (erro de lock/request/gravação/ativação, trava de produção, abortado, rollback…), mesmo com o item "Confirmado".
    /// </summary>
    public static readonly string[] NotFailed = [Activated, Simulated, NothingToDo];

    public static bool IsFailure(string? result) => result is not null && !NotFailed.Contains(result);

    /// <summary>
    /// Falha que o play pode reaplicar (simular → revisar → aplicar de novo): request, lock, gravação, trava de produção, fonte mudou, preparação.
    /// Erro de ativação e rollback ficam com o consultor (o fonte já foi gravado inativo ou revertido: decisão manter/rollback).
    /// </summary>
    public static bool Reprocessable(string? result) =>
        IsFailure(result) && result is not (ActivationError or RollbackActivationError or RollbackActivated or RollbackUnavailable or NotReady);

    public const string Simulated = "SIMULADO";
    public const string Activated = SourceWriteOutcome.Activated;
    public const string RollbackActivated = SourceWriteOutcome.RollbackActivated;
    public const string ActivationError = SourceWriteOutcome.ActivationError;
    public const string RollbackActivationError = SourceWriteOutcome.RollbackActivationError;
    public const string WriteError = SourceWriteOutcome.WriteError;
    public const string LockError = SourceWriteOutcome.LockError;
    public const string RequestError = SourceWriteOutcome.RequestError;
    public const string GenericError = SourceWriteOutcome.GenericError;
    public const string SourceChanged = "ABORTADO_FONTE_MUDOU";
    public const string TableStructureAborted = "ABORTADO_ESTRUTURA_TABELA";
    public const string ValidationError = "ERRO_VALIDACAO";
    public const string CreationError = DdicWriteOutcome.CreationError;
    public const string NothingToDo = "NADA_A_FAZER";
    public const string RollbackUnavailable = "ROLLBACK_INDISPONIVEL";

    /// <summary>O trecho da proposta não se encontra mais no fonte (re-preparação falhou): não existe no script (lá o <c>preparar</c> falha).</summary>
    public const string PreparationError = "ERRO_PREPARACAO";

    /// <summary>Item sem proposta preparada ou sem decisão Confirmado KB/IA: nada foi feito.</summary>
    public const string NotReady = "NAO_PRONTO";

    public const string DecisionKeepInactive = "manter_inativa";
    public const string DecisionRollback = "rollback";

    public static bool IsSuccess(string? result) => result is Activated or RollbackActivated;
}
