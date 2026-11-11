using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Tests.Integration;

/// <summary>Portas SAP pelo roteador (simulador): trava de escrita (produção e T000=P bloqueiam), gravação e SQL.</summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class SapPortsTests(AtcApiFactory api)
{
    private static SapConnection Conn(string host, bool production = false) => new(Guid.NewGuid(), host, "100", "MIGATC", string.Empty, "EN", null, production);

    private ISapWriter Writer => api.Services.GetRequiredService<ISapWriter>();

    private static SourceWriteRequest Write(SimulatedSapState state, string name, string source)
    {
        var uri = state.WithLock(() => state.Get(name)!.Ref.Uri);
        return new SourceWriteRequest(uri, name, uri + "/source/main", source, "SBXK900120", null);
    }

    [Fact]
    public async Task Cadastro_de_producao_bloqueia_a_escrita()
    {
        var (host, state) = api.NewSimulatedSap();
        var result = await Writer.WriteSourceAsync(Conn(host, production: true), Write(state, "ZMIG_CLEAN", "REPORT zmig_clean.\n"), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sap.WriteBlockedProduction");
        state.Journal.Should().NotContain(j => j.StartsWith("PUT", StringComparison.Ordinal) || j.StartsWith("LOCK", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Mandante_de_producao_na_t000_bloqueia_a_escrita()
    {
        var (host, state) = api.NewSimulatedSap(s => s with { ClientCategory = "P" });
        var result = await Writer.WriteSourceAsync(Conn(host), Write(state, "ZMIG_CLEAN", "REPORT zmig_clean.\n"), CancellationToken.None);
        result.Error.Code.Should().Be("Sap.WriteBlockedProductionClient");
        (await Writer.ActivateAsync(Conn(host), "/sap/bc/adt/programs/programs/zmig_clean", "ZMIG_CLEAN", null, CancellationToken.None))
            .Error.Code.Should().Be("Sap.WriteBlockedProductionClient");
    }

    [Fact]
    public async Task Categoria_ilegivel_bloqueia_a_escrita()
    {
        var (host, _) = api.NewSimulatedSap(s => s with { ClientCategory = null });
        (await Writer.EnsureWriteAllowedAsync(Conn(host), CancellationToken.None)).Error.Code.Should().Be("Sap.WriteBlockedCategoryUnknown");
    }

    [Fact]
    public async Task Fora_de_producao_grava_com_lock_put_unlock_e_ativa()
    {
        var (host, state) = api.NewSimulatedSap();
        const string source = "REPORT zmig_clean.\nDATA lv_x TYPE i VALUE 2.\nWRITE: / lv_x.\n";
        var result = await Writer.WriteSourceAsync(Conn(host), Write(state, "ZMIG_CLEAN", source), CancellationToken.None);
        result.Value.Result.Should().Be(SourceWriteOutcome.Activated);
        state.Journal.Where(j => !j.StartsWith("SQL", StringComparison.Ordinal)).Should().Equal(
            "LOCK ZMIG_CLEAN", "PUT ZMIG_CLEAN SBXK900120", "UNLOCK ZMIG_CLEAN", "ACTIVATE ZMIG_CLEAN");
        var reader = api.Services.GetRequiredService<ISapReader>();
        (await reader.GetSourceAsync(Conn(host), "/sap/bc/adt/programs/programs/zmig_clean/source/main", true, CancellationToken.None)).Value
            .Should().Be(source);
    }

    [Fact]
    public async Task Erro_de_ativacao_deixa_o_fonte_inativo_e_a_versao_ativa_anterior()
    {
        var (host, state) = api.NewSimulatedSap();
        var original = state.WithLock(() => state.Get("ZMIG_CLEAN")!.ActiveSource);
        var result = await Writer.WriteSourceAsync(Conn(host), Write(state, "ZMIG_CLEAN", "REPORT zmig_clean.\nSELECT * FROM konv.\n"), CancellationToken.None);
        result.Value.Result.Should().Be(SourceWriteOutcome.ActivationError);
        state.WithLock(() => state.Get("ZMIG_CLEAN")!.ActiveSource).Should().Be(original);
        state.WithLock(() => state.Get("ZMIG_CLEAN")!.Inactive).Should().BeTrue();
    }

    [Fact]
    public async Task Sql_le_a_t000_e_recusa_o_que_nao_e_select()
    {
        var (host, _) = api.NewSimulatedSap();
        var sql = api.Services.GetRequiredService<ISapSql>();
        var t000 = await sql.SelectAsync(Conn(host), "SELECT cccategory FROM t000 WHERE mandt = '100'", 1, CancellationToken.None);
        t000.Value.Rows.Should().ContainSingle().Which["CCCATEGORY"].Should().Be("T");
        (await sql.SelectAsync(Conn(host), "DELETE FROM t000", 1, CancellationToken.None)).Error.Code.Should().Be("Sap.OnlySelect");
    }

    [Fact]
    public async Task Fontes_do_objeto_incluem_os_includes()
    {
        var (host, _) = api.NewSimulatedSap();
        var reader = api.Services.GetRequiredService<ISapReader>();
        var parts = await reader.GetObjectSourcesAsync(Conn(host), "ZMIG_MAIN_INCL", "PROG", CancellationToken.None);
        parts.Value.Select(p => p.Name).Should().Equal("ZMIG_MAIN_INCL", "ZMIG_MAIN_INCL_TOP", "ZMIG_MAIN_INCL_F01");
    }
}
