// Permite obter o identificador da conta autenticada sem acessar HTTP nos handlers.
namespace FinData.Application.Interfaces;

public interface ICurrentUser
{
    Guid Id { get; }
}