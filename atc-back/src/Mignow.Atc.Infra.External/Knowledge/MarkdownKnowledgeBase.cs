using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mignow.Atc.Application.Modules.Knowledge;
using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Infra.External.Knowledge;

/// <summary>
/// KB markdown <c>KB_ATC</c> (porte de <c>kb_lookup</c>/<c>parse_cards</c>/<c>cmd_registrar</c>/<c>cmd_fato</c>/<c>cmd_ddic_lista</c>
/// do atc.py). Leitura com cache por arquivo validado por mtime+tamanho (a pasta é compartilhada com as skills, que
/// escrevem direto); escrita serializada por <see cref="KbWriteLock"/>, relendo o arquivo antes de numerar e sem
/// reescrever cartões existentes (só acrescenta ao fim, gravação atômica).
/// </summary>
public sealed partial class MarkdownKnowledgeBase : IKnowledgeBase
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);
    private static readonly string[] DdicKinds = ["elementos_dados", "estruturas", "funcoes", "tabelas"];
    private const string FactsCategory = "fatos";

    private readonly IKbPathResolver _paths;
    private readonly Func<DateOnly> _today;
    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<string, FileEntry> _files = new(StringComparer.Ordinal);

    public MarkdownKnowledgeBase(IServiceScopeFactory scopes, IConfiguration configuration, ILoggerFactory loggers)
        : this(new SettingsKbPathResolver(scopes, configuration, loggers.CreateLogger<SettingsKbPathResolver>()),
            () => DateOnly.FromDateTime(DateTime.Now), loggers.CreateLogger<MarkdownKnowledgeBase>())
    {
    }

    internal MarkdownKnowledgeBase(IKbPathResolver paths, Func<DateOnly> today, ILogger? logger = null)
    {
        _paths = paths;
        _today = today;
        _logger = logger;
    }

    // ------------------------------------------------------------------ leitura

    public async Task<KbLookupResult> LookupAsync(string referencedObject, string? refObjectType, int top, CancellationToken ct)
    {
        var root = await RootAsync(ct);
        var subst = Substitutions(root).ToMap();
        var slug = KbText.Slug(referencedObject);
        var found = new List<KbCard>();
        foreach (var category in KbCategories.SearchOrder(refObjectType))
        {
            foreach (var (folder, isContribution) in new[] { ("elementos", false), ("contribuicoes", true) })
                found.AddRange(CardsOf(root, Path.Combine(root, folder, category, slug + ".md"), isContribution, subst));
            if (found.Count > 0)
                break;
        }
        // contribuições primeiro, depois não substituídas, depois sem alerta, depois por ocorrências (OrderBy é estável)
        var ordered = found.OrderBy(c => !c.IsContribution).ThenBy(c => c.ReplacedBy is not null)
            .ThenBy(c => c.Alerts.Count > 0).ThenByDescending(c => c.Occurrences).ToList();
        return new KbLookupResult(ordered.Take(Math.Max(top, 0)).ToList(), ordered.Count);
    }

    public async Task<IReadOnlyList<KbFact>> FactsAsync(string element, CancellationToken ct) =>
        FactsOf(Path.Combine(await RootAsync(ct), "contribuicoes", "fatos", KbText.FactSlug(element) + ".md"));

    public async Task<IReadOnlyList<KbIndexEntry>> BrowseAsync(CancellationToken ct)
    {
        var root = await RootAsync(ct);
        var map = new Dictionary<(string Category, string Slug), (string Name, int Cards, int Contribs)>();
        foreach (var category in KbCategories.All)
        {
            foreach (var (folder, isContribution) in new[] { ("elementos", false), ("contribuicoes", true) })
            {
                foreach (var file in MdFiles(Path.Combine(root, folder, category)))
                {
                    var entry = Entry(file);
                    if (entry is null)
                        continue;
                    var slug = Path.GetFileNameWithoutExtension(file);
                    map.TryGetValue((category, slug), out var cur);
                    var name = isContribution ? cur.Name ?? entry.Heading ?? slug : entry.Heading ?? cur.Name ?? slug;
                    map[(category, slug)] = (name, cur.Cards + (isContribution ? 0 : entry.Cards.Count), cur.Contribs + (isContribution ? entry.Cards.Count : 0));
                }
            }
        }
        var facts = new Dictionary<string, (string Name, int Count)>(StringComparer.Ordinal);
        foreach (var file in MdFiles(Path.Combine(root, "contribuicoes", "fatos")).Where(f => Path.GetFileName(f) != "README.md"))
        {
            var entry = Entry(file);
            if (entry is null)
                continue;
            var name = (entry.Heading ?? Path.GetFileNameWithoutExtension(file)).Replace(" – fatos", "", StringComparison.Ordinal);
            facts[Path.GetFileNameWithoutExtension(file)] = (name, entry.Facts.Count);
        }
        var result = map.Select(kv => new KbIndexEntry(kv.Key.Category, kv.Value.Name, kv.Key.Slug, kv.Value.Cards, kv.Value.Contribs,
            facts.TryGetValue(kv.Key.Slug, out var f) ? f.Count : 0)).ToList();
        var withEntry = map.Keys.Select(k => k.Slug).ToHashSet(StringComparer.Ordinal);
        result.AddRange(facts.Where(kv => !withEntry.Contains(kv.Key)).Select(kv => new KbIndexEntry(FactsCategory, kv.Value.Name, kv.Key, 0, 0, kv.Value.Count)));
        return [.. result.OrderBy(e => e.Element, StringComparer.OrdinalIgnoreCase).ThenBy(e => e.Category, StringComparer.Ordinal)];
    }

    public async Task<IReadOnlyList<KbPatternEntry>> BrowsePatternsAsync(CancellationToken ct)
    {
        var path = Path.Combine(await RootAsync(ct), "indices", "por_padrao.md");
        if (!File.Exists(path))
            return [];
        var rows = new List<KbPatternEntry>();
        foreach (var line in KbText.NormalizeNewlines(await File.ReadAllTextAsync(path, Encoding.UTF8, ct)).Split('\n'))
        {
            var m = PatternRow().Match(line);
            if (m.Success)
                rows.Add(new KbPatternEntry(m.Groups[1].Value, m.Groups[2].Value.Trim(), int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture),
                    int.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture)));
        }
        return rows;
    }

    public async Task<KbElementDetail?> GetElementAsync(string category, string element, CancellationToken ct)
    {
        if (!KbCategories.IsValid(category) && category != FactsCategory)
            return null;
        var root = await RootAsync(ct);
        var slug = KbText.Slug(element);
        var factsList = FactsOf(Path.Combine(root, "contribuicoes", "fatos", slug + ".md"));
        if (category == FactsCategory)
            return factsList.Count == 0 ? null : new KbElementDetail(category, slug, [], [], factsList, []);
        var subst = Substitutions(root);
        var map = subst.ToMap();
        var basePath = Path.Combine(root, "elementos", category, slug + ".md");
        var contribPath = Path.Combine(root, "contribuicoes", category, slug + ".md");
        if (!File.Exists(basePath) && !File.Exists(contribPath))
            return null;
        var cards = CardsOf(root, basePath, false, map);
        var contribs = CardsOf(root, contribPath, true, map);
        var ids = cards.Concat(contribs).Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var repl = subst.ToReplacements().Where(r => ids.Contains(r.CardId) || ids.Contains(r.ReplacedBy)).ToList();
        var name = Entry(basePath)?.Heading ?? Entry(contribPath)?.Heading ?? slug;
        return new KbElementDetail(category, name, cards, contribs, factsList, repl);
    }

    public async Task<IReadOnlyList<KbDdicEntry>> ListDdicAsync(CancellationToken ct)
    {
        var ddic = Path.Combine(await RootAsync(ct), "ddic");
        if (!Directory.Exists(ddic))
            return [];
        return [.. DdicKinds.SelectMany(kind => FilesOf(Path.Combine(ddic, kind), "*.abap").Select(f => new KbDdicEntry(kind, Path.GetFileNameWithoutExtension(f))))
            .OrderBy(e => e.Kind, StringComparer.Ordinal).ThenBy(e => e.Name, StringComparer.Ordinal)];
    }

    public async Task<string?> GetDdicAsync(string kind, string name, CancellationToken ct)
    {
        if (!DdicKinds.Contains(kind) || string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name) || name.Contains(".."))
            return null;
        var path = Path.Combine(await RootAsync(ct), "ddic", kind, name + ".abap");
        return File.Exists(path) ? KbText.NormalizeNewlines(await File.ReadAllTextAsync(path, Encoding.UTF8, ct)) : null;
    }

    public async Task<KbDdicCandidates> DdicCandidatesAsync(int top, CancellationToken ct)
    {
        var root = await RootAsync(ct);
        var counts = new Dictionary<(string Type, string Name), int>();
        var files = KbCategories.All.SelectMany(c => MdFiles(Path.Combine(root, "elementos", c)))
            .Concat(MdFilesOneLevel(Path.Combine(root, "contribuicoes")));
        foreach (var file in files)
        {
            if (Entry(file) is not { } entry)
                continue;
            foreach (var card in entry.Cards)
            {
                var novo = card.New.ToUpperInvariant();
                foreach (Match fm in CallFunction().Matches(novo))
                {
                    var name = fm.Groups[1].Value;
                    if (name.StartsWith("BAPI_", StringComparison.Ordinal) || name.StartsWith("CONVERSION_EXIT", StringComparison.Ordinal)
                        || name.StartsWith("FI_", StringComparison.Ordinal) || name.StartsWith("UKM", StringComparison.Ordinal)
                        || name.StartsWith("CS_", StringComparison.Ordinal) || name.StartsWith("CLAF_", StringComparison.Ordinal)
                        || name.StartsWith("CACL_", StringComparison.Ordinal))
                        counts[("FUNC", name)] = counts.GetValueOrDefault(("FUNC", name)) + card.Occurrences;
                }
                foreach (Match sm in BapiStructure().Matches(novo))
                    counts[("STRU", sm.Groups[1].Value)] = counts.GetValueOrDefault(("STRU", sm.Groups[1].Value)) + card.Occurrences;
            }
        }
        var ddic = Path.Combine(root, "ddic");
        var cached = Directory.Exists(ddic)
            ? DdicKinds.SelectMany(k => FilesOf(Path.Combine(ddic, k), "*.abap")).Select(Path.GetFileNameWithoutExtension).ToHashSet(StringComparer.Ordinal)
            : [];
        // empate: ordem estável (tipo, nome); no Python depende da ordem do glob do sistema de arquivos
        var ranked = counts.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key.Type, StringComparer.Ordinal)
            .ThenBy(kv => kv.Key.Name, StringComparer.Ordinal).Take(Math.Max(top, 0)).ToList();
        return new KbDdicCandidates(
            [.. ranked.Select(kv => new KbDdicCandidate(kv.Key.Type, kv.Key.Name, kv.Value, cached.Contains(kv.Key.Name)))],
            [.. ranked.Where(kv => !cached.Contains(kv.Key.Name)).Select(kv => kv.Key.Name)]);
    }

    public Task ReloadAsync(CancellationToken ct)
    {
        _files.Clear();
        return Task.CompletedTask;
    }

    // ------------------------------------------------------------------ escrita

    public async Task<string> RegisterAsync(KbContribution contribution, CancellationToken ct)
    {
        if (!KbCategories.IsValid(contribution.Category))
            throw new KbException("Knowledge.InvalidCategory", $"Categoria deve ser uma de {string.Join(", ", KbCategories.All)}.");
        var slug = KbText.Slug(contribution.Element);
        if (slug.Length == 0)
            throw new KbException("Knowledge.ElementRequired", "O elemento é obrigatório.");
        var root = await RootAsync(ct);
        var replaces = contribution.Replaces.Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        var date = _today().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        await using var _ = await KbWriteLock.AcquireAsync(root, LockTimeout, ct);
        var contribRoot = Path.Combine(root, "contribuicoes");
        var path = Path.Combine(contribRoot, contribution.Category, slug + ".md");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        // relê dentro da trava: a numeração vem do arquivo como está agora (as skills podem ter escrito)
        var existing = File.Exists(path)
            ? await File.ReadAllTextAsync(path, Encoding.UTF8, ct)
            : KbMarkdownWriter.ContributionHeader(contribution.Element, contribution.Category);
        var id = $"C-{slug}-{Count(existing, "<a id=\"c") + 1:000}";
        var card = KbMarkdownWriter.ContributionCard(id, date, contribution.Problem, replaces, contribution.Before, contribution.After);
        await WriteAtomicAsync(path, existing + card, ct);
        if (replaces.Count > 0)
        {
            // a solução da KB continua no arquivo original, mas passa a ser exibida como substituída
            var sp = Path.Combine(contribRoot, "substituicoes.json");
            var st = KbSubstitutions.Parse(File.Exists(sp) ? await File.ReadAllTextAsync(sp, Encoding.UTF8, ct) : null);
            foreach (var s in replaces)
                st.Set(s, id, contribution.Element.ToUpperInvariant(), date, contribution.Reason ?? "");
            await WriteAtomicAsync(sp, st.Serialize(), ct);
        }
        await WriteContributionsReadmeAsync(contribRoot, ct);
        _logger?.LogInformation("KB: contribuição {Id} registrada em {Path}", id, path);
        return id;
    }

    public async Task<IReadOnlyList<string>> AddFactAsync(KbFactInput fact, CancellationToken ct)
    {
        var elements = fact.Elements.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        if (elements.Count == 0)
            throw new KbException("Knowledge.ElementRequired", "O elemento é obrigatório.");
        if (string.IsNullOrWhiteSpace(fact.Fact))
            throw new KbException("Knowledge.FactRequired", "O fato é obrigatório.");
        var root = await RootAsync(ct);
        var date = _today().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        await using var _ = await KbWriteLock.AcquireAsync(root, LockTimeout, ct);
        var dir = Path.Combine(root, "contribuicoes", "fatos");
        Directory.CreateDirectory(dir);
        var ids = new List<string>();
        foreach (var element in elements)
        {
            var slug = KbText.FactSlug(element);
            var path = Path.Combine(dir, slug + ".md");
            var existing = File.Exists(path) ? await File.ReadAllTextAsync(path, Encoding.UTF8, ct) : KbMarkdownWriter.FactHeader(slug);
            var id = $"F-{slug}-{Count(existing, "<a id=\"f-") + 1:000}";
            await WriteAtomicAsync(path, existing + KbMarkdownWriter.FactCard(id, date, fact.Fact, fact.Proof, fact.Impact), ct);
            ids.Add(id);
        }
        var rows = new List<(string, int)>();
        foreach (var f in Directory.EnumerateFiles(dir, "*.md").Select(Path.GetFileName).OfType<string>().Where(n => n != "README.md").Order(StringComparer.Ordinal))
            rows.Add((Path.GetFileNameWithoutExtension(f), Count(await File.ReadAllTextAsync(Path.Combine(dir, f), Encoding.UTF8, ct), "<a id=\"f-")));
        await WriteAtomicAsync(Path.Combine(dir, "README.md"), KbMarkdownWriter.FactsReadme(rows), ct);
        return ids;
    }

    private static async Task WriteContributionsReadmeAsync(string contribRoot, CancellationToken ct)
    {
        var rows = new List<(string Stem, string Folder, int Count)>();
        var files = Directory.EnumerateDirectories(contribRoot).Select(d => Path.GetFileName(d)!).Where(d => d != "fatos").Order(StringComparer.Ordinal)
            .SelectMany(folder => Directory.EnumerateFiles(Path.Combine(contribRoot, folder), "*.md")
                .Select(f => (Folder: folder, File: Path.GetFileName(f))).OrderBy(x => x.File, StringComparer.Ordinal));
        foreach (var (folder, file) in files)
            rows.Add((Path.GetFileNameWithoutExtension(file), folder, Count(await File.ReadAllTextAsync(Path.Combine(contribRoot, folder, file), Encoding.UTF8, ct), "<a id=\"c")));
        await WriteAtomicAsync(Path.Combine(contribRoot, "README.md"), KbMarkdownWriter.ContributionsReadme(rows), ct);
    }

    // ------------------------------------------------------------------ infra de arquivos

    private async Task<string> RootAsync(CancellationToken ct)
    {
        var root = await _paths.ResolveRootAsync(ct);
        if (!Directory.Exists(root))
            throw new KbException("Knowledge.KbNotFound", $"Pasta da KB não encontrada: {root}");
        return root;
    }

    private static int Count(string text, string needle)
    {
        var n = 0;
        for (var i = text.IndexOf(needle, StringComparison.Ordinal); i >= 0; i = text.IndexOf(needle, i + needle.Length, StringComparison.Ordinal))
            n++;
        return n;
    }

    /// <summary>Gravação atômica (arquivo temporário na mesma pasta + move): leitores nunca veem arquivo pela metade.</summary>
    private static async Task WriteAtomicAsync(string path, string content, CancellationToken ct)
    {
        var tmp = Path.Combine(Path.GetDirectoryName(path)!, $".{Path.GetFileName(path)}.{Path.GetRandomFileName()}.tmp");
        try
        {
            await File.WriteAllTextAsync(tmp, content, new UTF8Encoding(false), ct);
            File.Move(tmp, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmp))
                File.Delete(tmp);
        }
    }

    private static IEnumerable<string> MdFiles(string dir) => FilesOf(dir, "*.md");

    private static IEnumerable<string> MdFilesOneLevel(string dir) =>
        Directory.Exists(dir) ? Directory.EnumerateDirectories(dir).Order(StringComparer.Ordinal).SelectMany(d => FilesOf(d, "*.md")) : [];

    private static IEnumerable<string> FilesOf(string dir, string pattern) =>
        Directory.Exists(dir) ? Directory.EnumerateFiles(dir, pattern).Where(f => !Path.GetFileName(f).StartsWith('.')).Order(StringComparer.Ordinal) : [];

    private KbSubstitutions Substitutions(string root)
    {
        var path = Path.Combine(root, "contribuicoes", "substituicoes.json");
        var info = new FileInfo(path);
        if (!info.Exists)
            return new KbSubstitutions();
        var entry = _files.AddOrUpdate(path, _ => LoadSubstitutions(path, info), (_, old) => old.Matches(info) ? old : LoadSubstitutions(path, info));
        return entry.Substitutions!;
    }

    private static FileEntry LoadSubstitutions(string path, FileInfo info)
    {
        try
        {
            return new FileEntry(info.LastWriteTimeUtc.Ticks, info.Length, [], null, [], KbSubstitutions.Parse(File.ReadAllText(path, Encoding.UTF8)));
        }
        catch (IOException)
        {
            return new FileEntry(-1, -1, [], null, [], new KbSubstitutions());
        }
    }

    private FileEntry? Entry(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
        {
            _files.TryRemove(path, out _);
            return null;
        }
        return _files.AddOrUpdate(path, _ => Load(path, info), (_, old) => old.Matches(info) ? old : Load(path, info));
    }

    private static FileEntry Load(string path, FileInfo info)
    {
        var md = KbText.NormalizeNewlines(File.ReadAllText(path, Encoding.UTF8));
        var facts = KbCardParser.ParseFacts(md).Select(f => new KbFact(f.Id, f.Date, f.Text)).ToList();
        return new FileEntry(info.LastWriteTimeUtc.Ticks, info.Length, KbCardParser.Parse(md), KbCardParser.Heading(md), facts, null);
    }

    private IReadOnlyList<KbFact> FactsOf(string path) => Entry(path)?.Facts ?? [];

    private IReadOnlyList<KbCard> CardsOf(string root, string path, bool isContribution, Dictionary<string, KbReplacement> subst)
    {
        var entry = Entry(path);
        if (entry is null)
            return [];
        var source = Path.GetRelativePath(root, path).Replace('\\', '/');
        return [.. entry.Cards.Select(c =>
        {
            var alerts = KbCodeRules.Alerts(c.New, c.Original);
            var complexity = KbCodeRules.Complexity(alerts, c.Original, c.New);
            subst.TryGetValue(c.Id, out var s);
            if (s is not null)
                alerts.Insert(0, $"Solução substituída por {s.ReplacedBy} em {s.Date}" + (string.IsNullOrEmpty(s.Reason) ? "" : $": {s.Reason}"));
            return new KbCard(c.Id, c.Occurrences, c.Pattern, c.Original, c.New, source, isContribution, alerts, complexity, s?.ReplacedBy);
        })];
    }

    [GeneratedRegex(@"^\|\s*\[(P\d+)\]\([^)]*\)\s*\|\s*([^|]*?)\s*\|\s*(\d+)\s*\|\s*(\d+)\s*\|\s*$")]
    private static partial Regex PatternRow();

    [GeneratedRegex(@"CALL\s+FUNCTION\s+'([A-Z0-9_/]+)'")]
    private static partial Regex CallFunction();

    [GeneratedRegex(@"TYPE\s+(?:TABLE\s+OF\s+|STANDARD\s+TABLE\s+OF\s+)?(BAPI[A-Z0-9_]+)")]
    private static partial Regex BapiStructure();

    private sealed record FileEntry(long Ticks, long Length, IReadOnlyList<RawCard> Cards, string? Heading, IReadOnlyList<KbFact> Facts, KbSubstitutions? Substitutions)
    {
        public bool Matches(FileInfo info) => Ticks == info.LastWriteTimeUtc.Ticks && Length == info.Length;
    }
}
