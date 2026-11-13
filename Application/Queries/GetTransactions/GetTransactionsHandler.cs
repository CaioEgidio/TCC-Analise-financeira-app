using FinData.Application.Interfaces;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetTransactions;

public class GetTransactionsHandler
{
    private readonly FinDataDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetTransactionsHandler(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TransactionDto>> HandleAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (year < 1 || year > 9999)
            throw new ArgumentException(
                "O ano deve estar entre 1 e 9999.");

        if (month < 1 || month > 12)
            throw new ArgumentException(
                "O mês deve estar entre 1 e 12.");

        var userId = _currentUser.Id;

        var firstDay = new DateOnly(year, month, 1);
        var lastDay = new DateOnly(
            year, month, DateTime.DaysInMonth(year, month));

        return await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date >= firstDay &&
                transaction.Date <= lastDay)
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.CreatedAt)
            .Select(transaction => new TransactionDto(
                transaction.Id,
                transaction.Description,
                transaction.Amount,
                transaction.Date,
                transaction.Type,
                transaction.CategoryId,
                transaction.Category!.Name))
            .ToListAsync(cancellationToken);
    }
}