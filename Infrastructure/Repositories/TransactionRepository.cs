using FinData.Domain.Entities;
using FinData.Domain.Interfaces;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinDataDbContext _context;

    public TransactionRepository(FinDataDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(
                transaction => transaction.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(
            transaction,
            cancellationToken);
    }

    public void Remove(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}