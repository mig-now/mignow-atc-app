using System.Net;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>Cookies da sessão ADT (o handler primário é compartilhado e não guarda cookies).</summary>
internal sealed class AdtCookieHandler(CookieContainer cookies) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var uri = request.RequestUri!;
        var header = cookies.GetCookieHeader(uri);
        if (header.Length > 0)
            request.Headers.TryAddWithoutValidation("Cookie", header);
        var response = await base.SendAsync(request, ct);
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
            foreach (var value in values)
                cookies.SetCookies(uri, value);
        return response;
    }
}
