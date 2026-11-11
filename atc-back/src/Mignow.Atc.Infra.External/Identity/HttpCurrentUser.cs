using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Ports.Identity;

namespace Mignow.Atc.Infra.External.Identity;

/// <summary>
/// Usuário da requisição: e-mail/nome dos claims do Entra (preferred_username, email, upn). Sem requisição (worker), o
/// ator é "system". Em desenvolvimento sem Entra, o usuário de desenvolvimento (<c>dev@migatc.local</c>).
/// </summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor, IOptions<DevAuthOptions> dev) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    private bool DevMode => dev.Value.Enabled && accessor.HttpContext is not null && Principal?.Identity?.IsAuthenticated != true;

    public bool IsAuthenticated => DevMode || Principal?.Identity?.IsAuthenticated == true;

    public string? Email => DevMode
        ? dev.Value.Email
        : Principal?.FindFirst("preferred_username")?.Value ?? Principal?.FindFirst(ClaimTypes.Email)?.Value ?? Principal?.FindFirst("email")?.Value
          ?? Principal?.FindFirst("upn")?.Value;

    public string? Name => DevMode ? dev.Value.Name : Principal?.FindFirst("name")?.Value;

    public string Actor => accessor.HttpContext is null
        ? "system"
        : Email?.ToLowerInvariant() ?? Principal?.FindFirst("oid")?.Value ?? "anonymous";
}
