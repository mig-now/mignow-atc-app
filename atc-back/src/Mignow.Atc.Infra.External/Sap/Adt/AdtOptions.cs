namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>Seção <c>Sap:Adt</c>.</summary>
public sealed class AdtOptions
{
    public const string Section = "Sap:Adt";

    /// <summary>
    /// Chamadas simultâneas por ambiente SAP. Cada processo paralelo da varredura MIGNOW segura um espaço até o fim, por isso o padrão (10) comporta
    /// o máximo de 9 processos e ainda deixa 1 livre para as outras chamadas; o PUT de configurações recusa um paralelismo da varredura acima de
    /// <c>MaxConcurrencyPerSystem - 1</c>.
    /// </summary>
    public int MaxConcurrencyPerSystem { get; set; } = Mignow.Atc.Application.Services.SapConcurrencyOptions.DefaultMaxConcurrencyPerSystem;

    /// <summary>Tempo de cada chamada (a skill usa 180 s).</summary>
    public int TimeoutSeconds { get; set; } = 180;

    /// <summary>
    /// Tempo da chamada de execução do lote da varredura (classrun da classe gerada, que lê dezenas/centenas de fontes). Só essa chamada usa este
    /// valor (900 s); as demais seguem <see cref="TimeoutSeconds"/>.
    /// </summary>
    public int LongTimeoutSeconds { get; set; } = 900;

    public int ConnectTimeoutSeconds { get; set; } = 10;

    public int IdleConnectionSeconds { get; set; } = 30;

    /// <summary>Espera base das novas tentativas (só leitura GET, até 2; nunca PUT/POST).</summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>Tempo de vida da sessão de leitura reaproveitada (cookies e CSRF).</summary>
    public int ReadSessionMinutes { get; set; } = 10;

    /// <summary>A categoria do mandante (T000) lida pela trava de escrita fica em cache por este tempo.</summary>
    public int ClientCategoryCacheSeconds { get; set; } = 60;
}
