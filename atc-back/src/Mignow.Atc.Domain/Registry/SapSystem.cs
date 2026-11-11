using System.Text.RegularExpressions;
using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Registry;

/// <summary>
/// Ambiente SAP (equivale ao <c>config.json</c> do cliente nas skills). Pertence a um cliente. A senha nunca fica aqui:
/// só a referência ao cofre (<see cref="PasswordSecretRef"/>). <see cref="Host"/> = <c>sim://cenario</c> aponta para o
/// simulador (só Development e testes). <see cref="IsProduction"/> bloqueia toda escrita (trava do <c>ISapWriter</c>).
/// </summary>
public sealed partial class SapSystem : EntityBase
{
    public const string SimulatorScheme = "sim://";

    private SapSystem() { }

    public int CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public string Sid { get; private set; } = string.Empty;

    /// <summary>Mandante (3 dígitos).</summary>
    public string Client { get; private set; } = string.Empty;

    /// <summary>Endereço visto pela VPN (ex.: 10.99.80.8) ou <c>sim://cenario</c>.</summary>
    public string Host { get; private set; } = string.Empty;

    public int AdtPort { get; private set; }

    public bool Https { get; private set; }

    public string Language { get; private set; } = "EN";

    /// <summary>Thumbprint do certificado fixado (SAP com certificado autoassinado), gravado no primeiro teste de conexão.</summary>
    public string? CertThumbprint { get; private set; }

    public bool IsProduction { get; private set; }

    public ProjectType ProjectType { get; private set; }

    /// <summary>Autor dos marcadores (<c>AUT-XX</c>).</summary>
    public string Author { get; private set; } = string.Empty;

    public string? DefaultRequest { get; private set; }

    public string? ExemptionApprover { get; private set; }

    public string Username { get; private set; } = string.Empty;

    public string? PasswordSecretRef { get; private set; }

    public bool Active { get; private set; } = true;

    public string? Release { get; private set; }

    /// <summary>T000-CCCATEGORY lida no último teste de conexão (P = produção).</summary>
    public string? ClientCategory { get; private set; }

    public DateTimeOffset? LastConnectionTestAt { get; private set; }

    public bool? LastConnectionTestOk { get; private set; }

    public string? LastConnectionTestMessage { get; private set; }

    public bool IsSimulator => Host.StartsWith(SimulatorScheme, StringComparison.OrdinalIgnoreCase);

    /// <summary>URL base do ADT (<c>http(s)://host:porta</c>) ou o próprio <c>sim://cenario</c>.</summary>
    public string BaseUrl => IsSimulator ? Host : $"{(Https ? "https" : "http")}://{Host}:{AdtPort}";

    public string DisplayName => $"{Sid}/{Client}";

    public static SapSystem Create(Customer customer, string sid, string client, string host, int adtPort, bool https, string language,
        string? certThumbprint, bool isProduction, ProjectType projectType, string author, string? defaultRequest, string? exemptionApprover,
        string username)
    {
        var system = new SapSystem { Customer = customer, CustomerId = customer.Id };
        system.Update(sid, client, host, adtPort, https, language, certThumbprint, isProduction, projectType, author, defaultRequest,
            exemptionApprover, username, true);
        return system;
    }

    public void Update(string sid, string client, string host, int adtPort, bool https, string language, string? certThumbprint,
        bool isProduction, ProjectType projectType, string author, string? defaultRequest, string? exemptionApprover, string username, bool active)
    {
        var h = Guard.Required(host, "SapSystem.HostRequired", 200).TrimEnd('/');
        var simulator = h.StartsWith(SimulatorScheme, StringComparison.OrdinalIgnoreCase);
        if (!simulator && h.Contains("://", StringComparison.Ordinal))
            throw new DomainException("SapSystem.InvalidHost", "Informe só o host (sem http:// ou https://).");
        if (!simulator && adtPort is < 1 or > 65535)
            throw new DomainException("SapSystem.InvalidPort", "Porta ADT entre 1 e 65535.");
        var sapClient = Guard.Required(client, "SapSystem.ClientRequired", 3);
        if (sapClient.Length != 3 || !sapClient.All(char.IsDigit))
            throw new DomainException("SapSystem.InvalidClient", "O mandante tem 3 dígitos.");
        var aut = Guard.Required(author, "SapSystem.AuthorRequired", 20).ToUpperInvariant();
        if (!AuthorPattern().IsMatch(aut))
            throw new DomainException("SapSystem.InvalidAuthor", "O autor tem o formato AUT-XX.");
        Sid = Guard.Required(sid, "SapSystem.SidRequired", 3).ToUpperInvariant();
        Client = sapClient;
        Host = simulator ? h.ToLowerInvariant() : h;
        AdtPort = simulator ? 0 : adtPort;
        Https = !simulator && https;
        Language = Guard.Required(language, "SapSystem.LanguageRequired", 2).ToUpperInvariant();
        CertThumbprint = Guard.Optional(certThumbprint, 95)?.Replace(":", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
        IsProduction = isProduction;
        ProjectType = projectType;
        Author = aut;
        DefaultRequest = Guard.Optional(defaultRequest, 20)?.ToUpperInvariant();
        ExemptionApprover = Guard.Optional(exemptionApprover, 40)?.ToUpperInvariant();
        Username = Guard.Required(username, "SapSystem.UserRequired", 40).ToUpperInvariant();
        Active = active;
    }

    public void SetPasswordSecret(string secretRef) => PasswordSecretRef = secretRef;

    /// <summary>
    /// Resultado do teste de conexão. Sem certificado fixado, o thumbprint visto no teste passa a ser o fixado (como o
    /// <c>testar_conexao</c> da skill grava no config).
    /// </summary>
    public void RecordConnectionTest(DateTimeOffset at, bool ok, string? message, string? release, string? clientCategory, string? seenThumbprint)
    {
        LastConnectionTestAt = at;
        LastConnectionTestOk = ok;
        LastConnectionTestMessage = Guard.Optional(message, 500);
        if (!ok)
            return;
        if (release is not null)
            Release = Guard.Optional(release, 120);
        ClientCategory = Guard.Optional(clientCategory, 1);
        if (CertThumbprint is null && Https && !string.IsNullOrWhiteSpace(seenThumbprint))
            CertThumbprint = seenThumbprint.Trim().ToUpperInvariant();
    }

    [GeneratedRegex("^AUT-[A-Z0-9]{1,16}$")]
    private static partial Regex AuthorPattern();
}
