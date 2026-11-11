namespace Mignow.Atc.Application.Ports.Execution;

/// <summary>Fila no Postgres (<c>FOR UPDATE SKIP LOCKED</c>): várias instâncias não pegam a mesma execução.</summary>
public interface IRunQueue
{
    /// <summary>Reserva a próxima execução da fila (muda para Running) e devolve o id; nulo = fila vazia.</summary>
    Task<int?> ClaimNextAsync(CancellationToken ct);
}
