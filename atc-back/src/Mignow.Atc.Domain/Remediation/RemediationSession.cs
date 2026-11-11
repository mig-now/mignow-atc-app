using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Domain.Remediation;

/// <summary>
/// Sessão de remediação (= <c>sessao.json</c> da skill atc-remediacao) de um ambiente. Reimportar arquiva a anterior
/// (<see cref="Archived"/>, como o <c>backup_sessao_*</c>) e a nova é o ciclo seguinte (<see cref="Cycle"/>).
/// </summary>
public sealed class RemediationSession : EntityBase
{
    private readonly List<RemediationItem> _items = [];

    private RemediationSession() { }

    /// <summary>
    /// Itens adicionados na criação (navegação só para gravar sessão e itens num SaveChanges único; as consultas leem os
    /// itens pelo <c>SessionId</c>, sem carregar esta coleção).
    /// </summary>
    public IReadOnlyCollection<RemediationItem> Items => _items;

    public int CustomerId { get; private set; }

    public int SapSystemId { get; private set; }

    public RemediationSource Source { get; private set; }

    /// <summary>Extração, nome do CSV ou varredura de origem.</summary>
    public string? SourceRef { get; private set; }

    /// <summary>"1" | "1,2" | "1,2,3" (ou "todas").</summary>
    public string Priorities { get; private set; } = string.Empty;

    public string? Request { get; private set; }

    public string Author { get; private set; } = string.Empty;

    public ProjectType ProjectType { get; private set; }

    /// <summary>Cabeçalho do CSV de origem (export reproduz o CSV + coluna de status).</summary>
    public string CsvColumnsJson { get; private set; } = "[]";

    public string CsvEncoding { get; private set; } = "utf-8-sig";

    public string CsvDelimiter { get; private set; } = ";";

    public int Cycle { get; private set; } = 1;

    public bool Archived { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>"Modo sem confirmação" da skill.</summary>
    public bool AutoMode { get; private set; }

    /// <summary>
    /// Linhas do CSV de origem que não viraram item (fora do escopo de prioridades ou em branco): <c>[{"n":3,"row":["…"]}]</c>. O export reproduz o
    /// CSV original inteiro (como o <c>cmd_exportar</c>, que relê o <c>atc_original.csv</c>), com o status vazio nessas linhas.
    /// </summary>
    public string ExcludedRowsJson { get; private set; } = "[]";

    public static RemediationSession Create(int customerId, int sapSystemId, RemediationSource source, string? sourceRef, string priorities,
        string? request, string author, ProjectType projectType, string csvColumnsJson, string csvEncoding, string csvDelimiter, int cycle)
    {
        if (customerId <= 0 || sapSystemId <= 0)
            throw new DomainException("Session.SapSystemRequired", "A sessão precisa de cliente e ambiente.");
        if (cycle < 1)
            throw new DomainException("Session.InvalidCycle", "Ciclo a partir de 1.");
        return new RemediationSession
        {
            CustomerId = customerId,
            SapSystemId = sapSystemId,
            Source = source,
            SourceRef = Guard.Optional(sourceRef, 300),
            Priorities = Guard.Required(priorities, "Session.PrioritiesRequired", 20),
            Request = Guard.Optional(request, 20)?.ToUpperInvariant(),
            Author = Guard.Required(author, "Session.AuthorRequired", 20).ToUpperInvariant(),
            ProjectType = projectType,
            CsvColumnsJson = string.IsNullOrWhiteSpace(csvColumnsJson) ? "[]" : csvColumnsJson,
            CsvEncoding = Guard.Required(csvEncoding, "Session.EncodingRequired", 20),
            CsvDelimiter = string.IsNullOrEmpty(csvDelimiter) ? ";" : csvDelimiter,
            Cycle = cycle,
        };
    }

    /// <summary>Acrescenta os itens importados (o EF resolve o <c>SessionId</c> ao gravar).</summary>
    public void AddItems(IEnumerable<RemediationItem> items) => _items.AddRange(items);

    public void SetExcludedRows(string json) => ExcludedRowsJson = string.IsNullOrWhiteSpace(json) ? "[]" : json;

    public void SetRequest(string? request) => Request = Guard.Optional(request, 20)?.ToUpperInvariant();

    public void SetAutoMode(bool autoMode) => AutoMode = autoMode;

    public void Archive(DateTimeOffset at)
    {
        if (Archived)
            return;
        Archived = true;
        ArchivedAt = at;
    }
}
