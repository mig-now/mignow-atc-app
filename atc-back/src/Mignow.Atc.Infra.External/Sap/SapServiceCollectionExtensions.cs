using System.Net.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Infra.External.Sap.Adt;
using Mignow.Atc.Infra.External.Sap.Simulator;
using Polly;
using Polly.Timeout;

namespace Mignow.Atc.Infra.External.Sap;

public static class SapServiceCollectionExtensions
{
    /// <summary>Namespace das portas SAP: toda interface do <see cref="AdtSap"/> dentro dele é registrada com o roteador.</summary>
    public const string PortsNamespace = "Mignow.Atc.Application.Ports.Sap";

    public static IServiceCollection AddSapInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AdtOptions>().Bind(configuration.GetSection(AdtOptions.Section));
        services.AddOptions<SapConcurrencyOptions>().Bind(configuration.GetSection(SapConcurrencyOptions.Section));
        services.AddOptions<SapSimulatorOptions>().Bind(configuration.GetSection(SapSimulatorOptions.Section));
        services.AddHttpClient(AdtSessionFactory.ClientName)
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var adt = sp.GetRequiredService<IOptions<AdtOptions>>().Value;
                return new SocketsHttpHandler
                {
                    UseCookies = false,
                    AllowAutoRedirect = false,
                    ConnectTimeout = TimeSpan.FromSeconds(adt.ConnectTimeoutSeconds),
                    PooledConnectionIdleTimeout = TimeSpan.FromSeconds(adt.IdleConnectionSeconds),
                    // Certificado fixado por ambiente; sem ele, a confiança do sistema operacional (ou TOFU no teste de conexão).
                    SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = CertificatePinning.ValidateConnection },
                };
            })
            .AddResilienceHandler("adt", (pipeline, context) =>
            {
                var adt = context.ServiceProvider.GetRequiredService<IOptions<AdtOptions>>().Value;
                var retry = new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromMilliseconds(adt.RetryDelayMilliseconds),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                };
                retry.DisableForUnsafeHttpMethods(); // novas tentativas só em leitura (GET), nunca no PUT/POST
                // Timeout por chamada: 180 s; a chamada marcada com AdtSession.LongTimeout (classrun do lote da varredura) usa LongTimeoutSeconds.
                pipeline.AddRetry(retry).AddTimeout(new TimeoutStrategyOptions
                {
                    TimeoutGenerator = args => ValueTask.FromResult(TimeSpan.FromSeconds(
                        args.Context.GetRequestMessage() is { } request && request.Options.TryGetValue(AdtSession.LongTimeout, out var isLong) && isLong
                            ? adt.LongTimeoutSeconds : adt.TimeoutSeconds)),
                });
            });
        services.AddSingleton<AdtSessionFactory>();
        services.AddSingleton<AdtThrottle>();
        services.AddSingleton<AdtSap>();
        services.AddSingleton<SimulatedSapRegistry>();
        services.AddSingleton<SimulatedSap>();
        foreach (var port in SapPorts())
            services.AddSingleton(port, sp => SapPortProxy.Create(port, sp));
        return services;
    }

    /// <summary>Portas SAP implementadas pelo adapter ADT (as mesmas que o simulador precisa implementar).</summary>
    public static IReadOnlyList<Type> SapPorts() =>
        [.. typeof(AdtSap).GetInterfaces().Where(i => i.Namespace?.StartsWith(PortsNamespace, StringComparison.Ordinal) == true)];
}
