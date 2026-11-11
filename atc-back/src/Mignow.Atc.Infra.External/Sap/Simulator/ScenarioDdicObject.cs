namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Objeto DDIC do cenário: tipo (STRU|TABL|FUNC|DTEL), pacote e fonte (DDL ou definição), request aberta e se está inativo.</summary>
public sealed record ScenarioDdicObject(string Name, string Kind, string Source, string Package = "ZPACK", string? OpenRequest = null, bool Inactive = false);
