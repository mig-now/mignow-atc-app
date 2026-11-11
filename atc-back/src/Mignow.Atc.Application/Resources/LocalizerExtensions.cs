using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Resources;

public static class LocalizerExtensions
{
    /// <summary>Texto localizado da chave; sem tradução cadastrada, devolve a própria chave.</summary>
    public static string Text(this IStringLocalizer<Errors> l, string code, params object[] args) =>
        args.Length == 0 ? l[code].Value : l[code, args].Value;

    public static Error NotFound(this IStringLocalizer<Errors> l, string code, params object[] args) => Error.NotFound(code, l.Text(code, args));

    public static Error Conflict(this IStringLocalizer<Errors> l, string code, params object[] args) => Error.Conflict(code, l.Text(code, args));

    public static Error Validation(this IStringLocalizer<Errors> l, string code, params object[] args) => Error.Validation(code, l.Text(code, args));

    public static Error Forbidden(this IStringLocalizer<Errors> l, string code, params object[] args) => Error.Forbidden(code, l.Text(code, args));

    public static Error Upstream(this IStringLocalizer<Errors> l, string code, params object[] args) => Error.Upstream(code, l.Text(code, args));

    public static Error Failure(this IStringLocalizer<Errors> l, string code, params object[] args) => Error.Failure(code, l.Text(code, args));
}
