using FluentValidation;

namespace Mignow.Atc.Application.Abstractions;

public sealed class ValidationQueryHandlerDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner, IEnumerable<IValidator<TQuery>> validators)
    : IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    public async Task<TResult> Handle(TQuery query, CancellationToken ct)
    {
        await ValidationRunner.ValidateAsync(query, validators, ct);
        return await inner.Handle(query, ct);
    }
}
