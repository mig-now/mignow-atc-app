namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Política do cenário para o módulo Exemption (chave <c>exemption</c> do JSON, opcional). Padrões = sistema que aceita
/// isenções: política <c>A</c>, serviço publicado, motivos FPOS/OTHR, validades U/D/P e um aprovador. Findings não listados
/// em <see cref="Findings"/> recebem modelo normal (escopo <c>thisFinding</c> habilitado).
/// </summary>
public sealed record ExemptionScenario
{
    /// <summary>EXEMPTION.POLICY; nulo = chave ausente (nenhum resultado pode ser isento).</summary>
    public string? Policy { get; init; } = "A";

    public bool ApplyPublished { get; init; } = true;

    public IReadOnlyList<ScenarioExemptionReason> Reasons { get; init; } =
    [
        new() { Id = "FPOS", JustificationMandatory = false, Title = "False Positive" },
        new() { Id = "OTHR", JustificationMandatory = true, Title = "Other Reason" },
    ];

    public IReadOnlyList<string> Validities { get; init; } = ["U = Unlimited", "D = Until date", "P = Until next release"];

    /// <summary>Aprovadores cadastrados (SATC_CI_APPROVER); vazio com política A = aviso, não bloqueio.</summary>
    public IReadOnlyList<string> Approvers { get; init; } = ["APPROVER1"];

    public IReadOnlyList<string> Areas { get; init; } = [];

    /// <summary>Findings com comportamento próprio (sem escopo, bloqueado). Os demais usam o modelo normal.</summary>
    public IReadOnlyList<ScenarioExemptionFinding> Findings { get; init; } = [];

    /// <summary>Totais de <c>checkexemptionsview</c> fixos (a skill viu 0 mesmo com pedidos); nulo = conta os pedidos do simulador.</summary>
    public int? FixedUserTotal { get; init; }
}

public sealed record ScenarioExemptionReason
{
    public string Id { get; init; } = string.Empty;

    public bool JustificationMandatory { get; init; }

    public string Title { get; init; } = string.Empty;
}

/// <summary>Finding do cenário: tipo ADT do objeto (<c>AQSG</c>…), checksum (<c>-1</c> = sem escopo) e mensagem de bloqueio do SAP.</summary>
public sealed record ScenarioExemptionFinding
{
    public string MarkerId { get; init; } = string.Empty;

    public string ObjectName { get; init; } = "ZPROG";

    public string Type { get; init; } = "PROG/P";

    public string Checksum { get; init; } = "123456";

    /// <summary>Sem o modelo: o SAP responde só com um status (ex.: <c>Exemptions disabled by system setup</c>).</summary>
    public string? BlockMessage { get; init; }
}
