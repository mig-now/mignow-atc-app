namespace Mignow.Atc.Application.Dtos.Registry;

/// <summary>Usuário logado. Só existe o papel Consultor (qualquer autenticado).</summary>
public sealed record MeResponse(string? Email, string? Name, string Role);
