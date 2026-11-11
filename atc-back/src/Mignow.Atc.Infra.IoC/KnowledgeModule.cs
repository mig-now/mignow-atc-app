using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Infra.External.Knowledge;

namespace Mignow.Atc.Infra.IoC;

/// <summary>
/// Registro do módulo Knowledge (KB markdown). Singleton: o cache por arquivo e a trava de escrita são do processo.
/// A pasta vem de <c>GeneralSettings.KbPath</c> (ver <see cref="SettingsKbPathResolver"/>).
/// </summary>
public static class KnowledgeModule
{
    public static IServiceCollection AddKnowledgeModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddSingleton<IKnowledgeBase, MarkdownKnowledgeBase>();
        return services;
    }
}
