using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Application.Services;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// As portas SAP sobre o simulador (<c>sim://cenario</c>, só Development/Local/Testing). Mesmas regras do adapter ADT
/// (trava de escrita por produção/T000, lock de outro usuário, objeto em outra request, ativação com erro), para o
/// pipeline inteiro ser testado sem SAP. <c>partial</c>: cada módulo implementa as portas dele em
/// <c>SimulatedSap.&lt;Módulo&gt;.cs</c>.
/// </summary>
public sealed partial class SimulatedSap(SimulatedSapRegistry registry, IStringLocalizer<Errors> l) : ISapReader, ISapSql, ISapWriter
{
    // ---------------------------------------------------------------- ISapReader

    public Task<Result<SapSystemInfo>> TestConnectionAsync(SapConnection connection, CancellationToken ct) =>
        With(connection, s => Result.Success(new SapSystemInfo(s.Scenario.Release, ["simulator"], s.ClientCategory, null)));

    public Task<Result<IReadOnlyList<SapObjectRef>>> SearchAsync(SapConnection connection, string query, string? adtType, int maxResults, CancellationToken ct) =>
        With(connection, s =>
        {
            var pattern = query.Trim().TrimEnd('*');
            return Result.Success<IReadOnlyList<SapObjectRef>>([.. s.Objects
                .Where(o => o.Ref.Name.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) && (adtType is null || o.Ref.Type == adtType))
                .Select(o => o.Ref).Take(Math.Max(1, maxResults))]);
        });

    public Task<Result<IReadOnlyList<SapSourcePart>>> GetObjectSourcesAsync(SapConnection connection, string name, string type, CancellationToken ct) =>
        With(connection, s =>
        {
            if (s.Get(name) is not { } o)
                return Result.Success<IReadOnlyList<SapSourcePart>>([]);
            s.Record($"SOURCES {o.Ref.Name}");
            List<SapSourcePart> parts = [new(o.Ref.Name, "main", $"{o.Ref.Uri}/source/main")];
            parts.AddRange(o.Dependencies.Select(s.Get).OfType<SimulatedObject>().Select(d => new SapSourcePart(d.Ref.Name, "main", $"{d.Ref.Uri}/source/main")));
            return Result.Success<IReadOnlyList<SapSourcePart>>(parts);
        });

    public Task<Result<string>> GetSourceAsync(SapConnection connection, string sourceUri, bool activeVersion, CancellationToken ct) =>
        With(connection, s =>
        {
            if (s.FindByUri(sourceUri) is not { } o)
                return Result.Failure<string>(SapErrors.ObjectNotFound(l, sourceUri));
            s.Record($"READ {o.Ref.Name}");
            return s.ShouldFail("READ", o.Ref.Name)
                ? Result.Failure<string>(SapErrors.Upstream(l, "source (simulado)"))
                : Result.Success(activeVersion ? o.ActiveSource : o.Source);
        });

    public Task<Result<IReadOnlyList<SapObjectRef>>> GetStructureAsync(SapConnection connection, string objectUri, CancellationToken ct) =>
        With(connection, s => s.FindByUri(objectUri) is { } o
            ? Result.Success<IReadOnlyList<SapObjectRef>>([.. o.Dependencies.Select(s.Get).OfType<SimulatedObject>().Select(d => d.Ref)])
            : Result.Failure<IReadOnlyList<SapObjectRef>>(SapErrors.ObjectNotFound(l, objectUri)));

    public Task<Result<IReadOnlyList<SyntaxMessage>>> CheckSyntaxAsync(SapConnection connection, string objectUri, CancellationToken ct) =>
        With(connection, s =>
        {
            if (s.FindByUri(objectUri) is not { } o)
                return Result.Failure<IReadOnlyList<SyntaxMessage>>(SapErrors.ObjectNotFound(l, objectUri));
            s.Record($"CHECK {o.Ref.Name}");
            return Result.Success(s.SyntaxErrorsOf(o, o.ActiveSource));
        });

    public Task<Result<IReadOnlyList<SyntaxMessage>>> CheckSourceAsync(SapConnection connection, string objectUri, string source, string? contextUri,
        CancellationToken ct) =>
        With(connection, s => s.FindByUri(objectUri) is { } o
            ? Result.Success(s.SyntaxErrorsOf(o, source))
            : Result.Failure<IReadOnlyList<SyntaxMessage>>(SapErrors.ObjectNotFound(l, objectUri)));

    // ---------------------------------------------------------------- ISapSql

    public Task<Result<SqlResult>> SelectAsync(SapConnection connection, string sql, int maxRows, CancellationToken ct) =>
        With(connection, s =>
        {
            if (!Adt.AdtSql.IsSelect(sql))
                return Result.Failure<SqlResult>(SapErrors.OnlySelect(l));
            s.Record($"SQL {sql.Trim()}");
            return s.Select(sql, connection.Client, maxRows) is { } result
                ? Result.Success(result)
                : Result.Failure<SqlResult>(SapErrors.SqlFailed(l, "SQL não suportado pelo simulador"));
        });

    public async Task<Result<SqlResult>> SelectInAsync(SapConnection connection, string sql, IReadOnlyCollection<string> values, int blockSize,
        CancellationToken ct)
    {
        List<string> columns = [];
        var rows = new List<IReadOnlyDictionary<string, string>>();
        foreach (var list in Adt.AdtSql.InLists(values, Math.Max(1, blockSize)))
        {
            var part = await SelectAsync(connection, sql.Replace("{IN}", list, StringComparison.Ordinal), 100_000, ct);
            if (part.IsFailure)
                return part;
            if (columns.Count == 0)
                columns = [.. part.Value.Columns];
            rows.AddRange(part.Value.Rows);
        }
        return new SqlResult(columns, rows);
    }

    // ---------------------------------------------------------------- ISapWriter

    public Task<Result<WriteGuardInfo>> EnsureWriteAllowedAsync(SapConnection connection, CancellationToken ct) =>
        With(connection, s => GuardWrite(s, connection));

    /// <summary>Trava de escrita para os métodos de escrita dos módulos (chamar dentro do <c>With</c>).</summary>
    internal Result<WriteGuardInfo> GuardWrite(SimulatedSapState s, SapConnection connection)
    {
        var guard = SapWriteGuard.Evaluate(connection, s.ClientCategory, l);
        if (guard.IsFailure)
            s.Record($"WRITE_BLOCKED {guard.Error.Code}");
        return guard;
    }

    public Task<Result<SourceWriteOutcome>> WriteSourceAsync(SapConnection connection, SourceWriteRequest r, CancellationToken ct) =>
        With(connection, s =>
        {
            var guard = GuardWrite(s, connection);
            if (guard.IsFailure)
                return Result.Failure<SourceWriteOutcome>(guard.Error);
            if (s.FindByUri(r.SourceUri) is not { } o)
                return Result.Success(Outcome(SourceWriteOutcome.GenericError, $"objeto {r.ObjectName} não existe no simulador"));
            s.Record($"LOCK {o.Ref.Name}");
            if (o.LockedBy is not null && o.LockedBy != SimulatedSapState.SimulatorUser)
                return Result.Success(Outcome(SourceWriteOutcome.LockError, $"{o.Ref.Name} bloqueado por {o.LockedBy}"));
            o.LockedBy = SimulatedSapState.SimulatorUser;
            string unlock;
            try
            {
                if (o.OpenRequest is not null && !string.Equals(o.OpenRequest, r.TransportRequest, StringComparison.OrdinalIgnoreCase))
                    return Result.Success(Outcome(SourceWriteOutcome.RequestError, $"objeto já está na request {o.OpenRequest} (sessão usa {r.TransportRequest})"));
                if (s.ShouldFail("PUT", o.Ref.Name))
                {
                    s.Record($"PUT {o.Ref.Name} FAILED");
                    return Result.Success(Outcome(SourceWriteOutcome.WriteError, "falha simulada", "nada foi gravado; a versão ativa continua a anterior"));
                }
                o.Source = r.Source;
                o.Inactive = true;
                o.OpenRequest = r.TransportRequest;
                s.Record($"PUT {o.Ref.Name} {r.TransportRequest}");
            }
            finally
            {
                o.LockedBy = null;
                s.Record($"UNLOCK {o.Ref.Name}");
                unlock = "200";
            }
            var activation = Activate(s, o, r.ActivationContextUri);
            var messages = activation.Messages.Select(m => $"{m.Severity}: {m.Text}").ToList();
            return activation.Error
                ? Result.Success(new SourceWriteOutcome(r.Rollback ? SourceWriteOutcome.RollbackActivationError : SourceWriteOutcome.ActivationError, null,
                    "fonte gravado como versão INATIVA; a versão ativa continua a anterior", unlock, r.ActivationContextUri, messages))
                : Result.Success(new SourceWriteOutcome(r.Rollback ? SourceWriteOutcome.RollbackActivated : SourceWriteOutcome.Activated, null, null, unlock,
                    r.ActivationContextUri, messages));
        });

    public Task<Result<ActivationResult>> ActivateAsync(SapConnection connection, string objectUri, string objectName, string? contextUri, CancellationToken ct) =>
        With(connection, s =>
        {
            var guard = GuardWrite(s, connection);
            if (guard.IsFailure)
                return Result.Failure<ActivationResult>(guard.Error);
            return s.FindByUri(objectUri) is { } o
                ? Result.Success(Activate(s, o, contextUri))
                : Result.Failure<ActivationResult>(SapErrors.ObjectNotFound(l, objectName));
        });

    /// <summary>Ativação: sem erro de sintaxe, a versão inativa vira a ativa.</summary>
    private static ActivationResult Activate(SimulatedSapState s, SimulatedObject o, string? contextUri)
    {
        s.Record(contextUri is null ? $"ACTIVATE {o.Ref.Name}" : $"ACTIVATE {o.Ref.Name} context={contextUri}");
        if (s.ShouldFail("ACTIVATE", o.Ref.Name))
            return new ActivationResult(200, true, [new SyntaxMessage("E", "Ativação recusada (falha simulada).", null, null)]);
        var errors = s.SyntaxErrorsOf(o, o.Source);
        if (errors.Count == 0)
        {
            o.Inactive = false;
            o.ActiveSource = o.Source;
        }
        return new ActivationResult(200, errors.Count > 0, errors);
    }

    private static SourceWriteOutcome Outcome(string result, string? error, string? observation = null) => new(result, error, observation, null, null, []);

    /// <summary>Executa sobre o estado do cenário da conexão, sob o lock do estado.</summary>
    internal Task<Result<T>> With<T>(SapConnection connection, Func<SimulatedSapState, Result<T>> action)
    {
        var state = registry.Get(connection.BaseUrl);
        return Task.FromResult(state is null
            ? Result.Failure<T>(SapErrors.ScenarioNotFound(l, connection.BaseUrl))
            : state.WithLock(() => action(state)));
    }
}
