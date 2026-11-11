using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Engine.Steps.Scan;
using Mignow.Atc.Application.Ports.Scan;
using Mignow.Atc.Infra.External.Scan;

namespace Mignow.Atc.Infra.IoC;

/// <summary>
/// Registro do módulo Scan: handlers, validators, pipelines, passos e portas SAP (<c>IMignowScanner</c>) são automáticos; aqui só a
/// origem das regras (arquivo) e o executor dos lotes (<see cref="ScanLotRunner"/>, não é passo nem handler).
/// </summary>
public static class ScanModule
{
    public static IServiceCollection AddScanModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddSingleton<IMignowRulesSource, FileMignowRulesSource>();
        services.AddScoped<ScanLotRunner>();
        return services;
    }
}
