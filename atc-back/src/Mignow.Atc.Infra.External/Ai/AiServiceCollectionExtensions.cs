using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Infra.External.Ai.Anthropic;
using Mignow.Atc.Infra.External.Ai.Fake;
using Mignow.Atc.Infra.External.Ai.Providers;

namespace Mignow.Atc.Infra.External.Ai;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<AiProviderResolver>();
        services.AddSingleton<AnthropicJsonClient>();
        // Sem timeout no HttpClient (o padrão de 100 s cortaria o streaming): vale o do SDK, por tentativa.
        services.AddHttpClient(AnthropicJsonClient.ClientName, c => c.Timeout = Timeout.InfiniteTimeSpan);
        services.AddSingleton<IAiGateway, AiGateway>();
        // Respostas da IA de desenvolvimento: uma classe por módulo, achada por varredura.
        services.Scan(scan => scan.FromAssemblyOf<IFakeAiResponder>()
            .AddClasses(c => c.AssignableTo<IFakeAiResponder>()).As<IFakeAiResponder>().WithSingletonLifetime());
        return services;
    }
}
