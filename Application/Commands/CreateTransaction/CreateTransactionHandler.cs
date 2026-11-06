using FinData.Domain.Entities;
using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.CreateTransaction;

public class CreateTransactionHandler
{
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;

    public CreateTransactionHandler(
        ITransactionRepository transactions,
        ICategoryRepository categories)
    {
        _transactions = transactions;
        _categories = categories;
    }

    public async Task<Guid> HandleAsync(
        CreateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction = new Transaction(
            command.Description,
            command.Amount,
            command.Date,
            command.Type,
            command.CategoryId);

        var categoryExists = await _categories.ExistsAsync(
            command.CategoryId,
            cancellationToken);

        if (!categoryExists)
            throw new ArgumentException(
                "A categoria informada não existe.",
                nameof(command.CategoryId));

        await _transactions.AddAsync(transaction, cancellationToken);
        await _transactions.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}