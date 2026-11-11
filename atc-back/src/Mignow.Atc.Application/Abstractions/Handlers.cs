namespace Mignow.Atc.Application.Abstractions;

/// <summary>Escrita: transacional por definição (o TransactionDecorator envolve todo ICommandHandler).</summary>
public interface ICommand<TResult>;

/// <summary>Leitura: sem transação.</summary>
public interface IQuery<TResult>;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken ct);
}
