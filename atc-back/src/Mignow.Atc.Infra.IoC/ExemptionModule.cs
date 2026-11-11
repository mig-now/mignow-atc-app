using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Features.Exemptions;

namespace Mignow.Atc.Infra.IoC;

/// <summary>
/// Registro do módulo Exemption: handlers, validators, pipelines, passos e a porta SAP são automáticos; aqui só o que a
/// varredura não cobre (o enfileirador das execuções do lote).
/// </summary>
public static class ExemptionModule
{
    public static IServiceCollection AddExemptionModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddScoped<ExemptionRunStarter>();
        return services;
    }
}
