using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Ports.Identity;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Infra.External.Ai;
using Mignow.Atc.Infra.External.Identity;
using Mignow.Atc.Infra.External.Sap;
using Mignow.Atc.Infra.External.Time;

namespace Mignow.Atc.Infra.External;

public static class ExternalServiceCollectionExtensions
{
    /// <summary>Usuário atual, relógio, SAP (ADT + simulador) e IA. A KB é registrada pelo <c>KnowledgeModule</c> (IoC).</summary>
    public static IServiceCollection AddExternalInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddOptions<DevAuthOptions>().Bind(configuration.GetSection("Auth:Dev"));
        services.AddSingleton<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSapInfrastructure(configuration);
        services.AddAiInfrastructure();
        return services;
    }
}
