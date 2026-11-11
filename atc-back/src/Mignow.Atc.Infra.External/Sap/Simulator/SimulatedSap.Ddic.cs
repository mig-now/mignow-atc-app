using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Ddic;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Módulo Ddic: simulador das portas de <c>Application/Ports/Sap/Ddic/</c> (as mesmas do <c>AdtSap.Ddic.cs</c>). Mesmas regras do ADT:
/// trava de escrita, lock de outro usuário, objeto em outra request, falha de gravação/ativação injetáveis (<c>InjectFailure</c> com
/// as operações <c>CREATE</c>, <c>PUT</c> e <c>ACTIVATE</c>) e a criação deixa o objeto inativo até a ativação.
/// </summary>
public sealed partial class SimulatedSap : IDdicReader, IDdicWriter
{
    private static string KindCode(DdicKind kind) => kind switch
    {
        DdicKind.Structure => "STRU",
        DdicKind.Table => "TABL",
        DdicKind.Function => "FUNC",
        _ => "DTEL",
    };

    public Task<Result<DdicDefinition>> ReadDefinitionAsync(SapConnection connection, string name, DdicKind? kind, CancellationToken ct) =>
        With(connection, s =>
        {
            var n = name.Trim().ToUpperInvariant();
            foreach (var k in kind is null ? new[] { DdicKind.Structure, DdicKind.Table, DdicKind.Function, DdicKind.DataElement } : [kind.Value])
                if (s.GetDdic(KindCode(k), n) is { } o)
                {
                    s.Record($"DDIC_READ {n}");
                    return Result.Success(new DdicDefinition(n, k, o.ActiveSource));
                }
            return Result.Success(new DdicDefinition(n, null, null));
        });

    public Task<Result<DdicObjectState>> ReadObjectAsync(SapConnection connection, DdicKind kind, string name, CancellationToken ct) =>
        With(connection, s => s.GetDdic(KindCode(kind), name) is { } o
            ? Result.Success(new DdicObjectState(true, o.ActiveSource))
            : Result.Success(new DdicObjectState(false, null)));

    public Task<Result<DdicNameValidation>> ValidateNameAsync(SapConnection connection, DdicKind kind, string name, string package, string description,
        CancellationToken ct) =>
        With(connection, s =>
        {
            var guard = GuardWrite(s, connection);
            if (guard.IsFailure)
                return Result.Failure<DdicNameValidation>(guard.Error);
            if (s.GetDdic(KindCode(kind), name) is not null)
                return Result.Success(new DdicNameValidation(false, "ERRO E", $"Objeto {name.ToUpperInvariant()} já existe"));
            return string.IsNullOrWhiteSpace(package) || string.IsNullOrWhiteSpace(description)
                ? Result.Success(new DdicNameValidation(false, "ERRO E", "Pacote e descrição são obrigatórios"))
                : Result.Success(new DdicNameValidation(true, "OK", string.Empty));
        });

    public Task<Result<DdicWriteOutcome>> WriteAsync(SapConnection connection, DdicWriteRequest r, CancellationToken ct) =>
        With(connection, s =>
        {
            var guard = GuardWrite(s, connection);
            if (guard.IsFailure)
                return Result.Failure<DdicWriteOutcome>(guard.Error);
            var kind = KindCode(r.Kind);
            var name = r.Name.ToUpperInvariant();
            if (r.Create)
            {
                if (s.ShouldFail("CREATE", name) || s.GetDdic(kind, name) is not null)
                    return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.CreationError, "criação recusada (falha simulada ou objeto já existe)", null, []));
                s.AddDdic(kind, name, r.Package ?? "ZPACK");
                s.Record($"DDIC_CREATE {name}");
            }
            if (s.GetDdic(kind, name) is not { } o)
                return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.GenericError, $"objeto {name} não existe no simulador", null, []));
            s.Record($"DDIC_LOCK {name}");
            if (o.LockedBy is not null && o.LockedBy != SimulatedSapState.SimulatorUser)
                return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.LockError, $"{name} bloqueado por {o.LockedBy}", null, []));
            o.LockedBy = SimulatedSapState.SimulatorUser;
            try
            {
                if (o.OpenRequest is not null && !string.Equals(o.OpenRequest, r.TransportRequest, StringComparison.OrdinalIgnoreCase))
                    return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.RequestError,
                        $"objeto já está na request {o.OpenRequest} (sessão usa {r.TransportRequest})", null, []));
                if (s.ShouldFail("PUT", name))
                    return Result.Success(new DdicWriteOutcome(DdicWriteOutcome.WriteError, "falha simulada",
                        r.Create ? "objeto criado vazio/inativo; corrija o DDL e rode de novo" : "nada foi gravado; a versão ativa continua a anterior", []));
                o.Source = r.Source;
                o.Inactive = true;
                o.OpenRequest = r.TransportRequest;
                s.Record($"DDIC_PUT {name} {r.TransportRequest}");
            }
            finally
            {
                o.LockedBy = null;
                s.Record($"DDIC_UNLOCK {name}");
            }
            s.Record($"DDIC_ACTIVATE {name}");
            if (s.ShouldFail("ACTIVATE", name))
                return Result.Success(new DdicWriteOutcome(r.Rollback ? DdicWriteOutcome.RollbackActivationError : DdicWriteOutcome.ActivationError, null,
                    "DDL gravado como versão INATIVA; a versão ativa continua a anterior", ["E: Ativação recusada (falha simulada)."]));
            o.Inactive = false;
            o.ActiveSource = o.Source;
            return Result.Success(new DdicWriteOutcome(r.Rollback ? DdicWriteOutcome.RollbackActivated : DdicWriteOutcome.Activated, null, null, []));
        });
}
