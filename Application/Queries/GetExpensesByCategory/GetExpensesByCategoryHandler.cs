// Agrupa as despesas do usuário por categoria no período selecionado.
using FinData.Application.Interfaces;
using FinData.Domain.Entities;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetExpensesByCategory;

public class GetExpensesByCategoryHandler
{
    private readonly FinDataDbContext _context;
    private readonly ICurrentUser _currentUser;

    // Recebe os serviços necessários pela injeção de dependência.
    public GetExpensesByCategoryHandler(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // Agrupa as despesas do usuário por categoria no período selecionado.
    public async Task<List<ExpensesByCategoryDto>> HandleAsync(
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
                transaction.Type == TransactionType.Expense &&
                transaction.Date >= firstDay &&
                transaction.Date <= lastDay)
            // Soma apenas despesas e mantém separados o identificador e o nome de cada categoria.
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

        // Converte os resultados agregados para o formato de resposta da API.
        return totals
            .Select(category => new ExpensesByCategoryDto(
                category.CategoryId,
                category.CategoryName,
                category.Total))
            .ToList();
    }
}