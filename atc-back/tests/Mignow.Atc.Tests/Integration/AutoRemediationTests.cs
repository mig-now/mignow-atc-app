using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Mignow.Atc.Tests.Knowledge;

namespace Mignow.Atc.Tests.Integration;

/// <summary>
/// "Play" da remediação no simulador (Postgres de teste, IA Fake, KB temporária): um único run leva os itens de pendente até aplicado no SAP,
/// sem etapa manual; depois o monitor mostra a sessão com o run e a exportação sai com a coluna de status.
/// </summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class AutoRemediationTests(AtcApiFactory api)
{
    [Fact]
    public async Task Play_leva_os_itens_de_pendente_ate_aplicado_sem_etapa_manual()
    {
        var client = api.CreateClient();
        using var kb = new TempKb();
        kb.Write("elementos/tabelas/KONV.md", TempKb.File_("KONV", TempKb.Card("S00012", 5, "SELECT * FROM konv INTO TABLE lt.", "SELECT * FROM prcd_elements INTO TABLE lt.")));
        var settings = await client.GetAsync("/v1/settings").DataAsync<SettingsDto>();
        try
        {
            (await client.PutAsJsonAsync("/v1/settings", settings with { KbPath = kb.Root })).StatusCode.Should().Be(HttpStatusCode.OK);
            await WaitKbAsync(client);
            await PlayAsync(client);
        }
        finally
        {
            await client.PutAsJsonAsync("/v1/settings", settings);
        }
    }

    /// <summary>O caminho da KB fica alguns segundos em cache (SettingsKbPathResolver): espera a KB deste teste responder antes do play.</summary>
    private static async Task WaitKbAsync(HttpClient client)
    {
        for (var i = 0; i < 30; i++)
        {
            var lookup = await client.GetAsync("/v1/kb/lookup?ref=KONV");
            if (lookup.IsSuccessStatusCode && (await lookup.Content.ReadAsStringAsync()).Contains("S00012", StringComparison.Ordinal))
                return;
            await Task.Delay(500);
        }
        throw new TimeoutException("A KB temporária do teste não ficou visível para a API.");
    }

    private async Task PlayAsync(HttpClient client)
    {
        var sap = api.NewSimulatedSap();
        var (customerId, system) = await TestData.CustomerWithSystemAsync(client, sap.Host);
        var extraction = await client.PostAsJsonAsync("/v1/extractions", new { sapSystemId = system.Id, force = false }).DataAsync<JsonElement>();
        await api.ProcessQueuedAsync();
        var extractionId = (await client.GetAsync($"/v1/runs/{extraction.GetProperty("id").GetGuid()}").DataAsync<JsonElement>())
            .GetProperty("result").GetProperty("extractionId").GetGuid();

        // sem request a aplicação falharia no fim: o play recusa antes de começar
        var noRequest = await client.PostAsJsonAsync("/v1/sessions", new { extractionId, priorities = "1,2,3" }).DataAsync<JsonElement>();
        var refused = await client.PostAsJsonAsync($"/v1/sessions/{noRequest.GetProperty("id").GetGuid()}/auto-remediation", new { });
        refused.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await refused.ProblemAsync()).GetProperty("detail").GetString().Should().Be("Apply.RequestRequired");
        var withRequest = await client.PutAsJsonAsync($"/v1/sessions/{noRequest.GetProperty("id").GetGuid()}/request", new { request = "sbxk900121" })
            .DataAsync<JsonElement>();
        withRequest.GetProperty("request").GetString().Should().Be("SBXK900121");

        await api.ProcessQueuedAsync(); // leitura automática dos fontes da sessão anterior
        var session = await client.PostAsJsonAsync("/v1/sessions", new { extractionId, priorities = "1,2,3", request = "SBXK900120" }).DataAsync<JsonElement>();
        var sessionId = session.GetProperty("id").GetGuid();
        // ao carregar os itens, os fontes são lidos na sequência (run ReadSources); o play espera ele terminar
        var sourcesRun = session.GetProperty("sourcesRunId").GetGuid();
        await api.ProcessQueuedAsync();
        (await client.GetAsync($"/v1/runs/{sourcesRun}").DataAsync<JsonElement>()).GetProperty("status").GetString().Should().Be("Completed");

        // aba Fontes: objetos com fonte lido e o conteúdo de um include (só leitura)
        var sources = await client.GetAsync($"/v1/sessions/{sessionId}/sources").DataAsync<JsonElement>();
        sources.GetProperty("read").GetInt32().Should().BeGreaterThan(0);
        var firstObject = sources.GetProperty("items").EnumerateArray().First();
        var include = firstObject.GetProperty("includes").EnumerateArray().First();
        include.GetProperty("lines").GetInt32().Should().BeGreaterThan(0);
        var content = await client.GetAsync($"/v1/sessions/{sessionId}/sources/{include.GetProperty("id").GetGuid()}").DataAsync<JsonElement>();
        content.GetProperty("objectName").GetString().Should().Be(firstObject.GetProperty("objectName").GetString());
        content.GetProperty("source").GetString().Should().NotBeNullOrWhiteSpace();
        (await client.GetAsync($"/v1/sessions/{sessionId}/sources/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var items = await client.GetAsync($"/v1/sessions/{sessionId}/items?pageSize=200").DataAsync<PagedDto<JsonElement>>();
        var konv = items.Items.First(i => i.GetProperty("refObject").GetString() == "KONV" && i.GetProperty("objectName").GetString() == "ZMIG_MAIN_INCL_F01");
        var konvId = konv.GetProperty("id").GetGuid();

        // os outros objetos estão presos em outra request: a gravação deles falha (ERRO_REQUEST) e o item fica "Confirmado", mas com erro no SAP
        sap.State.WithLock(() =>
        {
            foreach (var o in sap.State.Objects.Where(o => !o.Ref.Name.StartsWith("ZMIG_MAIN_INCL", StringComparison.OrdinalIgnoreCase)))
                o.OpenRequest = "SBXK999999";
            return true;
        });

        // play com todos os pendentes (sem itemIds): um único run, sem decisão do consultor no meio
        var started = await client.PostAsJsonAsync($"/v1/sessions/{sessionId}/auto-remediation", new { });
        started.StatusCode.Should().Be(HttpStatusCode.Accepted, await started.Content.ReadAsStringAsync());
        var runId = (await started.DataAsync<JsonElement>()).GetProperty("id").GetGuid();
        await api.ProcessQueuedAsync();

        var run = await client.GetAsync($"/v1/runs/{runId}").DataAsync<JsonElement>();
        run.GetProperty("status").GetString().Should().Be("Completed", run.ToString());
        run.GetProperty("kind").GetString().Should().Be("AutoRemediation");
        run.GetProperty("steps").EnumerateArray().Select(s => s.GetProperty("code").GetString()).Should().ContainInOrder(
            "AUTO_SOURCES_PLAN", "READ_SOURCES_FETCH", "AUTO_TRIAGE", "CONFIRM_R1_CHECK", "CONFIRM_R1_APPLY", "PROPOSE", "AUTO_PROPOSE_AI",
            "AUTO_REMEDIATION_CONFIRM", "AUTO_SIMULATE", "AUTO_REVIEW", "AUTO_APPLY", "BATCH_REPORT", "AUTO_REMEDIATION_SUMMARY");
        run.GetProperty("steps").EnumerateArray().Should().OnlyContain(s => s.GetProperty("status").GetString() == "Completed");

        // o resultado acumula as etapas (antes a aplicação apagava o resumo da proposta do relatório do lote)
        var result = run.GetProperty("result");
        result.GetProperty("propose").ValueKind.Should().Be(JsonValueKind.Object);
        result.GetProperty("apply").GetArrayLength().Should().BeGreaterThan(0);
        result.GetProperty("reportMarkdown").GetString().Should().NotBeNullOrWhiteSpace();
        // triagem com as regras do tipo de projeto da sessão (conversão: as genéricas, sem a R9 de upgrade), sem escolha do consultor
        var projectType = Enum.Parse<Mignow.Atc.Domain.Registry.ProjectType>(session.GetProperty("projectType").GetString()!);
        var expectedRules = (await client.GetAsync($"/v1/triage-rules?projectType={projectType}").DataAsync<List<JsonElement>>())
            .Select(r => r.GetProperty("code").GetString()).ToList();
        result.GetProperty("triageRules").EnumerateArray().Select(r => r.GetString()).Should().Equal(expectedRules);
        var summary = result.GetProperty("summary");
        summary.GetProperty("items").GetInt32().Should().Be(items.Items.Count);
        summary.GetProperty("activated").GetInt32().Should().BeGreaterThan(0);

        // o item com cartão na KB foi confirmado pela KB e ativado no SAP
        var proposal = await client.GetAsync($"/v1/sessions/{sessionId}/items/{konvId}/proposal").DataAsync<JsonElement>();
        proposal.GetProperty("status").GetString().Should().Be("Confirmado KB");
        proposal.GetProperty("application").GetProperty("result").GetString().Should().Be("ATIVADO");

        // a lista de itens mostra o resultado no SAP ao lado do status (o "Confirmado" sozinho não diz se gravou)
        var listed = (await client.GetAsync($"/v1/sessions/{sessionId}/items?pageSize=200").DataAsync<PagedDto<JsonElement>>())
            .Items.Single(i => i.GetProperty("id").GetGuid() == konvId);
        listed.GetProperty("applicationResult").GetString().Should().Be("ATIVADO");
        listed.TryGetProperty("applicationDetail", out _).Should().BeTrue();

        // falhas no SAP: filtro próprio, contagem na sessão e coluna "Aplicação SAP" no CSV
        var failed = await client.GetAsync($"/v1/sessions/{sessionId}/items?status=applyError&pageSize=200").DataAsync<PagedDto<JsonElement>>();
        failed.Items.Should().NotBeEmpty();
        failed.Items.Should().OnlyContain(i => i.GetProperty("applicationResult").GetString() == "ERRO_REQUEST"
            && i.GetProperty("applicationDetail").GetString()!.Contains("SBXK999999"));
        var counted = await client.GetAsync($"/v1/sessions/{sessionId}").DataAsync<JsonElement>();
        counted.GetProperty("counts").GetProperty("applyErrors").GetInt32().Should().Be(failed.Items.Count);

        // reprocessar: a request foi liberada no SAP e o play aceita o item com erro (sem nova proposta: simula, revisa e aplica de novo)
        var retryId = failed.Items[0].GetProperty("id").GetGuid();
        sap.State.WithLock(() =>
        {
            foreach (var o in sap.State.Objects)
                o.OpenRequest = null;
            return true;
        });
        var retry = await client.PostAsJsonAsync($"/v1/sessions/{sessionId}/auto-remediation", new { itemIds = new[] { retryId } });
        retry.StatusCode.Should().Be(HttpStatusCode.Accepted, await retry.Content.ReadAsStringAsync());
        var retryRunId = (await retry.DataAsync<JsonElement>()).GetProperty("id").GetGuid();
        await api.ProcessQueuedAsync();
        (await client.GetAsync($"/v1/runs/{retryRunId}").DataAsync<JsonElement>()).GetProperty("status").GetString().Should().Be("Completed");
        var retried = await client.GetAsync($"/v1/sessions/{sessionId}/items/{retryId}/proposal").DataAsync<JsonElement>();
        retried.GetProperty("application").GetProperty("result").GetString().Should().Be("ATIVADO");
        runId = retryRunId; // o monitor abaixo mostra o reprocessamento como última execução

        // monitor: a sessão aparece com o play como última execução
        var monitor = await client.GetAsync($"/v1/sessions/monitor?customerId={customerId}").DataAsync<List<JsonElement>>();
        var row = monitor.Single(r => r.GetProperty("session").GetProperty("id").GetGuid() == sessionId);
        row.GetProperty("lastRun").GetProperty("id").GetGuid().Should().Be(runId);
        row.GetProperty("session").GetProperty("counts").GetProperty("kb").GetInt32().Should().BeGreaterThan(0);

        // exportação e histórico depois do play
        var export = await client.GetAsync($"/v1/sessions/{sessionId}/export");
        export.StatusCode.Should().Be(HttpStatusCode.OK);
        var csv = await export.Content.ReadAsStringAsync();
        csv.Should().Contain("Status Remediação").And.Contain("Confirmado KB").And.Contain("Aplicação SAP").And.Contain("ATIVADO");
        if (failed.Items.Count > 1) // o reprocessado virou ATIVADO; os demais continuam com o erro e o motivo
            csv.Should().Contain("ERRO_REQUEST: objeto já está na request SBXK999999");
        (await client.GetAsync($"/v1/sessions/{sessionId}/history")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
