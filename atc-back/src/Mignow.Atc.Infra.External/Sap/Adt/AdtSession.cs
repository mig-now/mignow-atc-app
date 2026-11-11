using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Mignow.Atc.Application.Ports.Sap;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Sessão ADT de um ambiente: cookies, token CSRF e <c>sap-client</c>. LOCK → PUT → UNLOCK usam a mesma sessão
/// (stateful); leitura e SQL vão stateless. A senha só vai no cabeçalho Basic, nunca em log.
/// </summary>
public sealed class AdtSession
{
    public const string AdtRoot = "/sap/bc/adt";

    /// <summary>Marca a requisição que usa <c>AdtOptions.LongTimeoutSeconds</c> em vez do timeout padrão (lido pelo handler de resiliência).</summary>
    public static readonly HttpRequestOptionsKey<bool> LongTimeout = new("mignow.adt.long-timeout");

    private readonly HttpClient _client;
    private readonly string _root;
    private readonly string _sapClient;
    private readonly AuthenticationHeaderValue _authorization;

    internal AdtSession(SapConnection connection, HttpMessageHandler pooledHandler)
    {
        _root = NormalizeRoot(connection.BaseUrl);
        _sapClient = connection.Client;
        Host = new Uri(_root).Host;
        CertificatePinning.Register(Host, connection.CertThumbprint, connection.TrustOnFirstUse);
        _authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{connection.User}:{connection.Password}")));
        var chain = new CsrfDelegatingHandler(this) { InnerHandler = new AdtCookieHandler(new CookieContainer()) { InnerHandler = pooledHandler } };
        _client = new HttpClient(chain, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
    }

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    public string Host { get; }

    internal string? CsrfToken { get; set; }

    /// <summary><c>http(s)://host:porta</c>, sem o <c>/sap/bc/adt</c> (aceito também com ele).</summary>
    public static string NormalizeRoot(string baseUrl)
    {
        var root = baseUrl.Trim().TrimEnd('/');
        return root.EndsWith(AdtRoot, StringComparison.OrdinalIgnoreCase) ? root[..^AdtRoot.Length] : root;
    }

    /// <summary>URL absoluta com o <c>sap-client</c>. <paramref name="pathAndQuery"/> começa em <c>/sap/...</c>.</summary>
    internal Uri Url(string pathAndQuery)
    {
        var separator = pathAndQuery.Contains('?') ? '&' : '?';
        return new Uri($"{_root}{pathAndQuery}{separator}sap-client={Uri.EscapeDataString(_sapClient)}");
    }

    internal void ApplyCommonHeaders(HttpRequestMessage request, bool stateful)
    {
        request.Headers.Authorization = _authorization;
        request.Headers.TryAddWithoutValidation("X-sap-adt-sessiontype", stateful ? "stateful" : "stateless");
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string pathAndQuery, CancellationToken ct, HttpContent? content = null, bool stateful = false,
        string? accept = null, IReadOnlyDictionary<string, string>? headers = null, bool longTimeout = false)
    {
        using var request = new HttpRequestMessage(method, Url(pathAndQuery)) { Content = content };
        if (longTimeout)
            request.Options.Set(LongTimeout, true);
        ApplyCommonHeaders(request, stateful);
        if (accept is not null)
            request.Headers.TryAddWithoutValidation("Accept", accept);
        foreach (var (key, value) in headers ?? new Dictionary<string, string>())
            request.Headers.TryAddWithoutValidation(key, value);
        return await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
    }
}
