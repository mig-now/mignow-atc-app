using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mignow.Atc.Infra.IoC;

/// <summary>
/// Registro do módulo Extraction: serviços que a varredura não cobre (handlers, validators, pipelines, passos, portas SAP e
/// respostas Fake já são automáticos). Só este arquivo é editado pelo módulo.
/// </summary>
public static class ExtractionModule
{
    public static IServiceCollection AddExtractionModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        return services;
    }
}
