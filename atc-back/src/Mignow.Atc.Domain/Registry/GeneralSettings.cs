using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Registry;

/// <summary>Parâmetros gerais (uma linha só). Padrões = os das skills (desenho §3.1).</summary>
public sealed class GeneralSettings : EntityBase
{
    public const int MaxParallelism = 16;
    public const int MaxMignowParallel = 9;
    public const string DefaultKbPath = "../.claude/skills/atc-remediacao/KB_ATC";
    public const string DefaultMignowRulesPath = "../.claude/skills/atc-extracao-mignow/regras/regras.json";

    private GeneralSettings() { }

    /// <summary>Tarefas independentes em paralelo (agentes, preparo de grupos).</summary>
    public int Parallelism { get; private set; } = 8;

    /// <summary>Leitura de fontes em paralelo.</summary>
    public int SourceReadParallelism { get; private set; } = 8;

    public int BatchSize { get; private set; } = 40;

    /// <summary>Pasta da KB (relativa à raiz do repositório ou absoluta) – a mesma usada pelas skills (decisão 16).</summary>
    public string KbPath { get; private set; } = DefaultKbPath;

    public string MignowRulesPath { get; private set; } = DefaultMignowRulesPath;

    public int MignowParallel { get; private set; } = 3;

    public int MignowWeightPerBatch { get; private set; } = 1000;

    /// <summary>Relatório final: objetos corrigidos por dia no processo manual.</summary>
    public int ManualObjectsPerDay { get; private set; } = 64;

    public int HoursPerDay { get; private set; } = 8;

    public static GeneralSettings CreateDefault() => new();

    public void Update(int parallelism, int sourceReadParallelism, int batchSize, string kbPath, string mignowRulesPath, int mignowParallel,
        int mignowWeightPerBatch, int manualObjectsPerDay, int hoursPerDay)
    {
        if (parallelism is < 1 or > MaxParallelism || sourceReadParallelism is < 1 or > MaxParallelism)
            throw new DomainException("Settings.InvalidParallelism", $"Paralelismo entre 1 e {MaxParallelism}.");
        if (mignowParallel is < 1 or > MaxMignowParallel)
            throw new DomainException("Settings.InvalidMignowParallel", $"Paralelismo da varredura entre 1 e {MaxMignowParallel}.");
        if (batchSize < 1 || mignowWeightPerBatch < 1 || manualObjectsPerDay < 1 || hoursPerDay is < 1 or > 24)
            throw new DomainException("Settings.InvalidNumber", "Valores numéricos fora da faixa.");
        Parallelism = parallelism;
        SourceReadParallelism = sourceReadParallelism;
        BatchSize = batchSize;
        KbPath = Guard.Required(kbPath, "Settings.KbPathRequired", 500);
        MignowRulesPath = Guard.Required(mignowRulesPath, "Settings.MignowRulesPathRequired", 500);
        MignowParallel = mignowParallel;
        MignowWeightPerBatch = mignowWeightPerBatch;
        ManualObjectsPerDay = manualObjectsPerDay;
        HoursPerDay = hoursPerDay;
    }
}
