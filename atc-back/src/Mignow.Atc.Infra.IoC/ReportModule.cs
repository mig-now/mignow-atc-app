using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Infra.External.Report;

namespace Mignow.Atc.Infra.IoC;

public static class ReportModule
{
    public static IServiceCollection AddReportModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddSingleton<IReportTemplate, EmbeddedReportTemplate>();
        services.AddSingleton<IMignowRulesSource, FileMignowRulesSource>();
        return services;
    }
}
