using FinData.Domain.Entities;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetFinancialSummary;

public class GetFinancialSummaryHandler
{
    private readonly FinDataDbContext _context;

    public GetFinancialSummaryHandler(FinDataDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialSummaryDto> HandleAsync(
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
                transaction.Date >= firstDay &&
                transaction.Date <= lastDay)
            .GroupBy(transaction => transaction.Type)
            .Select(group => new
            {
                Type = group.Key,
                Total = group.Sum(transaction => transaction.Amount)
            })
            .ToListAsync(cancellationToken);

        var totalIncome = totals
            .Where(total => total.Type == TransactionType.Income)
            .Sum(total => total.Total);

        var totalExpense = totals
            .Where(total => total.Type == TransactionType.Expense)
            .Sum(total => total.Total);

        return new FinancialSummaryDto(
            year,
            month,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense);
    }
}