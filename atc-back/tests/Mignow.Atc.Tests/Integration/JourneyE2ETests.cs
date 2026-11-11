using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.Database.Context;
using Mignow.Atc.Tests.Knowledge;

namespace Mignow.Atc.Tests.Integration;

/// <summary>
/// Jornada inteira pela API HTTP no simulador (Postgres de teste, IA Fake, KB em pasta temporária): extração → sessão → leitura de fontes → triagem →
/// grupos → proposta → decisão → simulação/aplicação → exportação → isenção → varredura MIGNOW → relatório → histórico.
/// </summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class JourneyE2ETests(AtcApiFactory api)
{
    private HttpClient _client = null!;

    /// <summary>regras.json real da skill (somente leitura); o caminho padrão é relativo ao diretório de trabalho da API.</summary>
    private static string RulesPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, "..", "..", "..", "..", ".claude", "skills", "atc-extracao-mignow", "regras", "regras.json"));

    private async Task<JsonElement> RunAsync(Task<HttpResponseMessage> start, string expectedStatus = "Completed")
    {
        var accepted = await start;
        accepted.StatusCode.Should().Be(HttpStatusCode.Accepted, await accepted.Content.ReadAsStringAsync());
        var run = await accepted.DataAsync<JsonElement>();
        return await FinishAsync(run.GetProperty("id").GetGuid(), expectedStatus);
    }

    private async Task<JsonElement> FinishAsync(Guid runId, string expectedStatus = "Completed")
    {
        await api.ProcessQueuedAsync();
        var done = await _client.GetAsync($"/v1/runs/{runId}").DataAsync<JsonElement>();
        done.GetProperty("status").GetString().Should().Be(expectedStatus, done.ToString());
        return done;
    }

    private Task<HttpResponseMessage> Post(string url, object? body = null) => _client.PostAsJsonAsync(url, body ?? new { });

    [Fact]
    public async Task Jornada_completa_extracao_ate_relatorio()
    {
        _client = api.CreateClient();
        using var kb = new TempKb();
        kb.Write("elementos/tabelas/KONV.md", TempKb.File_("KONV", TempKb.Card("S00012", 5, "SELECT * FROM konv INTO TABLE lt.", "SELECT * FROM prcd_elements INTO TABLE lt.")));
        var settings = await _client.GetAsync("/v1/settings").DataAsync<SettingsDto>();
        var (host, state) = api.NewSimulatedSap();
        try
        {
            var put = await _client.PutAsJsonAsync("/v1/settings", settings with { KbPath = kb.Root, MignowRulesPath = RulesPath() });
            put.StatusCode.Should().Be(HttpStatusCode.OK, await put.Content.ReadAsStringAsync());
            await JourneyAsync(host, state, kb);
        }
        finally
        {
            await _client.PutAsJsonAsync("/v1/settings", settings);
        }
    }

    private async Task JourneyAsync(string host, Mignow.Atc.Infra.External.Sap.Simulator.SimulatedSapState state, TempKb kb)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var customer = await _client.PostAsJsonAsync("/v1/customers", new { name = $"Jornada {suffix}", code = suffix }).DataAsync<IdDto>();
        var system = await _client.PostAsJsonAsync("/v1/sap-systems", new
        {
            customerId = customer.Id, sid = "SBX", client = "100", host, adtPort = 0, https = false, language = "EN", isProduction = false,
            projectType = "Conversion", author = "AUT-TS", defaultRequest = "SBXK900120", exemptionApprover = "APPROVER1", username = "MIGATC",
        }).DataAsync<SapSystemDto>();

        // ---- extração
        var extractionRun = await RunAsync(Post("/v1/extractions", new { sapSystemId = system.Id, force = false }));
        var extractionId = extractionRun.GetProperty("result").GetProperty("extractionId").GetGuid();
        await AllColumnsReadersFindTheFieldsAsync(extractionId);

        // ---- sessão
        var session = await _client.PostAsJsonAsync("/v1/sessions", new { extractionId, priorities = "1,2,3", request = "SBXK900120" }).DataAsync<JsonElement>();
        var sessionId = session.GetProperty("id").GetGuid();
        session.GetProperty("import").GetProperty("items").GetInt32().Should().BeGreaterThan(0);
        (await _client.PutAsJsonAsync($"/v1/sessions/{sessionId}/auto-mode", new { enabled = true }).DataAsync<JsonElement>()).GetProperty("autoMode").GetBoolean().Should().BeTrue();
        (await _client.PutAsJsonAsync($"/v1/sessions/{sessionId}/auto-mode", new { enabled = false }).DataAsync<JsonElement>()).GetProperty("autoMode").GetBoolean().Should().BeFalse();

        // ---- leitura dos fontes começa sozinha ao carregar os itens; outra execução no ambiente: 409 com o run que bloqueia
        var sourcesRunId = session.GetProperty("sourcesRunId").GetGuid();
        var firstRun = await _client.GetAsync($"/v1/runs/{sourcesRunId}").DataAsync<JsonElement>();
        firstRun.GetProperty("kind").GetString().Should().Be("ReadSources");
        var busy = await Post($"/v1/sessions/{sessionId}/read-sources", new { });
        busy.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await busy.ProblemAsync();
        problem.GetProperty("detail").GetString().Should().Be("Run.EnvironmentBusy");
        problem.GetProperty("blockingRunId").GetGuid().Should().Be(firstRun.GetProperty("id").GetGuid());
        problem.GetProperty("blockingRunKind").GetString().Should().Be("ReadSources");
        await FinishAsync(firstRun.GetProperty("id").GetGuid());

        // ---- filtro de runs por situação
        var completed = await _client.GetAsync($"/v1/runs?sapSystemId={system.Id}&status=Completed&pageSize=100").DataAsync<PagedDto<RunDto>>();
        completed.Items.Should().NotBeEmpty().And.OnlyContain(r => r.Status == "Completed");
        (await _client.GetAsync($"/v1/runs?sapSystemId={system.Id}&status=Failed&status=Cancelled")).DataAsync<PagedDto<RunDto>>().Result.Items.Should().BeEmpty();

        // ---- triagem
        var triage = await _client.GetAsync($"/v1/sessions/{sessionId}/triage").DataAsync<JsonElement>();
        triage.ValueKind.Should().Be(JsonValueKind.Object);
        await _client.PostAsJsonAsync($"/v1/sessions/{sessionId}/triage/apply", new { rules = Array.Empty<string>() });

        // ---- grupos: o objeto referenciado é a tabela do SELECT, não "FROM"
        var groups = await _client.GetAsync($"/v1/sessions/{sessionId}/groups").DataAsync<JsonElement>();
        var refs = groups.GetProperty("groups").EnumerateArray().Select(g => g.ToString()).ToList();
        refs.Should().NotBeEmpty();
        refs.Should().NotContain(g => g.Contains("\"FROM\""), "o simulador usa a tabela real do SELECT como objeto referenciado");

        // ---- proposta (KB) + decisão + simulação + aplicação
        var items = await _client.GetAsync($"/v1/sessions/{sessionId}/items?pageSize=200").DataAsync<PagedDto<JsonElement>>();
        var konv = items.Items.Where(i => i.GetProperty("refObject").GetString() == "KONV" && i.GetProperty("objectName").GetString() == "ZMIG_MAIN_INCL_F01"
                                                 && i.GetProperty("status").GetString() == "").Take(1).ToList();
        konv.Should().NotBeEmpty();
        var ids = konv.Select(i => i.GetProperty("id").GetGuid()).ToList();
        await RunAsync(Post($"/v1/sessions/{sessionId}/propose", new { itemIds = ids, mode = "kb" }));
        var proposal = await _client.GetAsync($"/v1/sessions/{sessionId}/items/{ids[0]}/proposal").DataAsync<JsonElement>();
        proposal.GetProperty("factCandidates").ValueKind.Should().Be(JsonValueKind.Array);
        proposal.GetProperty("proposal").GetProperty("origin").GetString().Should().Be("KB");
        (await _client.PostAsJsonAsync($"/v1/sessions/{sessionId}/proposals/decision", new { itemIds = ids, decision = "ConfirmKb", obs = "KB S00012" })).StatusCode
            .Should().Be(HttpStatusCode.OK);

        await RunAsync(Post($"/v1/sessions/{sessionId}/apply-simulate", new { itemIds = ids }));
        var applied = await RunAsync(Post($"/v1/sessions/{sessionId}/apply", new { itemIds = ids }));
        var after = await _client.GetAsync($"/v1/sessions/{sessionId}/items/{ids[0]}/proposal").DataAsync<JsonElement>();
        after.GetProperty("application").GetProperty("result").GetString().Should().Be("ATIVADO", applied.ToString());

        // ---- IA (ConfirmIa) em outro item, depois exportação
        var other = items.Items.First(i => i.GetProperty("status").GetString() == "" && !ids.Contains(i.GetProperty("id").GetGuid()) && i.GetProperty("refObject").GetString() != "KONV");
        var otherId = other.GetProperty("id").GetGuid();
        await RunAsync(Post($"/v1/sessions/{sessionId}/propose", new { itemIds = new[] { otherId }, mode = "ia" }));
        var aiProposal = await _client.GetAsync($"/v1/sessions/{sessionId}/items/{otherId}/proposal").DataAsync<JsonElement>();
        if (aiProposal.GetProperty("proposal").GetProperty("action").GetString() == "corrigir")
            (await _client.PostAsJsonAsync($"/v1/sessions/{sessionId}/proposals/decision", new { itemIds = new[] { otherId }, decision = "ConfirmIa", obs = "IA" })).StatusCode
                .Should().Be(HttpStatusCode.OK);

        var export = await _client.GetAsync($"/v1/sessions/{sessionId}/export");
        export.StatusCode.Should().Be(HttpStatusCode.OK);
        (await export.Content.ReadAsStringAsync()).Should().Contain("Status Remediação");

        // ---- isenção: itens marcados como Isenção → lote → descobrir/simular/enviar/conferir
        var exempt = items.Items.Where(i => i.GetProperty("status").GetString() == "" && i.GetProperty("findingId").GetString() is { Length: > 0 }
                                            && !ids.Contains(i.GetProperty("id").GetGuid()) && i.GetProperty("id").GetGuid() != otherId).Take(2)
            .Select(i => i.GetProperty("id").GetGuid()).ToList();
        exempt.Should().NotBeEmpty();
        (await _client.PostAsJsonAsync($"/v1/sessions/{sessionId}/items/decision", new { itemIds = exempt, decision = "Exemption", obs = "Triagem R5: BAPI ja corrigida" })).StatusCode
            .Should().Be(HttpStatusCode.OK);
        var batch = await _client.PostAsJsonAsync("/v1/exemption-batches", new { sessionId, priorities = new[] { 1, 2, 3 } }).DataAsync<JsonElement>();
        var batchId = batch.GetProperty("batch").GetProperty("id").GetGuid();
        await RunAsync(Post($"/v1/exemption-batches/{batchId}/discover"));
        await RunAsync(Post($"/v1/exemption-batches/{batchId}/simulate"));
        await RunAsync(Post($"/v1/exemption-batches/{batchId}/send"));
        await RunAsync(Post($"/v1/exemption-batches/{batchId}/check"));
        var requests = await _client.GetAsync($"/v1/exemption-batches/{batchId}/requests").DataAsync<PagedDto<JsonElement>>();
        requests.Items.Should().NotBeEmpty().And.OnlyContain(r => r.GetProperty("status").GetString() == "PEDIDO");
        state.Exemptions.Should().NotBeEmpty();

        // ---- varredura MIGNOW
        var scan = await Post("/v1/scans", new { sapSystemId = system.Id, includeGenerated = false });
        scan.StatusCode.Should().Be(HttpStatusCode.Accepted, await scan.Content.ReadAsStringAsync());
        var scanStarted = await scan.DataAsync<JsonElement>();
        var scanId = scanStarted.GetProperty("scan").GetProperty("id").GetGuid();
        await FinishAsync(scanStarted.GetProperty("run").GetProperty("id").GetGuid());
        var rules = await _client.GetAsync("/v1/scans/rules").DataAsync<JsonElement>();
        var ruleIds = rules.GetProperty("rules").EnumerateArray().Select(r => r.GetProperty("id").GetString()!).ToList();
        (await _client.PutAsJsonAsync($"/v1/scans/{scanId}/scope", new { ruleIds })).StatusCode.Should().Be(HttpStatusCode.OK);
        await RunAsync(Post($"/v1/scans/{scanId}/simulate"));
        await RunAsync(Post($"/v1/scans/{scanId}/execute", new { }));
        var occurrences = await _client.GetAsync($"/v1/scans/{scanId}/occurrences?pageSize=5").DataAsync<PagedDto<JsonElement>>();
        occurrences.Total.Should().BeGreaterThan(0);
        (await _client.GetAsync($"/v1/scans/{scanId}/files/p1")).StatusCode.Should().Be(HttpStatusCode.OK);
        var imported = await _client.PostAsync($"/v1/scans/{scanId}/import", null);
        imported.StatusCode.Should().Be(HttpStatusCode.OK, await imported.Content.ReadAsStringAsync());
        {
            var mignowSession = (await imported.DataAsync<JsonElement>()).GetProperty("id").GetGuid();
            var mignowItems = await _client.GetAsync($"/v1/sessions/{mignowSession}/items?pageSize=5").DataAsync<PagedDto<JsonElement>>();
            mignowItems.Items.Should().NotBeEmpty().And.OnlyContain(i => i.GetProperty("origin").GetString() == "MIGNOW");
        }

        // ---- relatório e histórico
        var report = await _client.GetAsync($"/v1/customers/{customer.Id}/report").DataAsync<JsonElement>();
        report.ValueKind.Should().Be(JsonValueKind.Object);
        foreach (var view in new[] { "findings", "objetos" })
        {
            var html = await _client.GetAsync($"/v1/customers/{customer.Id}/report.html?view={view}");
            html.StatusCode.Should().Be(HttpStatusCode.OK);
            (await html.Content.ReadAsStringAsync()).Should().Contain("<html");
        }
        foreach (var module in new[] { "extraction", "remediation", "exemption", "scan" })
            (await _client.GetAsync($"/v1/history?customerId={customer.Id}&module={module}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.GetAsync($"/v1/sessions/{sessionId}/history")).StatusCode.Should().Be(HttpStatusCode.OK);
        _ = kb;
    }

    /// <summary>AllColumnsJson gravado pela extração = objeto nome → valor; todos os leitores acham os campos que usam.</summary>
    private async Task AllColumnsReadersFindTheFieldsAsync(Guid extractionGuid)
    {
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AtcDbContext>();
        var extraction = await db.Set<AtcExtraction>().AsNoTracking().SingleAsync(e => e.Guid == extractionGuid);
        var finding = await db.Set<AtcFinding>().AsNoTracking().Where(f => f.ExtractionId == extraction.Id && f.FindingId != null).OrderBy(f => f.Seq).FirstAsync();
        using var doc = JsonDocument.Parse(finding.AllColumnsJson);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Object, "gravador: objeto nome da coluna → valor");

        var columns = ExtractionFiles.ParseRow(finding.AllColumnsJson);
        columns["location"].Should().Contain("/source/main#start=", "isenção, varredura e relatório leem location");
        columns["quickfixInfo"].Should().Be(finding.FindingId);
        columns["tag_REF_OBJ_NAME"].Should().Be(finding.RefObjectName);
        columns["tag_REF_OBJ_NAME"].Should().NotBe("FROM");

        // remediação (ExtractionRows)
        var row = Mignow.Atc.Application.Modules.Remediation.ExtractionRows.Row(finding);
        var layout = Mignow.Atc.Application.Modules.Remediation.ExtractionRows.Layout;
        row[layout.ToList().IndexOf("Referenced Object")].Should().Be(columns["tag_REF_OBJ_NAME"]);
        row[layout.ToList().IndexOf("Finding ID")].Should().Be(finding.FindingId);

        // isenção (location → include/linha) e varredura (location)
        var (include, line) = Mignow.Atc.Application.Features.Exemptions.ExemptionListing.Location(columns["location"]);
        include.Should().NotBeNullOrEmpty();
        line.Should().BeGreaterThan(0);
    }
}
