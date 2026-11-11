using FluentValidation;

namespace Mignow.Atc.Application.Abstractions;

internal static class ValidationRunner
{
    public static async Task ValidateAsync<T>(T request, IEnumerable<IValidator<T>> validators, CancellationToken ct)
    {
        var list = validators as IValidator<T>[] ?? validators.ToArray();
        if (list.Length == 0)
            return;
        var context = new ValidationContext<T>(request);
        var failures = (await Task.WhenAll(list.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors).Where(f => f is not null).ToArray();
        if (failures.Length != 0)
            throw new ValidationException(failures); // → 400 no middleware
    }
}
