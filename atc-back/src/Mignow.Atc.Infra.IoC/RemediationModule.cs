using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Infra.IoC;

/// <summary>
/// Registro do módulo Remediation (parte 1): handlers, validators, pipelines e passos são automáticos; aqui só o que a varredura não cobre
/// (regras por produto MIGNOW e a criação de sessão, usada também pelo módulo Scan). Só este arquivo é editado pelo módulo.
/// </summary>
public static class RemediationModule
{
    public static IServiceCollection AddRemediationModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddSingleton<IProductRules, DefaultProductRules>();
        services.AddScoped<SessionCreator>();
        return services;
    }
}
