using System.Globalization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Extraction;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Módulo Extraction: resultado do ATC pela ADT (GET) e complementos por SQL stateless (porte de <c>extrair.ps1</c> /
/// <c>AtcExtracao.psm1</c>). Somente leitura. Simulador: <c>Simulator/SimulatedSap.Extraction.cs</c>.
/// </summary>
public sealed partial class AdtSap : IAtcExtractionReader
{
    private const int ExtInBlock = 200;

    public Task<Result<IReadOnlyList<AtcRunHeader>>> ListRunsAsync(SapConnection connection, IReadOnlyList<string> prefixes, CancellationToken ct) =>
        Call(connection, "atc-runs", ct, async s =>
        {
            // '_' é curinga no LIKE: busca ampla (Z_MIG_ -> Z%MIG%); o prefixo exato é filtrado pela Application.
            var likes = string.Join(" OR ", prefixes.Select(p => $"run_series_name LIKE '{p.Replace("'", "''").Replace('_', '%').TrimEnd('%')}%'"));
            var sql = "SELECT display_id, exec_id, title, run_series_name, scheduled_on_ts, scheduled_by, is_complete, is_reworked, chk_profile_name, " +
                      $"configuration, count_prio1, count_prio2, count_prio3, count_prio4, count_plnerr FROM satc_ac_resulth WHERE ( {likes} )";
            var result = await SelectCoreAsync(s, sql, 100_000, ct);
            if (result.IsFailure)
                return Result.Failure<IReadOnlyList<AtcRunHeader>>(result.Error);
            IReadOnlyList<AtcRunHeader> headers = [.. result.Value.Rows.Select(r => new AtcRunHeader(
                ExtV(r, "DISPLAY_ID"), ExtV(r, "EXEC_ID"), ExtV(r, "TITLE"), ExtV(r, "RUN_SERIES_NAME"), ExtV(r, "SCHEDULED_ON_TS"), ExtV(r, "SCHEDULED_BY"),
                ExtV(r, "IS_COMPLETE") == "X", ExtV(r, "IS_REWORKED").Length > 0, ExtV(r, "CHK_PROFILE_NAME"), ExtV(r, "CONFIGURATION"), ExtInt(r, "COUNT_PRIO1"),
                ExtInt(r, "COUNT_PRIO2"), ExtInt(r, "COUNT_PRIO3"), ExtInt(r, "COUNT_PRIO4"), ExtInt(r, "COUNT_PLNERR")))];
            return Result.Success(headers);
        });

    public Task<Result<AtcRawFindings>> ReadFindingsAsync(SapConnection connection, string displayId, string language, CancellationToken ct) =>
        Call(connection, "atc-results", ct, async s =>
        {
            var path = $"{Root}/atc/results/{Uri.EscapeDataString(displayId)}?includeExemptedFindings=true&sap-language={Uri.EscapeDataString(language)}";
            using var response = await s.SendAsync(HttpMethod.Get, path, ct, accept: "application/xml");
            var xml = await Ok(response, "atc-results", ct);
            return Result.Success(AdtAtcXml.ParseFindings(xml));
        });

    public Task<Result<AtcComplements>> ReadComplementsAsync(SapConnection connection, AtcComplementRequest request, CancellationToken ct) =>
        Call(connection, "atc-complements", ct, async s =>
        {
            var lang = request.Language.Length > 0 ? request.Language[..1] : "E";
            Error? error = null;

            async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> Q(string sql)
            {
                if (error is not null)
                    return [];
                var r = await SelectCoreAsync(s, sql, 100_000, ct);
                if (r.IsFailure)
                {
                    error = r.Error;
                    return [];
                }
                return r.Value.Rows;
            }

            async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> QIn(string sql, IEnumerable<string> values)
            {
                List<IReadOnlyDictionary<string, string>> all = [];
                foreach (var list in AdtSql.InLists(values, ExtInBlock))
                    all.AddRange(await Q(sql.Replace("{IN}", list, StringComparison.Ordinal)));
                return all;
            }

            async Task<Dictionary<string, string>> DomainByRoll(string roll)
            {
                var d = ExtTexts();
                foreach (var r in await Q("SELECT t~domvalue_l, t~ddtext FROM dd04l AS d INNER JOIN dd07t AS t ON t~domname = d~domname " +
                                          $"WHERE d~rollname = '{roll}' AND d~as4local = 'A' AND t~ddlanguage = '{lang}' AND t~as4local = 'A'"))
                    d[ExtV(r, "DOMVALUE_L")] = ExtV(r, "DDTEXT");
                return d;
            }

            // tabelas opcionais: nem todo sistema tem o baseline do ATC (MI9/S4CORE 108 SP01 sem SATC_AC_BSI) ou os títulos de nota
            string[] optional = ["SATC_AC_BSI", "CWBNTSTXT"];
            var present = (await Q("SELECT tabname FROM dd02l WHERE as4local = 'A' AND tabname IN ( 'SATC_AC_BSI', 'CWBNTSTXT' )"))
                .Select(r => ExtV(r, "TABNAME")).ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<string> missing = error is null ? [.. optional.Where(t => !present.Contains(t))] : [];

            var items = new Dictionary<string, AtcItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in await QIn("SELECT item_id, type, created_on, updated_on, last_run_index, processor, status FROM satc_ac_itm WHERE item_id IN ( {IN} )", request.ItemIds))
                items[ExtV(r, "ITEM_ID").ToUpperInvariant()] = new AtcItem(ExtV(r, "TYPE"), ExtV(r, "CREATED_ON"), ExtV(r, "UPDATED_ON"), ExtV(r, "LAST_RUN_INDEX"),
                    ExtV(r, "PROCESSOR"), ExtV(r, "STATUS"));
            var statusTexts = await DomainByRoll("SATC_D_AC_ITEM_STATUS");
            var typeTexts = await DomainByRoll("SATC_D_AC_ITEM_TYPE");

            var baselines = new Dictionary<string, List<AtcBaselineEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in missing.Contains("SATC_AC_BSI") ? [] : await Q("SELECT f~item_id, b~baseline_ix, b~baseline_effect FROM satc_ac_fnd AS f INNER JOIN satc_ac_bsi AS b " +
                                      "ON b~object_ix = f~object_ix AND b~checksum = f~checksum AND b~module_ix = f~module_ix AND b~message_ix = f~message_ix"))
            {
                var key = ExtV(r, "ITEM_ID").ToUpperInvariant();
                if (!baselines.TryGetValue(key, out var list))
                    baselines[key] = list = [];
                list.Add(new AtcBaselineEntry(ExtV(r, "BASELINE_IX"), ExtV(r, "BASELINE_EFFECT")));
            }
            var effectTexts = await DomainByRoll("SATC_D_AC_BASELINE_EFFECT");

            var tadir = new Dictionary<string, AtcTadirEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in await QIn("SELECT object, obj_name, author, devclass FROM tadir WHERE pgmid = 'R3TR' AND obj_name IN ( {IN} )", request.ObjectNames))
                tadir[$"{ExtV(r, "OBJECT")}|{ExtV(r, "OBJ_NAME")}"] = new AtcTadirEntry(ExtV(r, "AUTHOR"), ExtV(r, "DEVCLASS"));

            var notes = ExtTexts();
            foreach (var r in missing.Contains("CWBNTSTXT") ? [] : await QIn($"SELECT numm, stext FROM cwbntstxt WHERE langu = '{lang}' AND numm IN ( {{IN}} )", request.Notes))
                notes[ExtV(r, "NUMM")] = ExtV(r, "STEXT");

            var categories = ExtTexts();
            foreach (var r in await Q($"SELECT domvalue_l, ddtext FROM dd07t WHERE domname = 'SYCM_PLIST_CHANGE_CATEGORY' AND ddlanguage = '{lang}' AND as4local = 'A'"))
                categories[ExtV(r, "DOMVALUE_L")] = ExtV(r, "DDTEXT");
            var exemptions = await DomainByRoll("SATC_D_AC_RSLT_XMPT_KIND");

            if (error is not null)
                return Result.Failure<AtcComplements>(error);
            return Result.Success(new AtcComplements(items, statusTexts, typeTexts,
                baselines.ToDictionary(k => k.Key, k => (IReadOnlyList<AtcBaselineEntry>)k.Value, StringComparer.OrdinalIgnoreCase), effectTexts, tadir, notes,
                categories, exemptions, missing));
        });

    private static Dictionary<string, string> ExtTexts() => new(StringComparer.OrdinalIgnoreCase);

    private static string ExtV(IReadOnlyDictionary<string, string> row, string column) => row.TryGetValue(column, out var v) ? v : string.Empty;

    private static int ExtInt(IReadOnlyDictionary<string, string> row, string column) =>
        int.TryParse(ExtV(row, column), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;
}
