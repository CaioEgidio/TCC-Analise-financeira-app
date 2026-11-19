// Define as operações de persistência de lançamentos usadas pela aplicação.
using FinData.Domain.Entities;

namespace FinData.Domain.Interfaces;

public interface ITransactionRepository
{
    // Obtém o lançamento da conta atual; retorna nulo quando ele não está acessível.
    Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // Adiciona o novo lançamento ao contexto de persistência.
    Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default);

    // Marca o lançamento para exclusão permanente.
    void Remove(Transaction transaction);

    // Confirma inclusões, alterações e exclusões pendentes no banco.
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}