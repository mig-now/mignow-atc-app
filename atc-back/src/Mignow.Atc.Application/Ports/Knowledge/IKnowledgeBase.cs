namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>
/// KB markdown <c>KB_ATC</c> compartilhada com as skills (decisões 4, 5 e 16). Leitura com cache por arquivo (relê quando
/// o mtime/tamanho muda); escrita uma por vez, com trava de arquivo, relendo antes de numerar <c>C-</c>/<c>F-</c>;
/// cartões existentes nunca são reescritos. Falhas de escrita/categoria lançam <see cref="KbException"/>.
/// Implementação: <c>Infra.External/Knowledge</c> (módulo KB).
/// </summary>
public interface IKnowledgeBase
{
    /// <summary>Porte de <c>kb_lookup(cfg, ref, robt, top=3)</c>.</summary>
    Task<KbLookupResult> LookupAsync(string referencedObject, string? refObjectType, int top, CancellationToken ct);

    /// <summary>Porte de <c>fatos_de</c>: fatos do elemento (<c>contribuicoes/fatos/&lt;ELEM&gt;.md</c>).</summary>
    Task<IReadOnlyList<KbFact>> FactsAsync(string element, CancellationToken ct);

    /// <summary>Registra a contribuição e devolve o id do cartão (<c>C-&lt;ELEM&gt;-NNN</c>).</summary>
    Task<string> RegisterAsync(KbContribution contribution, CancellationToken ct);

    /// <summary>Registra o fato e devolve os ids (<c>F-&lt;ELEM&gt;-NNN</c>), um por elemento.</summary>
    Task<IReadOnlyList<string>> AddFactAsync(KbFactInput fact, CancellationToken ct);

    /// <summary>Índice por elemento (varre <c>elementos/</c>, <c>contribuicoes/</c> e <c>fatos/</c>).</summary>
    Task<IReadOnlyList<KbIndexEntry>> BrowseAsync(CancellationToken ct);

    /// <summary>Índice por padrão (<c>indices/por_padrao.md</c>).</summary>
    Task<IReadOnlyList<KbPatternEntry>> BrowsePatternsAsync(CancellationToken ct);

    /// <summary>Elemento completo; <paramref name="element"/> aceita o nome ou o slug. Null se não existir.</summary>
    Task<KbElementDetail?> GetElementAsync(string category, string element, CancellationToken ct);

    /// <summary>Cache DDIC (<c>ddic/</c>), só leitura: lista.</summary>
    Task<IReadOnlyList<KbDdicEntry>> ListDdicAsync(CancellationToken ct);

    /// <summary>Cache DDIC: fonte ABAP de um item (null se não existir).</summary>
    Task<string?> GetDdicAsync(string kind, string name, CancellationToken ct);

    /// <summary>Porte de <c>cmd_ddic_lista</c>.</summary>
    Task<KbDdicCandidates> DdicCandidatesAsync(int top, CancellationToken ct);

    /// <summary>Descarta o índice em memória (próxima leitura relê a pasta).</summary>
    Task ReloadAsync(CancellationToken ct);
}
