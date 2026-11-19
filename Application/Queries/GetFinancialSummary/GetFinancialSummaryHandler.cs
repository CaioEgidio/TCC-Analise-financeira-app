// Calcula os totais financeiros do mês para a conta autenticada.
using FinData.Application.Interfaces;
using FinData.Domain.Entities;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetFinancialSummary;

public class GetFinancialSummaryHandler
{
    private readonly FinDataDbContext _context;
    private readonly ICurrentUser _currentUser;

    // Recebe os serviços necessários pela injeção de dependência.
    public GetFinancialSummaryHandler(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // Calcula os totais financeiros do mês para a conta autenticada.
    public async Task<FinancialSummaryDto> HandleAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        // Valida os parâmetros antes de calcular o intervalo e consultar o banco.
        if (year < 1 || year > 9999)
            throw new ArgumentException(
                "O ano deve estar entre 1 e 9999.");

        if (month < 1 || month > 12)
            throw new ArgumentException(
                "O mês deve estar entre 1 e 12.");

        var userId = _currentUser.Id;

        // Inclui desde o primeiro até o último dia do mês, pela data da movimentação.
        var firstDay = new DateOnly(year, month, 1);
        var lastDay = new DateOnly(
            year, month, DateTime.DaysInMonth(year, month));

        var totals = await _context.Transactions
            // A consulta é somente leitura; o EF não precisa acompanhar mudanças nas entidades.
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date >= firstDay &&
                transaction.Date <= lastDay)
            // O banco soma os valores por tipo de movimentação, sem carregar cada lançamento no frontend.
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

        // Saldo do mês = receitas - despesas; não acumula resultados de meses anteriores.
        return new FinancialSummaryDto(
            year,
            month,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense);
    }
}