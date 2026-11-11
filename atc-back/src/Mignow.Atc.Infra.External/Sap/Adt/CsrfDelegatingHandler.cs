namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Token CSRF da sessão ADT: busca no <c>/discovery</c> antes do primeiro POST/PUT e, num 403 com
/// <c>x-csrf-token: Required</c>, busca de novo e repete <b>uma</b> vez.
/// </summary>
internal sealed class CsrfDelegatingHandler(AdtSession session) : DelegatingHandler
{
    public const string Header = "x-csrf-token";

    /// <summary>Uma busca de token por vez na sessão (chamadas paralelas na sessão de leitura compartilhada).</summary>
    private readonly SemaphoreSlim _fetch = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Method == HttpMethod.Get)
            return Capture(await base.SendAsync(request, ct));

        var body = request.Content is null ? null : await request.Content.ReadAsByteArrayAsync(ct);
        var contentType = request.Content?.Headers.ContentType;
        if (session.CsrfToken is null)
            await FetchOnceAsync(null, ct);
        request.Headers.Remove(Header);
        request.Headers.TryAddWithoutValidation(Header, session.CsrfToken);
        var response = Capture(await base.SendAsync(request, ct));
        if (!IsTokenRequired(response))
            return response;

        var rejected = request.Headers.TryGetValues(Header, out var sent) ? sent.FirstOrDefault() : null;
        response.Dispose();
        await FetchOnceAsync(rejected, ct);
        using var retry = Clone(request, body, contentType);
        retry.Headers.Remove(Header);
        retry.Headers.TryAddWithoutValidation(Header, session.CsrfToken);
        return Capture(await base.SendAsync(retry, ct));
    }

    /// <summary>Busca o token se ainda não há um, ou se o atual é o que acabou de ser recusado (outra chamada pode já ter renovado).</summary>
    private async Task FetchOnceAsync(string? rejected, CancellationToken ct)
    {
        await _fetch.WaitAsync(ct);
        try
        {
            if (session.CsrfToken is null || session.CsrfToken == rejected)
                await FetchAsync(ct);
        }
        finally
        {
            _fetch.Release();
        }
    }

    private async Task FetchAsync(CancellationToken ct)
    {
        using var fetch = new HttpRequestMessage(HttpMethod.Get, session.Url("/sap/bc/adt/discovery"));
        fetch.Headers.TryAddWithoutValidation(Header, "fetch");
        // Validado no MI9 (projeto anterior): sem Accept o /discovery devolve 400.
        fetch.Headers.TryAddWithoutValidation("Accept", "application/atomsvc+xml");
        session.ApplyCommonHeaders(fetch, stateful: false);
        using var response = Capture(await base.SendAsync(fetch, ct));
    }

    private HttpResponseMessage Capture(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(Header, out var values))
        {
            var token = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(token) && !string.Equals(token, "Required", StringComparison.OrdinalIgnoreCase))
                session.CsrfToken = token;
        }
        return response;
    }

    private static bool IsTokenRequired(HttpResponseMessage response) =>
        response.StatusCode == System.Net.HttpStatusCode.Forbidden &&
        response.Headers.TryGetValues(Header, out var values) &&
        values.Any(v => string.Equals(v, "Required", StringComparison.OrdinalIgnoreCase));

    private static HttpRequestMessage Clone(HttpRequestMessage original, byte[]? body, System.Net.Http.Headers.MediaTypeHeaderValue? contentType)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        foreach (var h in original.Headers)
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
        if (body is not null)
        {
            clone.Content = new ByteArrayContent(body);
            clone.Content.Headers.ContentType = contentType;
        }
        return clone;
    }
}
