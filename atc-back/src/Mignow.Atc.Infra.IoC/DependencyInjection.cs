using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Infra.Database;
using Mignow.Atc.Infra.External;

namespace Mignow.Atc.Infra.IoC;

public static class DependencyInjection
{
    /// <summary>
    /// Composition root. Handlers e validators por convenção (assembly da Application), decorados com Scrutor — a ORDEM
    /// importa: o último Decorate é o mais externo. Command: Validação → Transação → Handler. Query: Validação → Handler.
    /// Pipelines (<see cref="IRunPipeline"/>) e passos (<see cref="IRunStep"/>) também por varredura. Cada módulo tem o
    /// próprio arquivo de registro (<c>*Module.cs</c>), já chamado aqui.
    /// </summary>
    public static IServiceCollection AddDiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var application = typeof(IApplicationMarker).Assembly;
        services.AddLocalization();
        services.AddSingleton<IStringLocalizer<Errors>, ErrorText>();
        services.AddValidatorsFromAssembly(application, includeInternalTypes: true);
        services.Scan(scan => scan.FromAssemblies(application)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false).AsImplementedInterfaces().WithScopedLifetime());
        services.Decorate(typeof(ICommandHandler<,>), typeof(TransactionCommandHandlerDecorator<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationQueryHandlerDecorator<,>));

        services.AddScoped<SapConnections>();
        services.AddEngine(application);

        services.AddDatabaseInfrastructure(configuration);
        services.AddExternalInfrastructure(configuration);

        services.AddExtractionModule(configuration);
        services.AddRemediationModule(configuration);
        services.AddRemediationApplyModule(configuration);
        services.AddExemptionModule(configuration);
        services.AddScanModule(configuration);
        services.AddReportModule(configuration);
        services.AddKnowledgeModule(configuration);
        return services;
    }

    /// <summary>Motor: catálogo de pipelines, processador, recuperação, enfileirador; pipelines e passos por varredura.</summary>
    private static IServiceCollection AddEngine(this IServiceCollection services, System.Reflection.Assembly application)
    {
        services.AddOptions<RunWorkerOptions>().BindConfiguration(RunWorkerOptions.Section);
        services.Scan(scan => scan.FromAssemblies(application)
            .AddClasses(c => c.AssignableTo<IRunPipeline>(), publicOnly: false).As<IRunPipeline>().WithSingletonLifetime()
            .AddClasses(c => c.AssignableTo<IRunStep>(), publicOnly: false).AsSelf().WithScopedLifetime());
        services.AddSingleton<RunPipelineCatalog>();
        services.AddSingleton<RunProcessor>();
        services.AddSingleton<RunRecovery>();
        services.AddScoped<RunStarter>();
        return services;
    }

    /// <summary>Worker da fila (hospedado na API).</summary>
    public static IServiceCollection AddRunWorker(this IServiceCollection services) => services.AddHostedService<RunWorker>();
}
