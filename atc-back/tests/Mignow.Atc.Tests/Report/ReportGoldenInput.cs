using System.Globalization;
using System.Text.Json;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.Report;

/// <summary>
/// Converte a entrada sintética do golden (<c>Golden/report/entrada</c>, no formato de pastas que o <c>coletar.py</c> lê) em entidades.
/// A saída esperada (<c>Golden/report/saida/dados.json</c>) foi gerada pelo script original (<c>gerar.sh</c>).
/// </summary>
internal static class ReportGoldenInput
{
    public static string Root => Path.Combine(AppContext.BaseDirectory, "Golden", "report");

    public static JsonElement Expected() => JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "saida", "dados.json"))).RootElement.Clone();

    private static JsonElement J(string rel) => JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "entrada", rel))).RootElement.Clone();

    private static DateTimeOffset T(JsonElement e, string prop) => DateTimeOffset.Parse(e.GetProperty(prop).GetString()!, CultureInfo.InvariantCulture);

    private static Run NewRun(RunKind kind, Guid? subject, DateTimeOffset start, DateTimeOffset end, int input, int output)
    {
        var run = Run.Create(kind, 1, 1, subject, null, "test", ["S"], start);
        run.Start(start);
        if (input + output > 0)
            run.AddUsage(input, output);
        run.Complete(end);
        return run;
    }

    private static void AddUsage(this Run run, int input, int output) => run.AddAiUsage(input, output, null);

    public static ReportInput Load()
    {
        var cfg = J("config.json");
        var customer = Customer.Create("GOLD", "GOLD");
        var system = SapSystem.Create(customer, cfg.GetProperty("sid").GetString()!, cfg.GetProperty("mandante").GetString()!, "sim://gold", 0, false, "EN", null,
            false, ProjectType.Conversion, cfg.GetProperty("autor").GetString()!, cfg.GetProperty("request").GetString(), cfg.GetProperty("aprovador_isencao").GetString(), "DEV");
        system.RecordConnectionTest(DateTimeOffset.UnixEpoch, true, null, cfg.GetProperty("release").GetString(), "T", null);

        var sessao = J("sessao.json");
        var session = RemediationSession.Create(1, 1, RemediationSource.Extraction, null, "1", cfg.GetProperty("request").GetString(), "AUT-XX", ProjectType.Conversion,
            "[]", "utf-8-sig", ";", 1);
        var items = new List<RemediationItem>();
        foreach (var i in sessao.GetProperty("itens").EnumerateArray())
        {
            var item = RemediationItem.Create(1, i.GetProperty("n").GetInt32(), i.GetProperty("objeto").GetString()!, i.GetProperty("tipo").GetString()!,
                i.GetProperty("ref").GetString(), null, i.GetProperty("check").GetString(), i.GetProperty("msg").GetString(), null, null, null, null, "1", null,
                ItemOrigin.Atc, "[]");
            var obs = i.GetProperty("obs").GetString() ?? "";
            foreach (var h in i.GetProperty("historico_status").EnumerateArray())
                item.MarkStatus(h.GetProperty("status").GetString()!, obs, T(h, "data"));
            if (i.TryGetProperty("aplicacao", out var ap))
            {
                var attempts = ap.GetProperty("historico").EnumerateArray()
                    .Select(a => new ApplicationAttempt(DateTimeOffset.UnixEpoch, "apply", a.GetProperty("resultado").GetString()!, a.GetProperty("request").GetString(), null)).ToList();
                item.SetApplication(new ItemApplication(attempts[^1].Result, attempts[^1].Request, null, null, null, attempts));
            }
            if (i.TryGetProperty("preparacao", out var prep))
                item.SetPreparation(new ItemPreparation(prep.GetProperty("fonte").GetString(), null, null, null, null, null, null));
            items.Add(item);
        }
        var sessions = new[] { new SessionData(session, items, DateTimeOffset.Parse("2026-10-20T10:00:00+00:00", CultureInfo.InvariantCulture)) };

        var runs = new List<RunWithCalls>();
        var extractions = new List<AtcExtraction>();
        foreach (var h in J("extracoes/historico.json").EnumerateArray())
        {
            var fim = T(h, "fim_utc");
            var ex = AtcExtraction.Create(1, h.GetProperty("serie").GetString()!, h.GetProperty("display_id").GetString()!, null,
                DateTimeOffset.ParseExact(h.GetProperty("executada_utc").GetString()!, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal), fim);
            ex.SetCounts(h.GetProperty("qtde_total").GetInt32(), h.GetProperty("qtde_em_baseline").GetInt32(), 6, 30, 24, h.GetProperty("com_baseline_p1").GetInt32(),
                h.GetProperty("com_baseline_p2").GetInt32(), h.GetProperty("com_baseline_p3").GetInt32());
            extractions.Add(ex);
            var tk = h.GetProperty("tokens_detalhe");
            runs.Add(new RunWithCalls(NewRun(RunKind.Extraction, Guid.NewGuid(), T(h, "inicio_utc"), fim, tk.GetProperty("input_tokens").GetInt32(), tk.GetProperty("output_tokens").GetInt32()), []));
        }
        foreach (var h in J("remediacao/historico.json").EnumerateArray())
        {
            var tk = h.GetProperty("tokens_detalhe");
            runs.Add(new RunWithCalls(NewRun(RunKind.Propose, session.Guid, T(h, "inicio_utc"), T(h, "fim_utc"), tk.GetProperty("input_tokens").GetInt32(), tk.GetProperty("output_tokens").GetInt32()), []));
        }

        var batch = ExemptionBatch.Create(1, 1, "1", "APR1", null);
        var folder = Directory.GetDirectories(Path.Combine(Root, "entrada", "isencoes"), "lote_*").Single();
        var name = Path.GetFileName(folder);
        var at = DateTimeOffset.ParseExact(name[^15..], "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        var lista = J($"isencoes/{name}/lista.json").GetProperty("findings").EnumerateArray().ToList();
        var envio = J($"isencoes/{name}/resultado_envio.json").EnumerateArray().ToDictionary(x => x.GetProperty("seq").GetInt32());
        var sim = J($"isencoes/{name}/resultado_simulacao.json").EnumerateArray().ToDictionary(x => x.GetProperty("seq").GetInt32());
        var requests = new List<ExemptionRequestData>();
        foreach (var f in lista)
        {
            var seq = f.GetProperty("seq").GetInt32();
            var r = ExemptionRequest.Create(1, 1, $"M{seq}", seq, envio[seq].GetProperty("motivo").GetString()!, "j", false);
            r.Record(envio[seq].GetProperty("resultado").GetString()!, sim[seq].TryGetProperty("mensagem", out var m) ? m.GetString() : null);
            requests.Add(new ExemptionRequestData(r, f.GetProperty("n").GetInt32()));
        }
        foreach (var h in J("isencoes/historico.json").EnumerateArray())
        {
            var tk = h.GetProperty("tokens_detalhe");
            runs.Add(new RunWithCalls(NewRun(RunKind.ExemptionSend, batch.Guid, T(h, "inicio_utc"), T(h, "fim_utc"), tk.GetProperty("input_tokens").GetInt32(), tk.GetProperty("output_tokens").GetInt32()), []));
        }

        return new ReportInput(customer, system, GeneralSettings.CreateDefault(), sessions, [new ExemptionBatchData(batch, at, requests)], extractions, runs, null,
            DateTimeOffset.Parse("2026-11-01T12:00:00+00:00", CultureInfo.InvariantCulture), TimeZoneInfo.Utc);
    }
}
