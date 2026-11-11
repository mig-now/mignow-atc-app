using FluentAssertions;
using Mignow.Atc.Application;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Infra.External.Sap;
using Mignow.Atc.Infra.External.Sap.Adt;
using Mignow.Atc.Infra.External.Sap.Simulator;
using NetArchTest.Rules;

namespace Mignow.Atc.Tests.Architecture;

/// <summary>Regras de dependência do blueprint (§3) + regras do motor e das portas SAP.</summary>
public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_nao_depende_de_ninguem() =>
        Types.InAssembly(typeof(EntityBase).Assembly).ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "FluentValidation", "Mignow.Atc.Application", "Mignow.Atc.Infra", "Anthropic",
                "Microsoft.AspNetCore")
            .GetResult().IsSuccessful.Should().BeTrue();

    [Fact]
    public void Application_nao_depende_de_infra_nem_de_http() =>
        Types.InAssembly(typeof(IApplicationMarker).Assembly).ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "Microsoft.AspNetCore", "Mignow.Atc.Infra", "Mignow.Atc.Api", "Anthropic",
                "System.Net.Http")
            .GetResult().IsSuccessful.Should().BeTrue();

    [Fact]
    public void Controllers_nao_tocam_infra() =>
        Types.InAssembly(typeof(Mignow.Atc.Api.Controllers.ApiControllerBase).Assembly)
            .That().ResideInNamespace("Mignow.Atc.Api.Controllers")
            .ShouldNot().HaveDependencyOn("Mignow.Atc.Infra")
            .GetResult().IsSuccessful.Should().BeTrue();

    [Fact]
    public void Infra_database_nao_depende_de_infra_external() =>
        Types.InAssembly(typeof(Mignow.Atc.Infra.Database.Context.AtcDbContext).Assembly).ShouldNot()
            .HaveDependencyOnAny("Mignow.Atc.Infra.External", "Mignow.Atc.Api")
            .GetResult().IsSuccessful.Should().BeTrue();

    [Fact]
    public void Handlers_sao_sealed() =>
        Types.InAssembly(typeof(IApplicationMarker).Assembly)
            .That().HaveNameEndingWith("Handler").And().AreClasses()
            .Should().BeSealed()
            .GetResult().IsSuccessful.Should().BeTrue();

    [Fact]
    public void Handlers_nao_chamam_SaveChanges() =>
        Types.InAssembly(typeof(IApplicationMarker).Assembly)
            .That().HaveNameEndingWith("Handler")
            .ShouldNot().HaveDependencyOn("Mignow.Atc.Application.Abstractions.IUnitOfWork")
            .GetResult().IsSuccessful.Should().BeTrue();

    /// <summary>Cada porta SAP precisa das duas implementações (o roteador escolhe por conexão).</summary>
    [Fact]
    public void Simulador_implementa_todas_as_portas_do_adapter_adt()
    {
        var ports = SapServiceCollectionExtensions.SapPorts();
        ports.Should().Contain([typeof(Mignow.Atc.Application.Ports.Sap.ISapReader), typeof(Mignow.Atc.Application.Ports.Sap.ISapSql),
            typeof(Mignow.Atc.Application.Ports.Sap.ISapWriter)]);
        typeof(SimulatedSap).GetInterfaces().Where(i => i.Namespace?.StartsWith(SapServiceCollectionExtensions.PortsNamespace, StringComparison.Ordinal) == true)
            .Should().BeEquivalentTo(ports);
        typeof(AdtSap).Should().NotBeNull();
    }

    /// <summary>Toda porta SAP devolve Task&lt;Result…&gt; e recebe a SapConnection (requisito do roteador).</summary>
    [Fact]
    public void Portas_sap_recebem_a_conexao_e_devolvem_result()
    {
        foreach (var method in SapServiceCollectionExtensions.SapPorts().SelectMany(p => p.GetMethods()))
        {
            method.GetParameters().Should().Contain(p => p.ParameterType == typeof(Mignow.Atc.Application.Ports.Sap.SapConnection), method.Name);
            method.ReturnType.IsGenericType.Should().BeTrue(method.Name);
            method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(Task<>), method.Name);
            var inner = method.ReturnType.GetGenericArguments()[0];
            (inner == typeof(Mignow.Atc.Application.Common.Result) || inner.IsGenericType && inner.GetGenericTypeDefinition() == typeof(Mignow.Atc.Application.Common.Result<>))
                .Should().BeTrue(method.Name);
        }
    }

    [Fact]
    public void Pipelines_um_por_tipo_e_passos_sao_sealed()
    {
        var pipelines = typeof(IApplicationMarker).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IRunPipeline).IsAssignableFrom(t))
            .Select(t => (IRunPipeline)Activator.CreateInstance(t)!).ToList();
        var catalog = new RunPipelineCatalog(pipelines);
        catalog.For(RunKind.ConnectionTest).Should().NotBeNull();
        Types.InAssembly(typeof(IApplicationMarker).Assembly).That().ImplementInterface(typeof(IRunStep)).Should().BeSealed()
            .GetResult().IsSuccessful.Should().BeTrue();
    }
}
