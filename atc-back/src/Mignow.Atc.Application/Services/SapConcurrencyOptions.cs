namespace Mignow.Atc.Application.Services;

/// <summary>
/// Limite de chamadas simultâneas ao ADT por ambiente SAP (seção <c>Sap:Adt</c>, chave <c>MaxConcurrencyPerSystem</c>; a mesma lida pelo
/// <c>AdtThrottle</c>). Cada processo paralelo da varredura MIGNOW segura UM espaço até terminar, então o paralelismo da varredura precisa caber
/// no limite, deixando <see cref="ReservedSlots"/> livre para as demais chamadas do ambiente (leitura, cancelamento, limpeza).
/// </summary>
public sealed class SapConcurrencyOptions
{
    public const string Section = "Sap:Adt";
    public const int DefaultMaxConcurrencyPerSystem = 10;
    public const int ReservedSlots = 1;

    public int MaxConcurrencyPerSystem { get; set; } = DefaultMaxConcurrencyPerSystem;

    /// <summary>Maior paralelismo de varredura que não espera espaço: o limite menos o reservado (nunca abaixo de 1).</summary>
    public int MaxMignowParallel => Math.Max(1, MaxConcurrencyPerSystem - ReservedSlots);
}
