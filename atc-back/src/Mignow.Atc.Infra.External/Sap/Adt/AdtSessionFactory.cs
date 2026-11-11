using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Ports.Sap;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Sessões ADT. Leitura/SQL: uma sessão stateless reaproveitada por ambiente (cookies e CSRF), renovada depois de alguns
/// minutos. Gravação: sessão nova e exclusiva (stateful), para o lock não se misturar com outra chamada.
/// </summary>
public sealed class AdtSessionFactory(IHttpMessageHandlerFactory handlers, IOptions<AdtOptions> options)
{
    public const string ClientName = "adt";

    private readonly ConcurrentDictionary<string, AdtSession> _readSessions = new();

    public AdtSession ForRead(SapConnection connection)
    {
        var key = Key(connection);
        var ttl = TimeSpan.FromMinutes(options.Value.ReadSessionMinutes);
        return _readSessions.AddOrUpdate(key, _ => Create(connection), (_, s) => DateTimeOffset.UtcNow - s.CreatedAt > ttl ? Create(connection) : s);
    }

    internal int ReadSessionCount => _readSessions.Count;

    public AdtSession ForWrite(SapConnection connection) => Create(connection);

    private AdtSession Create(SapConnection connection) => new(connection, handlers.CreateHandler(ClientName));

    /// <summary>Chave da sessão: ambiente, mandante, usuário, certificado e um hash da senha (trocar a senha abre sessão nova).</summary>
    private static string Key(SapConnection c) =>
        $"{c.SystemId}|{AdtSession.NormalizeRoot(c.BaseUrl)}|{c.Client}|{c.User}|{c.CertThumbprint}|{c.TrustOnFirstUse}|{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(c.Password)))}";
}
