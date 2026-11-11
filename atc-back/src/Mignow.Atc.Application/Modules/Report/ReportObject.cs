namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Um objeto (tipo + nome) com a soma das soluções dos seus findings: unidade da versão por objetos do relatório.</summary>
public sealed record ReportObject(string Obj, string Type, int Count, int Kb, int Ai, int Exemption, int Pending, string Category, IReadOnlyList<string> Requests,
    bool ExemptionRequested, string Checks);
