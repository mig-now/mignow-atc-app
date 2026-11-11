using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Tests.Integration;

namespace Mignow.Atc.Tests.Contracts;

/// <summary>
/// Códigos de passo e de mensagem que o front traduz. <c>openapi/step-codes.json</c> = {código do passo: [tipos de execução]} e
/// <c>openapi/message-codes.json</c> = {código: texto pt-BR} saem daqui (regerar com <c>ATC_WRITE_OPENAPI=1 dotnet test --filter Category=Contract</c>);
/// sem a variável o teste só confere que os arquivos estão em dia. Também garante que todo código de mensagem usado no código-fonte tem
/// tradução nos 3 idiomas.
/// </summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "Contract")]
public sealed partial class CodesContractTests(AtcApiFactory api)
{
    private static readonly string[] Cultures = ["", ".pt-BR", ".es-ES"];
    private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static string BackRoot([CallerFilePath] string path = "") => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, "..", "..", ".."));

    [Fact]
    public void Todos_os_passos_de_todos_os_pipelines_estao_em_step_codes_json()
    {
        var catalog = api.Services.GetRequiredService<RunPipelineCatalog>();
        using var scope = api.Services.CreateScope();
        var map = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (var kind in catalog.Kinds)
            foreach (var code in catalog.StepCodes(kind, scope.ServiceProvider))
            {
                if (!map.TryGetValue(code, out var kinds))
                    map[code] = kinds = new SortedSet<string>(StringComparer.Ordinal);
                kinds.Add(kind.ToString());
            }
        map.Should().NotBeEmpty();
        map.Keys.Should().Contain(["TEST_CONNECTION", "EXTRACTION_EXTRACT", "SCAN_LOTS", "EXEMPTION_SEND"]);
        Sync("step-codes.json", JsonSerializer.Serialize(map, Pretty));
    }

    [Fact]
    public void Todo_codigo_de_mensagem_do_codigo_fonte_tem_traducao_nos_tres_idiomas()
    {
        var resources = Path.Combine(BackRoot(), "src", "Mignow.Atc.Application", "Resources");
        var perCulture = Cultures.Select(c => Directory.GetFiles(resources, $"*Errors{c}.resx")
            .Where(f => c.Length > 0 || !Regex.IsMatch(Path.GetFileName(f), @"\.[a-z]{2}-[A-Z]{2}\.resx$"))
            .SelectMany(f => XDocument.Load(f).Descendants("data").Select(d => (Key: d.Attribute("name")!.Value, Value: d.Element("value")!.Value)))
            .GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.First().Value)).ToList();
        for (var i = 1; i < perCulture.Count; i++)
            perCulture[i].Keys.Order().Should().Equal(perCulture[0].Keys.Order(), $"idioma {Cultures[i]} precisa ter as mesmas chaves do padrão");

        var prefixes = perCulture[0].Keys.Select(k => k.Split('.')[0]).ToHashSet();
        var used = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in Directory.GetFiles(Path.Combine(BackRoot(), "src"), "*.cs", SearchOption.AllDirectories)
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                                 && !f.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}")))
            foreach (var line in File.ReadLines(file).Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal)))
                foreach (Match m in CodeLiteral().Matches(line))
                    if (prefixes.Contains(m.Groups[1].Value.Split('.')[0]))
                        used.TryAdd(m.Groups[1].Value, Path.GetFileName(file));
        used.Where(u => !perCulture[0].ContainsKey(u.Key)).Select(u => $"{u.Key} ({u.Value})").Should().BeEmpty("código usado no código-fonte sem tradução");
        used.Should().NotBeEmpty();

        Sync("message-codes.json", JsonSerializer.Serialize(new SortedDictionary<string, string>(perCulture[1], StringComparer.Ordinal), Pretty));
    }

    private static void Sync(string fileName, string json)
    {
        var path = Path.Combine(BackRoot(), "openapi", fileName);
        json += "\n";
        if (Environment.GetEnvironmentVariable("ATC_WRITE_OPENAPI") == "1")
            File.WriteAllText(path, json);
        else
            File.ReadAllText(path).Should().Be(json, $"{fileName} desatualizado: rode ATC_WRITE_OPENAPI=1 dotnet test --filter Category=Contract");
    }

    [GeneratedRegex("\"([A-Z][A-Za-z0-9]*(?:\\.[A-Za-z0-9_]+)+)\"")]
    private static partial Regex CodeLiteral();
}
