using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Infra.IoC;

/// <summary>
/// Registro do módulo Remediation (proposta e aplicação): serviços que a varredura não cobre (handlers, validators, pipelines, passos, portas SAP e
/// respostas Fake já são automáticos). Só este arquivo é editado pelo módulo.
/// </summary>
public static class RemediationApplyModule
{
    public static IServiceCollection AddRemediationApplyModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddScoped<ProposeEngine>();
        services.AddScoped<ApplyEngine>();
        services.AddScoped<ReviewEngine>();
        services.AddScoped<Mignow.Atc.Application.Modules.Remediation.IDecisionHook, KbContributionHook>();
        return services;
    }
}
