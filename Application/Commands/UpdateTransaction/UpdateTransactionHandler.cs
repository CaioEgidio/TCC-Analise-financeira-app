// Coordena a edição de um lançamento pertencente à conta autenticada.
using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.UpdateTransaction;

public class UpdateTransactionHandler
{
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;

    // Recebe os serviços necessários pela injeção de dependência.
    public UpdateTransactionHandler(
        ITransactionRepository transactions,
        ICategoryRepository categories)
    {
        _transactions = transactions;
        _categories = categories;
    }

    // Executa a edição de um lançamento pertencente à conta autenticada.
    public async Task<bool> HandleAsync(
        Guid id,
        UpdateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        // O repositório procura pelo identificador e pelo proprietário ao mesmo tempo.
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

        // Reutiliza as regras da entidade para validar os novos valores.
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