// Coordena a exclusão permanente de um lançamento da conta autenticada.
using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.DeleteTransaction;

public class DeleteTransactionHandler
{
    private readonly ITransactionRepository _repository;

    // Recebe os serviços necessários pela injeção de dependência.
    public DeleteTransactionHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    // Executa a exclusão permanente de um lançamento da conta autenticada.
    public async Task<bool> HandleAsync(
        DeleteTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (transaction is null)
            return false;

        // Marca o registro para exclusão; SaveChangesAsync confirma a remoção no banco.
        _repository.Remove(transaction);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}