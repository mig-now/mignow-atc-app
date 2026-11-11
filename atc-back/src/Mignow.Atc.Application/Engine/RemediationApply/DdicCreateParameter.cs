namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Criação DDIC de um item cujo objeto ainda não existe no sistema (pacote e descrição, como <c>-Pacote</c>/<c>-Descricao</c>).</summary>
public sealed record DdicCreateParameter(Guid ItemId, string Package, string Description);
