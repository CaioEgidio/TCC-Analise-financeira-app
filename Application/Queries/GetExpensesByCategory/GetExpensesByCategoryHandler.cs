using FinData.Domain.Entities;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetExpensesByCategory;

public class GetExpensesByCategoryHandler
{
    private readonly FinDataDbContext _context;

    public GetExpensesByCategoryHandler(FinDataDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExpensesByCategoryDto>> HandleAsync(
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

        var firstDay = new DateOnly(year, month, 1);

        var lastDay = new DateOnly(
            year,
            month,
            DateTime.DaysInMonth(year, month));

        var totals = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.Type == TransactionType.Expense &&
                transaction.Date >= firstDay &&
                transaction.Date <= lastDay)
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                CategoryName = transaction.Category!.Name
            })
            .Select(group => new
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.CategoryName,
                Total = group.Sum(transaction => transaction.Amount)
            })
            .OrderByDescending(category => category.Total)
            .ThenBy(category => category.CategoryName)
            .ToListAsync(cancellationToken);

        return totals
            .Select(category => new ExpensesByCategoryDto(
                category.CategoryId,
                category.CategoryName,
                category.Total))
            .ToList();
    }
}