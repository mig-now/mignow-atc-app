using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Abstractions;

public sealed class TransactionCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner, IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    public async Task<TResult> Handle(TCommand command, CancellationToken ct)
    {
        var response = await inner.Handle(command, ct);
        if (response is Result { IsFailure: true })
            return response; // falha de negócio → não commita
        await unitOfWork.SaveChangesAsync(ct);
        return response;
    }
}
