// Consulta os lançamentos do usuário em um mês e ano específicos.
using FinData.Application.Interfaces;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetTransactions;

public class GetTransactionsHandler
{
    private readonly FinDataDbContext _context;
    private readonly ICurrentUser _currentUser;

    // Recebe os serviços necessários pela injeção de dependência.
    public GetTransactionsHandler(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // Consulta os lançamentos do usuário em um mês e ano específicos.
    public async Task<List<TransactionDto>> HandleAsync(
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

        return await _context.Transactions
            // A consulta é somente leitura; o EF não precisa acompanhar mudanças nas entidades.
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date >= firstDay &&
                transaction.Date <= lastDay)
            // Mostra primeiro as movimentações mais recentes; a criação desempata datas iguais.
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