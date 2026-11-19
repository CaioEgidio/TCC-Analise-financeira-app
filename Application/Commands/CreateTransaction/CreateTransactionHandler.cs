// Coordena a validação e a gravação de um novo lançamento.
using FinData.Application.Interfaces;
using FinData.Domain.Entities;
using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.CreateTransaction;

public class CreateTransactionHandler
{
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUser _currentUser;

    // Recebe os serviços necessários pela injeção de dependência.
    public CreateTransactionHandler(
        ITransactionRepository transactions,
        ICategoryRepository categories,
        ICurrentUser currentUser)
    {
        _transactions = transactions;
        _categories = categories;
        _currentUser = currentUser;
    }

    // Executa a validação e a gravação de um novo lançamento.
    public async Task<Guid> HandleAsync(
        CreateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction = new Transaction(
            _currentUser.Id,
            command.Description,
            command.Amount,
            command.Date,
            command.Type,
            command.CategoryId);

        // Confirma que a categoria existe e pertence ao usuário atual antes de gravar.
        var categoryExists = await _categories.ExistsAsync(
            command.CategoryId,
            cancellationToken);

        if (!categoryExists)
            throw new ArgumentException(
                "A categoria não foi encontrada para este usuário.",
                nameof(command.CategoryId));

        await _transactions.AddAsync(transaction, cancellationToken);
        await _transactions.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}