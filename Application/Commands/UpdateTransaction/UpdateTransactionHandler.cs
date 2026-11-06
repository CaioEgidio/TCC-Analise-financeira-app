using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.UpdateTransaction;

public class UpdateTransactionHandler
{
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;

    public UpdateTransactionHandler(
        ITransactionRepository transactions,
        ICategoryRepository categories)
    {
        _transactions = transactions;
        _categories = categories;
    }

    public async Task<bool> HandleAsync(
        Guid id,
        UpdateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactions.GetByIdAsync(
            id,
            cancellationToken);

        if (transaction is null)
            return false;

        var categoryExists = await _categories.ExistsAsync(
            command.CategoryId,
            cancellationToken);

        if (!categoryExists)
            throw new ArgumentException(
                "A categoria informada não existe.",
                nameof(command.CategoryId));

        transaction.Update(
            command.Description,
            command.Amount,
            command.Date,
            command.Type,
            command.CategoryId);

        await _transactions.SaveChangesAsync(cancellationToken);

        return true;
    }
}