namespace Mignow.Atc.Application.Ports.Identity;

/// <summary>
/// Usuário da requisição (Entra fora de Dev; <c>dev@migatc.local</c> em Dev sem Entra). Só existe o papel Consultor =
/// qualquer autenticado (decisão 20). Ator de auditoria = e-mail (ou <c>system</c> no worker).
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string Actor { get; }

    string? Email { get; }

    string? Name { get; }
}
