using Microsoft.Extensions.Localization;

namespace Mignow.Atc.Application.Resources;

/// <summary>
/// <c>IStringLocalizer&lt;Errors&gt;</c> composto: procura o código no recurso do núcleo e depois nos de cada módulo
/// (todo <see cref="IErrorResource"/> do assembly). Assim handlers, passos e o middleware usam um localizador só e cada
/// módulo adiciona chaves apenas no próprio <c>.resx</c>. Sem tradução em nenhum, devolve a própria chave.
/// </summary>
public sealed class ErrorText : IStringLocalizer<Errors>
{
    private static readonly Type[] Markers =
    [
        typeof(Errors),
        .. typeof(Errors).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IErrorResource).IsAssignableFrom(t) && t != typeof(Errors))
            .OrderBy(t => t.Name, StringComparer.Ordinal),
    ];

    private readonly IReadOnlyList<IStringLocalizer> _localizers;

    public ErrorText(IStringLocalizerFactory factory) => _localizers = [.. Markers.Select(factory.Create)];

    public static IReadOnlyList<Type> ResourceTypes => Markers;

    public LocalizedString this[string name] => Find(name, l => l[name]);

    public LocalizedString this[string name, params object[] arguments] => Find(name, l => l[name, arguments]);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _localizers.SelectMany(l => l.GetAllStrings(includeParentCultures)).DistinctBy(s => s.Name);

    private LocalizedString Find(string name, Func<IStringLocalizer, LocalizedString> get)
    {
        foreach (var localizer in _localizers)
        {
            var value = get(localizer);
            if (!value.ResourceNotFound)
                return value;
        }
        return new LocalizedString(name, name, resourceNotFound: true);
    }
}
