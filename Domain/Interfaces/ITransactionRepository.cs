using FinData.Domain.Entities;

namespace FinData.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default);

    void Remove(Transaction transaction);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}