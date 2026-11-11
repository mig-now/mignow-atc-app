using FluentValidation;

namespace Mignow.Atc.Application.Abstractions;

public sealed class ValidationCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner, IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    public async Task<TResult> Handle(TCommand command, CancellationToken ct)
    {
        await ValidationRunner.ValidateAsync(command, validators, ct);
        return await inner.Handle(command, ct);
    }
}
