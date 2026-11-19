// Implementa o acesso aos lançamentos pelo EF Core.
using FinData.Application.Interfaces;
using FinData.Domain.Entities;
using FinData.Domain.Interfaces;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinDataDbContext _context;
    private readonly ICurrentUser _currentUser;

    // Recebe o contexto do banco e o serviço que identifica a conta autenticada.
    public TransactionRepository(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // Filtra também o proprietário para não disponibilizar lançamentos de outra conta.
    public async Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;

        return await _context.Transactions.FirstOrDefaultAsync(
            transaction => transaction.Id == id &&
                           transaction.UserId == userId,
            cancellationToken);
    }

    // Adiciona ao contexto, sem confirmar a gravação ainda.
    public async Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(
            transaction,
            cancellationToken);
    }

    // Marca a entidade para remoção; a exclusão será confirmada em SaveChangesAsync.
    public void Remove(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
    }

    // Envia ao banco as alterações acompanhadas pelo contexto.
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}